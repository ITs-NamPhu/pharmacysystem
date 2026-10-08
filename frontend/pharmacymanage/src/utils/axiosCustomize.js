import axios from "axios";
import nProgress from "nprogress";
import { store } from '../redux/store';
import { postRefreshToken } from "../services/apiService";
import { doRefreshToken } from "../redux/action/userAction";
import { computeIdempotencyKey, MUTATING_METHODS } from "./idempotency";



// tạo hiệu ứng khi gửi request
nProgress.configure({
    showSpinner: false,
    trickleSpeed: 100
})


const refresh_token = async () => {
    const accessToken = store?.getState().user.account.access_token;
    const refreshToken = store?.getState().user.account.refresh_token;
    let data = { accessToken, refreshToken }
    return await postRefreshToken(data);
}


const instance = axios.create({
    baseURL: process.env.REACT_APP_API_URL
    // || 'https://localhost:7196'
})



// Add a request interceptor
instance.interceptors.request.use(function (config) {
    if (!config.url.includes("api/auth/refreshtoken")) {
        const access_token = store?.getState().user.account.access_token;
        config.headers["Authorization"] = "Bearer " + access_token;

        const currentBranchId = store?.getState().user.account.currentBranchId;
        if (currentBranchId) {
            config.headers["X-Branch-Id"] = currentBranchId;
        }
    }

    const method = (config.method || 'get').toLowerCase();
    if (MUTATING_METHODS.includes(method) && !config.headers['Idempotency-Key']) {
        config.headers['Idempotency-Key'] = computeIdempotencyKey(config);
    }

    nProgress.start();
    return config;
}, function (error) {
    return Promise.reject(error);
});



// handle for refresh token
// Case: nếu request bị lỗi 401 (Unauthorized) for access token hết hạn
//      và chưa retry lần nào thì sẽ gọi API refresh token để lấy access token mới,
//      sau đó retry lại request cũ với access token mới.

let isRefreshing = false;
let refreshSubscribers = [];

// duyệt qua tất cả request đang chờ refresh token
// và gọi lại với access token mới
function onRefreshed(accessToken) {
    refreshSubscribers.forEach(callback => callback(accessToken));
    refreshSubscribers = [];
}

function addRefreshSubscriber(callback) {
    refreshSubscribers.push(callback);
}

// Add a response interceptor
instance.interceptors.response.use(function (response) {
    nProgress.done();
    return response && response.data ? response.data : response;
}, async function (error) {

    nProgress.done();

    const originalRequest = error.config;

    if (error.response?.data?.ec === -999 && !originalRequest._retry) {
        if (isRefreshing) {
            return new Promise((resolve) => {
                addRefreshSubscriber((accessToken) => {
                    originalRequest.headers["Authorization"] = "Bearer " + accessToken;
                    originalRequest._retry = true;
                    resolve(instance(originalRequest));
                });
            });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        // nếu gọi refresh token thành công thì thực hiện lại request cũ với access token mới
        // không thì reject lỗi reset lại queue refreshSubscribers và isRefreshing
        try {
            const res = await refresh_token();
            if (res && res.ec === 0) {
                store.dispatch(doRefreshToken(res));
                const accessToken = res.dt.accessToken;

                isRefreshing = false;
                onRefreshed(accessToken);

                originalRequest.headers["Authorization"] = "Bearer " + accessToken;
                return instance(originalRequest);
            } else {
                isRefreshing = false;
                refreshSubscribers = [];
                return Promise.reject(error);
            }
        } catch (err) {
            isRefreshing = false;
            refreshSubscribers = [];
            return Promise.reject(err);
        }
    }

    return error?.response?.data
        ? error.response.data : Promise.reject(error);
});

export default instance;
