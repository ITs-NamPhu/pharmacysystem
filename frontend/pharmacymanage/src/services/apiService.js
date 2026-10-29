import instance, { refreshInstance } from '../utils/axiosCustomize';

// module api user
const getUserbyBranch = (page, limitPage) => {
    return instance.get(`api/User/GetAllActive`,
        {
            params: { page: page, count: limitPage }
        })
}
const CreateUser = (email, password, username, fullname, phone, address) => {
    return instance.post(`api/User`, { UserName: username, Password: password, FullName: fullname, Phone: phone, Address: address, Email: email })
}
const deleteUser = (userID) => {
    return instance.delete(`api/User`, { params: { userID: userID } })
}

const UpdateUser = (userID, FullName, Phone, Address, Email) => {
    return instance.put(`api/User`, { FullName, Phone, Address, Email }, {
        params: { userID }
    })
}

const getAllRole = () => {
    return instance.get(`api/Role/All`)
}

const getAllBranch = () => {
    return instance.get(`api/Branch/All`)
}

const putAssign_RoleBranch = (userID, RoleId) => {
    return instance.put(`api/User/${userID}/assignment`, { RoleId })
}

// const postCreateUser = (email, password, username, role, image) => {
//     const data = new FormData();
//     data.append('email', email);
//     data.append('password', password);
//     data.append('username', username);
//     data.append('role', role);
//     data.append('userImage', image);
//     return instance.post(`api/v1/participant`, data);
// }

// branch
const getAllBranchPag = (page, limitPage) => {
    return instance.get(`api/Branch/GetAll`, {
        params: { page: page, count: limitPage }
    })
}

const CreateBranch = (branchName, phone, address) => {
    return instance.post(`api/Branch`, { BranchName: branchName, Phone: phone, Address: address })
}

const UpdateBranch = (branchID, branchName, phone, address) => {
    return instance.put(`api/Branch`, { BranchName: branchName, Phone: phone, Address: address }, {
        params: { branchID }
    })
}

const DeleteBranch = () => {
    return instance.delete();
}

// customer
const getAllCustomerPag = (page, count) => {
    return instance.get(`api/Customer/GetAll`, {
        params: { page: page, count: count }
    })
}
const CreateCustomer = (customerName, phone, address, customerTypeID) => {
    return instance.post(`api/Customer`, { CustomerName: customerName, Phone: phone, Address: address, CustomerTypeID: customerTypeID })
}
const UpdateCustomer = (customerID, customerName, phone, address, customerTypeID) => {
    return instance.put(`api/Customer/${customerID}`, { CustomerName: customerName, Phone: phone, Address: address, CustomerTypeID: customerTypeID })
}
const DeleteCustomer = (customerID) => {
    return instance.delete(`api/Customer/${customerID}`)
}
const getAllCustomerType = () => {
    return instance.get(`api/CustomerType/All`)
}

const getAllCustomerNoPag = () => {
    return instance.get(`api/Customer/All`)
}

// manufacturer
const getAllManufacturerPag = (page, count) => {
    return instance.get(`api/Manufacturer/GetAll`, {
        params: { page: page, count: count }
    })
}
const CreateManufacturer = (manufacturerName) => {
    return instance.post(`api/Manufacturer`, { ManufacturerName: manufacturerName })
}
const UpdateManufacturer = (manufacturerID, manufacturerName) => {
    return instance.put(`api/Manufacturer/${manufacturerID}`, { ManufacturerName: manufacturerName })
}
const DeleteManufacturer = (manufacturerID) => {
    return instance.delete(`api/Manufacturer/${manufacturerID}`)
}

const getAllManufacturerAll = () => {
    return instance.get(`api/Manufacturer/All`)
}

// supplier
const getAllSupplierPag = (page, count) => {
    return instance.get(`api/Supplier/GetAll`, {
        params: { page: page, count: count }
    })
}
const CreateSupplier = (supplierName, phone, email, address) => {
    return instance.post(`api/Supplier`, { SupplierName: supplierName, Phone: phone, Email: email, Address: address })
}
const UpdateSupplier = (supplierID, supplierName, phone, email, address) => {
    return instance.put(`api/Supplier/${supplierID}`, { SupplierName: supplierName, Phone: phone, Email: email, Address: address })
}
const DeleteSupplier = (supplierID) => {
    return instance.delete(`api/Supplier/${supplierID}`)
}

