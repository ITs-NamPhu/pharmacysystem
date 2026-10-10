import { useState } from 'react';
import './sidebar.scss'
import { useDispatch } from 'react-redux';
import { useNavigate } from "react-router-dom";
import { Logout, postRefreshToken } from "../../services/apiService";
import { doLogout, doRefreshToken } from '../../redux/action/userAction';
import { useSelector } from "react-redux";
import { toast } from 'react-toastify';
import { canAccess } from '../../config/permissions';

const Sidebar = (props) => {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const account = useSelector(state => state.user.account);
    const roleName = account.roleName;
    const { isCollapsed } = props;

    const [groups, setGroups] = useState({
        overview: true,
        ops: true,
        system: true
    });

    const toggleGroup = (name) => {
        setGroups(prev => ({ ...prev, [name]: !prev[name] }));
    };

    const handleLogout = async () => {
        let refreshToken = account.refresh_token;
        let accessToken = account.access_token;
        let res = await Logout(accessToken, refreshToken);
        if (res && res.ec === 0) {
            toast.success(res.em)
            dispatch(doLogout())
            navigate("/")
        }
    }

    return (
        <>
            <div className="sidebar-header">
                <div className="brand-mark">UsK</div>
                <div className="brand-text">
                    <span className="brand-name">NHATHUOC</span>
                    <span className="brand-sub">OS · Quản lý nhà thuốc</span>
                </div>
            </div>

            <nav className={`nav${isCollapsed ? ' is-collapsed' : ''}`}>
                <div className={`nav-group${!groups.overview ? ' is-collapsed' : ''}`}>
                    <button className="nav-group-toggle" type="button" onClick={() => toggleGroup('overview')} aria-expanded={groups.overview}>
                        <span>Tổng quan</span>
                        <svg className="chevron" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>
                    </button>
                    <div className="nav-group-items">
                        <div className="nav-group-items-inner">
                            <a className="nav-item is-active" href="#">
                                <svg className="nav-icon" viewBox="0 0 24 24" fill="none">
                                    <rect x="3" y="3" width="7" height="9" rx="1.5" stroke="currentColor" strokeWidth="1.8" />
                                    <rect x="14" y="3" width="7" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.8" />
                                    <rect x="14" y="12" width="7" height="9" rx="1.5" stroke="currentColor" strokeWidth="1.8" />
                                    <rect x="3" y="16" width="7" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.8" />
                                </svg>
                                <span onClick={() => navigate('/admin')}>Bảng điều khiển</span>
                            </a>
                            <a className="nav-item" href="#">
                                <svg className="nav-icon" viewBox="0 0 24 24" fill="none">
                                    <path d="M6 9V6a6 6 0 0112 0v3" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
                                    <rect x="4" y="9" width="16" height="12" rx="2" stroke="currentColor" strokeWidth="1.8" />
                                    <path d="M12 13v4M10 15h4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
                                </svg>
                                <span>Bán hàng</span>
                                <span className="badge">12</span>
                            </a>
                        </div>
                    </div>
                </div>

                <div className={`nav-group${!groups.ops ? ' is-collapsed' : ''}`}>
                    <button className="nav-group-toggle" type="button" onClick={() => toggleGroup('ops')} aria-expanded={groups.ops}>
                        <span>Vận hành</span>
                        <svg className="chevron" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>
                    </button>
                    <div className="nav-group-items">
                        <div className="nav-group-items-inner">
                            {canAccess('managewarehouse_medicine', roleName) && (
                                <a className="nav-item" onClick={() => navigate('/admin/managewarehouse_medicine')}>
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M3 7l9-4 9 4-9 4-9-4z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M3 7v10l9 4 9-4V7" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M12 11v10" stroke="currentColor" strokeWidth="1.8" /></svg>
                                    <span>Kho thuốc</span>
                                </a>
                            )}
                            {canAccess('manageinvoice', roleName) && (
                                <a className="nav-item" onClick={() => navigate('/admin/manageinvoice')}>
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M8 3h8l1 4H7l1-4z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M6 7h12l-1 13a2 2 0 01-2 2H9a2 2 0 01-2-2L6 7z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M10 12h4M10 16h4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span>Đơn thuốc</span>
                                </a>
                            )}
                            {canAccess('managereceipt', roleName) && (
                                <a className="nav-item" onClick={() => navigate('/admin/managereceipt')}>
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M12 2l8 4v6c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6l8-4z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M9 12l2 2 4-4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                                    <span>Phiếu thu</span>
                                </a>
                            )}
                            {canAccess('managecustomer', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managecustomer')}>Khách hàng</span>
                                </a>
                            )}
                            {canAccess('managesupply', roleName) && (
                                <a className="nav-item">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managesupply')}>Nhà cung cấp</span>
                                </a>
                            )}
                            {canAccess('managemanufacturer', roleName) && (
                                <a className="nav-item">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managemanufacturer')}>Nhà sản xuất</span>
                                </a>
                            )}
                            {canAccess('manageuser', roleName) && (
                                <a className="nav-item">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/manageuser')}>Người dùng</span>
                                </a>
                            )}
                            {canAccess('managebranch', roleName) && (
                                <a className="nav-item">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managebranch')}>Chi nhánh</span>
                                </a>
                            )}
                            {canAccess('manageunit', roleName) && (
                                <a className="nav-item">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="8" r="3.2" stroke="currentColor" strokeWidth="1.8" /><path d="M5 20c0-3.9 3.13-7 7-7s7 3.1 7 7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/manageunit')}>Đơn vị tính</span>
                                </a>
                            )}
                            {canAccess('managemedicine', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M4 20V10M12 20V4M20 20v-7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managemedicine')}>Thuốc</span>
                                </a>
                            )}
                            {canAccess('managegoodsreceipt', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M4 20V10M12 20V4M20 20v-7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                    <span onClick={() => navigate('/admin/managegoodsreceipt')}>Phiếu nhập kho</span>
                                </a>
                            )}
                            {canAccess('managestocktake', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M9 11l3 3L22 4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                                    <span onClick={() => navigate('/admin/managestocktake')}>Kiểm kê kho</span>
                                </a>
                            )}
                            {canAccess('managestockadjustment', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M3 3h18v18H3z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /><path d="M8 16l4-4 4 4M8 8h8" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                                    <span onClick={() => navigate('/admin/managestockadjustment')}>Điều chỉnh kho</span>
                                </a>
                            )}
                            {canAccess('managedestroy', roleName) && (
                                <a className="nav-item" href="#">
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M3 6h18M8 6V4a1 1 0 011-1h6a1 1 0 011 1v2M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                                    <span onClick={() => navigate('/admin/managedestroy')}>Tiêu hủy thuốc</span>
                                </a>
                            )}
                            <a className="nav-item" href="#">
                                <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M4 20V10M12 20V4M20 20v-7" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" /></svg>
                                <span>Báo cáo</span>
                            </a>
                        </div>
                    </div>
                </div>

                <div className={`nav-group${!groups.system ? ' is-collapsed' : ''}`}>
                    <button className="nav-group-toggle" type="button" onClick={() => toggleGroup('system')} aria-expanded={groups.system}>
                        <span>Hệ thống</span>
                        <svg className="chevron" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>
                    </button>
                    <div className="nav-group-items">
                        <div className="nav-group-items-inner">
                            {canAccess('config', roleName) && (
                                <a className="nav-item" onClick={() => navigate('/admin/config')}>
                                    <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M12 15a3 3 0 100-6 3 3 0 000 6z" stroke="currentColor" strokeWidth="1.8" /><path d="M19.4 15a1.7 1.7 0 00.34 1.87l.06.06a2 2 0 11-2.83 2.83l-.06-.06a1.7 1.7 0 00-1.87-.34 1.7 1.7 0 00-1 1.55V21a2 2 0 01-4 0v-.09a1.7 1.7 0 00-1-1.55 1.7 1.7 0 00-1.87.34l-.06.06a2 2 0 11-2.83-2.83l.06-.06A1.7 1.7 0 004.6 15a1.7 1.7 0 00-1.55-1H3a2 2 0 010-4h.09A1.7 1.7 0 004.6 9a1.7 1.7 0 00-.34-1.87l-.06-.06a2 2 0 112.83-2.83l.06.06A1.7 1.7 0 009 4.6a1.7 1.7 0 001-1.55V3a2 2 0 014 0v.09a1.7 1.7 0 001 1.55 1.7 1.7 0 001.87-.34l.06-.06a2 2 0 112.83 2.83l-.06.06A1.7 1.7 0 0019.4 9c.14.4.5 1.55 1.55 1z" stroke="currentColor" strokeWidth="1.5" /></svg>
                                    <span>Cấu hình</span>
                                </a>
                            )}
                            <a className="nav-item" href="#">
                                <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="1.8" /><path d="M19.4 15a1.7 1.7 0 00.34 1.87l.06.06a2 2 0 11-2.83 2.83l-.06-.06a1.7 1.7 0 00-1.87-.34 1.7 1.7 0 00-1 1.55V21a2 2 0 01-4 0v-.09a1.7 1.7 0 00-1-1.55 1.7 1.7 0 00-1.87.34l-.06.06a2 2 0 11-2.83-2.83l.06-.06A1.7 1.7 0 004.6 15a1.7 1.7 0 00-1.55-1H3a2 2 0 010-4h.09A1.7 1.7 0 004.6 9a1.7 1.7 0 00-.34-1.87l-.06-.06a2 2 0 112.83-2.83l.06.06A1.7 1.7 0 009 4.6a1.7 1.7 0 001-1.55V3a2 2 0 014 0v.09a1.7 1.7 0 001 1.55 1.7 1.7 0 001.87-.34l.06-.06a2 2 0 112.83 2.83l-.06.06A1.7 1.7 0 0019.4 9c.14.4.5 1.55 1.55 1z" stroke="currentColor" strokeWidth="1.5" /></svg>
                                <span>Cài đặt</span>
                            </a>
                            <a className="nav-item" onClick={() => handleLogout()}>
                                <svg className="nav-icon" viewBox="0 0 24 24" fill="none"><path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" /></svg>
                                <span>Đăng xuất</span>
                            </a>
                        </div>
                    </div>
                </div>
            </nav>

            <div className="sidebar-footer">
                <div className="avatar">DH</div>
                <div className="who">
                    <span className="who-name">{account.fullName}</span>
                    <span className="who-role">Quản lý ca</span>
                </div>
            </div>
        </>
    );
}
export default Sidebar;
