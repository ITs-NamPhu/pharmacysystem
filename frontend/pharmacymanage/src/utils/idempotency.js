import { v4 as uuidv4 } from "uuid";

export const MUTATING_METHODS = ['post', 'put', 'delete', 'patch'];

// Chỉ các API này mới gắn Idempotency-Key (tránh tạo trùng / sai lệch tiền - kho).
export const ListURL_Idempotency = [
    'api/Invoice',
    'api/Receipt',
    'api/GoodsReceipt',
    'api/StockTake',
    'api/StockAdjustment',
    'api/DestroyReceipt',
    'api/User',
    'api/Branch',
    'api/Customer',
    'api/Manufacturer',
    'api/Supplier',
    'api/Unit',
    'api/Medicine',
    'api/UnitConversion',
];

// Key ngẫu nhiên cho mỗi lần submit.
// Khi axios retry (vd refresh token), key cũ được giữ nên không bị tạo trùng.
export const newIdempotencyKey = () => uuidv4();