const getAllSupplierNoPag = () => {
    return instance.get(`api/Supplier/All`)
}
// unit
const getAllUnitPag = (page, count) => {
    return instance.get(`api/Unit/GetAll`, {
        params: { page: page, count: count }
    })
}
const GetAllUnitNoPag = () => {
    return instance.get(`api/Unit/All`)
}
const CreateUnit = (unitName) => {
    return instance.post(`api/Unit`, { UnitName: unitName })
}
const UpdateUnit = (unitID, unitName) => {
    return instance.put(`api/Unit/${unitID}`, { UnitName: unitName })
}
const DeleteUnit = (unitID) => {
    return instance.delete(`api/Unit/${unitID}`)
}



// medicine category (all)
const getAllMedicineCategoryAll = () => {
    return instance.get(`api/MedicineCategory/All`)
}

// medicine
const getAllMedicinePag = (page, count) => {
    return instance.get(`api/Medicine/GetAll`, {
        params: { page: page, count: count }
    })
}

const GetMedicineById = (medicineID) => {
    return instance.get(`api/Medicine/${medicineID}`)
}

const CreateMedicine = (medicineName, defaultRetailPrice, defaultWholesalePrice, vatPercent, categoryID, manufacturerID, baseUnitID) => {
    return instance.post(`api/Medicine`, {
        MedicineName: medicineName,
        DefaultRetailPrice: defaultRetailPrice,
        DefaultWholesalePrice: defaultWholesalePrice,
        VATPercent: vatPercent,
        CategoryID: categoryID,
        ManufacturerID: manufacturerID,
        BaseUnitID: baseUnitID
    })
}
const UpdateMedicine = (medicineID, medicineName, defaultRetailPrice, defaultWholesalePrice, vatPercent, categoryID, manufacturerID, baseUnitID) => {
    return instance.put(`api/Medicine/${medicineID}`, {
        MedicineName: medicineName,
        DefaultRetailPrice: defaultRetailPrice,
        DefaultWholesalePrice: defaultWholesalePrice,
        VATPercent: vatPercent,
        CategoryID: categoryID,
        ManufacturerID: manufacturerID,
        BaseUnitID: baseUnitID
    })
}
const DeleteMedicine = (medicineID) => {
    return instance.delete(`api/Medicine/${medicineID}`)
}

const getAllMedicineNoPag = () => {
    return instance.get(`api/Medicine/All`)
}

// warehouse - batches
const getAllBatchInWarehouse = (page, count) => {
    return instance.get(`api/Warehouse/batches`, {
        params: { page: page, count: count }
    })
}

const getWarehouseByBranch = (branchId, page, count) => {
    return instance.get(`api/Warehouse/ByBranch/${branchId}`, {
        params: { page: page, count: count }
    })
}

// unit conversion
const CreateUnitConversion = (unitID, medicineID, factor) => {
    return instance.post(`api/UnitConversion`, {
        UnitID: unitID,
        MedicineID: medicineID,
        Factor: factor
    })
}
const DeleteUnitConversion = (unitConversionID) => {
    return instance.delete(`api/UnitConversion/${unitConversionID}`)
}
const GetAllUnitConversion = () => {
    return instance.get(`api/UnitConversion/All`)
}

const getAllUnitConversion_Medicine = () => {
    return instance.get(`api/UnitConversion/GetList`)
}

// goods receipt
const getAllGoodsReceiptPag = (page, count) => {
    return instance.get(`api/GoodsReceipt/GetAll`, {
        params: { page: page, count: count }
    })
}
const GetGoodsReceiptById = (goodsReceiptID) => {
    return instance.get(`api/GoodsReceipt/${goodsReceiptID}`)
}
const CreateGoodsReceipt = (supplierID, note, paidAmount, receiptDate, items) => {
    return instance.post(`api/GoodsReceipt`, {
        SupplierID: supplierID,
        Note: note,
        PaidAmount: paidAmount,
        ReceiptDate: receiptDate,
        Items: items
    })
}
const UpdateGoodsReceipt = (goodsReceiptID, supplierID, note, paidAmount, receiptDate, items) => {
    return instance.put(`api/GoodsReceipt/${goodsReceiptID}`, {
        SupplierID: supplierID,
        Note: note,
        PaidAmount: paidAmount,
        ReceiptDate: receiptDate,
        Items: items
    })
}
const DeleteGoodsReceipt = (goodsReceiptID) => {
    return instance.delete(`api/GoodsReceipt/${goodsReceiptID}`)
}

// invoice
const getAllInvoicePag = (page, count) => {
    return instance.get(`api/Invoice/GetAll`, {
        params: { page: page, count: count }
    })
}
const GetInvoiceById = (invoiceID) => {
    return instance.get(`api/Invoice/${invoiceID}`)
}
const CreateInvoice = (customerID, note, createdByUserID, invoiceItems, mode, paidAmount, useWalletAmount) => {
    const payload = {
        CustomerID: customerID,
        Note: note,
        CreatedByUserID: createdByUserID,
        Mode: mode,
        InvoiceItems: invoiceItems
    };
    if (paidAmount != null && paidAmount > 0) payload.PaidAmount = +paidAmount;
    if (useWalletAmount != null && useWalletAmount > 0) payload.UseWalletAmount = +useWalletAmount;

    return instance.post(`api/Invoice`, payload)
}
const UpdateInvoice = (invoiceID, customerID, note, createdByUserID, invoiceItems, mode) => {
    return instance.put(`api/Invoice/${invoiceID}`, {
        CustomerID: customerID,
        Note: note,
        CreatedByUserID: createdByUserID,
        Mode: mode,
        InvoiceItems: invoiceItems
    })
}
const DeleteInvoice = (invoiceID) => {
    return instance.delete(`api/Invoice/${invoiceID}`)
}
const GetBatchesByMedicine = (medicineID) => {
    return instance.get(`api/Invoice/BatchesByMedicine`, {
        params: { medicineID: medicineID }
    })
}
const GetFefoBatches = (medicineID, quantity) => {
    return instance.get(`api/Invoice/FefoBatches`, {
        params: { medicineID: medicineID, quantity: quantity }
    })
}
const GetUsersByBranch = () => {
    return instance.get(`api/Invoice/UsersByBranch`)
}

// receipt (phiếu thu)
const getAllReceiptPag = (page, count, customerId) => {
    const params = { page: page, count: count };
    if (customerId) params.customerId = customerId;
    return instance.get(`api/Receipt/GetAll`, { params })
}
const GetReceiptById = (receiptID) => {
    return instance.get(`api/Receipt/${receiptID}`)
}
const CreateReceipt = (customerID, totalAmount) => {
    return instance.post(`api/Receipt`, { CustomerID: customerID, TotalAmount: totalAmount })
}

// customer wallet
const GetCustomerWalletById = (customerID) => {
    return instance.get(`api/Customer/${customerID}/Wallet`)
}

const postLogin = (username, userpassword, delay) => {
    return instance.post(`api/auth/login`, { userName: username, password: userpassword, delay: 1000 });
}
const getListQuizByUser = () => {
    return instance.get(`api/v1/quiz-by-participant`)
}
const getQuestionsByQuizID = (QuizId) => {
    return instance.get(`api/v1/questions-by-quiz?quizId=${QuizId}`);
}
const postSubmitQuiz = (data) => {
    return instance.post(`api/v1/quiz-submit`, { ...data })
}
const postCreateQuiz = (description, username, difficulty, quizImage) => {
    const data = new FormData();
    data.append('description', description);
    data.append('name', username);
    data.append('difficulty', difficulty);
    data.append('quizImage', quizImage);
    return instance.post(`api/v1/quiz`, data)
}
const putQuiz = (id, description, name, difficulty, quizImage) => {
    const data = new FormData();
    data.append('id', id);
    data.append('description', description);
    data.append('name', name);
    data.append('difficulty', difficulty);
    data.append('quizImage', quizImage);
    return instance.put(`api/v1/quiz`, data)
}
const DeleteQuiz = (idQuiz) => {
    return instance.delete(`api/v1/quiz/${+idQuiz}`)
}
const getListQuizForAdmin = () => {
    return instance.get(`api/v1/quiz/all`);
}
const postNewQuestion = (quiz_id, description, questionImage) => {
    const data = new FormData();
    data.append('quiz_id', quiz_id);
    data.append('description', description);
    data.append('questionImage', questionImage);
    return instance.post(`api/v1/question`, data);
}
const postNewAnswer = (description, correct_answer, question_id) => {
    return instance.post(`api/v1/answer`, { description, correct_answer, question_id });
}
const postAssignQuizForUser = (quizId, userId) => {
    return instance.post(`api/v1/quiz-assign-to-user`, { quizId, userId });
}
const getQuizWithQA = (quizId) => {
    return instance.get(`api/v1/quiz-with-qa/${quizId}`)
}
const UpdateQA_API = (data) => {
    //console.log(data);
    return instance.post(`api/v1/quiz-upsert-qa`, { ...data })
}
const Logout = (accessToken, refreshToken) => {
    return instance.post(`api/auth/logout`, { accessToken, refreshToken })
}
const getOverView = () => {
    return instance.get(`api/v1/overview`)
}
const postRefreshToken = (data) => {
    return instance.post(`api/auth/refreshtoken`, { accessToken: data.accessToken, refreshToken: data.refreshToken })
}
const postRegister = (email, password, username) => {
    return instance.post(`api/v1/register`, { email, password, username })
}
const updateProfile = (username, image) => {
    const data = new FormData();
    data.append('username', username);
    data.append('userImage', image);
    return instance.post(`api/v1/profile`, data)
}
const updatePassword = (current_password, new_password) => {
    return instance.post(`api/v1/change-password`, { current_password, new_password })
}
const getHistoryQuiz = () => {
    return instance.get(`api/v1/history`)
}
// dashboard
const getTotalRevenue = () => {
    return instance.get(`api/Dashboard/total-revenue`)
}
const getTotalOrders = () => {
    return instance.get(`api/Dashboard/total-orders`)
}
const getRevenueByBranch = () => {
    return instance.get(`api/Dashboard/revenue-by-branch`)
}
const getNewCustomers = () => {
    return instance.get(`api/Dashboard/new-customers`)
}
const getRevenueTrend = (days = 7) => {
    return instance.get(`api/Dashboard/revenue-trend`, { params: { days } })
}
const getTopMedicines = (top = 10) => {
    return instance.get(`api/Dashboard/top-medicines`, { params: { top } })
}
const getExpiringBatches = () => {
    return instance.get(`api/Dashboard/expiring-batches`)
}
const getLowStock = (threshold = 10) => {
    return instance.get(`api/Dashboard/low-stock`, { params: { threshold } })
}
const getDestroyQueue = () => {
    return instance.get(`api/Dashboard/destroy-queue`)
}
const getInventoryCapital = () => {
    return instance.get(`api/Dashboard/inventory-capital`)
}
const getPendingImports = () => {
    return instance.get(`api/Dashboard/pending-imports`)
}
const getStockTransferRequests = () => {
    return instance.get(`api/Dashboard/stock-transfer-requests`)
}
const getPendingStockTransfers = () => {
    return instance.get(`api/Dashboard/pending-stock-transfers`)
}
const getBranchRevenue = () => {
    return instance.get(`api/Dashboard/branch-revenue`)
}
const getBranchOrders = () => {
    return instance.get(`api/Dashboard/branch-orders`)
}
const getOutOfStock = () => {
    return instance.get(`api/Dashboard/out-of-stock`)
}
const getEmployeeProgress = () => {
    return instance.get(`api/Dashboard/employee-progress`)
}
const getShiftRevenue = () => {
    return instance.get(`api/Dashboard/shift-revenue`)
}
const getShiftOrders = () => {
    return instance.get(`api/Dashboard/shift-orders`)
}
const getPromotions = () => {
    return instance.get(`api/Dashboard/promotions`)
}
const getCounterAlerts = () => {
    return instance.get(`api/Dashboard/counter-alerts`)
}


const getAllStockTakePag = (page, count, warehouseId) => {
    return instance.get(`api/StockTake/GetAll`, {
        params: { page: page, count: count, warehouseId: warehouseId || 0 }
    })
}
const GetStockTakeById = (stockTakeID) => {
    return instance.get(`api/StockTake/${stockTakeID}`)
}
const CreateStockTake = (warehouseID, note, items) => {
    return instance.post(`api/StockTake`, {
        WarehouseID: warehouseID,
        Note: note,
        Items: items
    })
}
const UpdateStockTake = (stockTakeID, data) => {
    return instance.put(`api/StockTake/${stockTakeID}`, data)
}
const CompleteStockTake = (stockTakeID, items) => {
    return instance.put(`api/StockTake/${stockTakeID}/complete`, {
        Items: items
    })
}
const CancelStockTake = (stockTakeID) => {
    return instance.put(`api/StockTake/${stockTakeID}/cancel`)
}
const ApproveStockTake = (stockTakeID) => {
    return instance.put(`api/StockTake/${stockTakeID}/approve`)
}
const RejectStockTake = (stockTakeID) => {
    return instance.put(`api/StockTake/${stockTakeID}/reject`)
}
const DeleteStockTake = (stockTakeID) => {
    return instance.delete(`api/StockTake/${stockTakeID}`)
}
const getAllStockAdjustmentPag = (page, count, warehouseId) => {
    return instance.get(`api/StockAdjustment/GetAll`, {
        params: { page: page, count: count, warehouseId: warehouseId || 0 }
    })
}
const GetStockAdjustmentById = (stockAdjustmentID) => {
    return instance.get(`api/StockAdjustment/${stockAdjustmentID}`)
}
const CreateStockAdjustment = (stockTakeID, note, items) => {
    return instance.post(`api/StockAdjustment`, {
        StockTakeID: stockTakeID,
        Note: note,
        Items: items
    })
}
const UpdateStockAdjustment = (stockAdjustmentID, data) => {
    return instance.put(`api/StockAdjustment/${stockAdjustmentID}`, data)
}
const CompleteStockAdjustment = (stockAdjustmentID) => {
    return instance.put(`api/StockAdjustment/${stockAdjustmentID}/complete`)
}
const ApproveStockAdjustment = (stockAdjustmentID) => {
    return instance.put(`api/StockAdjustment/${stockAdjustmentID}/approve`)
}
const RejectStockAdjustment = (stockAdjustmentID) => {
    return instance.put(`api/StockAdjustment/${stockAdjustmentID}/reject`)
}
const DeleteStockAdjustment = (stockAdjustmentID) => {
    return instance.delete(`api/StockAdjustment/${stockAdjustmentID}`)
}
const getAllDestroyReceiptPag = (page, count, warehouseId) => {
    return instance.get(`api/DestroyReceipt/GetAll`, {
        params: { page: page, count: count, warehouseId: warehouseId || 0 }
    })
}
const GetDestroyReceiptById = (destroyReceiptID) => {
    return instance.get(`api/DestroyReceipt/${destroyReceiptID}`)
}
const CreateDestroyReceipt = (warehouseID, stockTakeID, note, items) => {
    return instance.post(`api/DestroyReceipt`, {
        WarehouseID: warehouseID,
        StockTakeID: stockTakeID,
        Note: note,
        Items: items
    })
}
const UpdateDestroyReceipt = (destroyReceiptID, data) => {
    return instance.put(`api/DestroyReceipt/${destroyReceiptID}`, data)
}
const CompleteDestroyReceipt = (destroyReceiptID) => {
    return instance.put(`api/DestroyReceipt/${destroyReceiptID}/complete`)
}
const ApproveDestroyReceipt = (destroyReceiptID) => {
    return instance.put(`api/DestroyReceipt/${destroyReceiptID}/approve`)
}
const RejectDestroyReceipt = (destroyReceiptID) => {
    return instance.put(`api/DestroyReceipt/${destroyReceiptID}/reject`)
}
const DeleteDestroyReceipt = (destroyReceiptID) => {
    return instance.delete(`api/DestroyReceipt/${destroyReceiptID}`)
}
const ApproveGoodsReceipt = (goodsReceiptID) => {
    return instance.put(`api/GoodsReceipt/${goodsReceiptID}/approve`)
}
const RejectGoodsReceipt = (goodsReceiptID) => {
    return instance.put(`api/GoodsReceipt/${goodsReceiptID}/reject`)
}
const CompleteGoodsReceipt = (goodsReceiptID) => {
    return instance.put(`api/GoodsReceipt/${goodsReceiptID}/complete`)
}

const cleanFilterParams = (params) => {
    const result = {};
    for (const [key, value] of Object.entries(params)) {
        if (value !== null && value !== undefined && value !== '' && value !== 'All' && value !== 0) {
            result[key] = value;
        }
    }
    return result;
};

const getMedicineFiltered = (filters) => {
    const params = cleanFilterParams({
        Keyword: filters.keyword,
        PageNumber: filters.pageNumber,
        PageSize: filters.pageSize,
        SortBy: filters.sortBy,
        IsDescending: filters.isDescending,
        CategoryID: filters.categoryID,
        ManufacturerID: filters.manufacturerID,
        StockStatus: filters.stockStatus,
        StockThreshold: filters.stockThreshold,
    });
    return instance.get(`api/Medicine/GetAll`, { params });
};

const getBatchFiltered = (filters) => {
    const params = cleanFilterParams({
        Keyword: filters.keyword,
        PageNumber: filters.pageNumber,
        PageSize: filters.pageSize,
        SortBy: filters.sortBy,
        IsDescending: filters.isDescending,
        ExpiryStatus: filters.expiryStatus,
        WarehouseID: filters.warehouseID,
    });
    return instance.get(`api/Warehouse/batches`, { params });
};

const getInvoiceFiltered = (filters) => {
    const params = cleanFilterParams({
        Keyword: filters.keyword,
        PageNumber: filters.pageNumber,
        PageSize: filters.pageSize,
        SortBy: filters.sortBy,
        IsDescending: filters.isDescending,
        FromDate: filters.fromDate,
        ToDate: filters.toDate,
        CustomerID: filters.customerID,
        UserID: filters.userID,
        MinTotalAmount: filters.minTotalAmount,
        MaxTotalAmount: filters.maxTotalAmount,
    });
    return instance.get(`api/Invoice/GetAll`, { params });
};

const getGoodsReceiptFiltered = (filters) => {
    const params = cleanFilterParams({
        Keyword: filters.keyword,
        PageNumber: filters.pageNumber,
        PageSize: filters.pageSize,
        SortBy: filters.sortBy,
        IsDescending: filters.isDescending,
        FromDate: filters.fromDate,
        ToDate: filters.toDate,
        SupplierID: filters.supplierID,
        MinTotalAmount: filters.minTotalAmount,
        MaxTotalAmount: filters.maxTotalAmount,
    });
    return instance.get(`api/GoodsReceipt/GetAll`, { params });
};

export {
    postLogin, Logout, postRefreshToken,
    getAllRole, getAllBranch, putAssign_RoleBranch,
    getUserbyBranch, deleteUser, UpdateUser, CreateUser,

    getAllBranchPag, UpdateBranch, DeleteBranch, CreateBranch,

    getAllCustomerPag, CreateCustomer, UpdateCustomer, DeleteCustomer, getAllCustomerType, getAllCustomerNoPag,
    GetCustomerWalletById,
    getAllManufacturerPag, CreateManufacturer, UpdateManufacturer, DeleteManufacturer,
    getAllSupplierPag, CreateSupplier, UpdateSupplier, DeleteSupplier, getAllSupplierNoPag,
    getAllUnitPag, GetAllUnitNoPag, CreateUnit, UpdateUnit, DeleteUnit,

    getAllManufacturerAll,
    getAllMedicineCategoryAll,
    getAllMedicineNoPag,
    getAllMedicinePag, GetMedicineById, CreateMedicine, UpdateMedicine, DeleteMedicine,
    CreateUnitConversion, DeleteUnitConversion, GetAllUnitConversion,

    getAllInvoicePag, GetInvoiceById, CreateInvoice, UpdateInvoice, DeleteInvoice,
    GetBatchesByMedicine, GetFefoBatches, GetUsersByBranch,
    getAllUnitConversion_Medicine,

    getAllReceiptPag, GetReceiptById, CreateReceipt,

    getAllGoodsReceiptPag, GetGoodsReceiptById, CreateGoodsReceipt, UpdateGoodsReceipt, DeleteGoodsReceipt,

    getAllBatchInWarehouse,
    getWarehouseByBranch,

    getTotalRevenue, getTotalOrders, getRevenueByBranch, getNewCustomers,
    getRevenueTrend, getTopMedicines,
    getExpiringBatches, getLowStock, getDestroyQueue,
    getInventoryCapital, getPendingImports,
    getStockTransferRequests, getPendingStockTransfers,
    getBranchRevenue, getBranchOrders, getOutOfStock, getEmployeeProgress,
    getShiftRevenue, getShiftOrders,
    getPromotions, getCounterAlerts,


    getMedicineFiltered, getBatchFiltered, getInvoiceFiltered, getGoodsReceiptFiltered,

    getAllStockTakePag, GetStockTakeById, CreateStockTake, UpdateStockTake, CompleteStockTake, CancelStockTake, ApproveStockTake, RejectStockTake, DeleteStockTake,

    getAllStockAdjustmentPag, GetStockAdjustmentById, CreateStockAdjustment, UpdateStockAdjustment, CompleteStockAdjustment, ApproveStockAdjustment, RejectStockAdjustment, DeleteStockAdjustment,

    getAllDestroyReceiptPag, GetDestroyReceiptById, CreateDestroyReceipt, UpdateDestroyReceipt, CompleteDestroyReceipt, ApproveDestroyReceipt, RejectDestroyReceipt, DeleteDestroyReceipt,

    ApproveGoodsReceipt, RejectGoodsReceipt, CompleteGoodsReceipt
};