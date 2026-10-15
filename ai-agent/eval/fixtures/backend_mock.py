"""Backend mock cho evaluation chạy mode=mock.

Thay vì gọi HTTP thật, ta patch trực tiếp singleton `backend_client` với dữ liệu
fixture xác định => câu trả lời của agent mang tính định lượng ổn định, có thể
kiểm tra số liệu có "bịa" (hallucination) hay không.

Cách dùng:
    async with mock_backend():
        ...  # chạy agent
"""
import contextlib
from datetime import date, timedelta

from app.services.backend_client import backend_client

TODAY = date(2026, 9, 23)

_INVENTORY = [
    {
        "MedicineId": 1, "MedicineName": "Paracetamol 500mg", "UnitName": "Hộp",
        "CategoryName": "Giảm đau", "ManufacturerName": "Hau Giang",
        "BatchID": "PG123", "QuantityInStock": 120, "ExpiryDate": "2027-06-30",
        "Price": 15000, "WarehouseName": "Kho 1", "BranchName": "Hà Nội",
    },
    {
        "MedicineId": 2, "MedicineName": "Amoxicillin 500mg", "UnitName": "Hộp",
        "CategoryName": "Kháng sinh", "ManufacturerName": "Domesco",
        "BatchID": "AM099", "QuantityInStock": 80, "ExpiryDate": "2026-11-20",
        "Price": 22000, "WarehouseName": "Kho 2", "BranchName": "Hà Nội",
    },
    {
        "MedicineId": 3, "MedicineName": "Vitamin C 1000mg", "UnitName": "Lọ",
        "CategoryName": "Vitamin", "ManufacturerName": "Pymepharco",
        "BatchID": "VC055", "QuantityInStock": 5, "ExpiryDate": "2026-12-05",
        "Price": 18000, "WarehouseName": "Kho 1", "BranchName": "Hà Nội",
    },
]

_SALES = [
    {
        "InvoiceId": 123, "CreatedAt": "2026-09-20 10:15", "CustomerName": "Nguyễn Văn An",
        "CustomerPhone": "0912345678", "TotalAmount": 150000, "PaidAmount": 150000,
        "Outstanding": 0, "PaymentStatus": 1, "SellerName": "Trần Thị Bình",
        "HasReturnedItems": False,
    },
    {
        "InvoiceId": 124, "CreatedAt": "2026-09-21 09:30", "CustomerName": "Lê Thị Hoa",
        "CustomerPhone": "0987654321", "TotalAmount": 6000000, "PaidAmount": 2000000,
        "Outstanding": 4000000, "PaymentStatus": 0, "SellerName": "Trần Thị Bình",
        "HasReturnedItems": False,
    },
    {
        "InvoiceId": 125, "CreatedAt": "2026-09-22 14:00", "CustomerName": "Phạm Văn Bảo",
        "CustomerPhone": "0911222333", "TotalAmount": 450000, "PaidAmount": 0,
        "Outstanding": 450000, "PaymentStatus": 0, "SellerName": "Ngô Văn Cường",
        "HasReturnedItems": True,
    },
]

_CUSTOMERS = [
    {
        "CustomerId": 1, "CustomerName": "Nguyễn Văn An", "Phone": "0912345678",
        "Address": "Hà Nội", "Email": "an@mail.com", "CustomerTypeName": "VIP",
        "WalletBalance": 1500000, "DebtAmount": 0,
    },
    {
        "CustomerId": 2, "CustomerName": "Lê Thị Hoa", "Phone": "0987654321",
        "Address": "Hà Nội", "Email": "hoa@mail.com", "CustomerTypeName": "Thường",
        "WalletBalance": 120000, "DebtAmount": 750000,
    },
]

_RECEIPTS = [
    {
        "GoodsReceiptId": 201, "ReceiptNumber": "PN-2026-0201",
        "SupplierName": "Hau Giang", "UserName": "Trần Thị Bình",
        "ReceiptDate": "2026-09-18", "TotalAmount": 30000000, "PaidAmount": 30000000,
        "Status": "APPROVED",
    },
    {
        "GoodsReceiptId": 202, "ReceiptNumber": "PN-2026-0202",
        "SupplierName": "Domesco", "UserName": "Ngô Văn Cường",
        "ReceiptDate": "2026-09-22", "TotalAmount": 6000000, "PaidAmount": 3000000,
        "Status": "COMPLETE",
    },
]

_SUPPLIERS = {
    "Suppliers": [
        {"SupplierID": 1, "SupplierName": "Hau Giang", "Phone": "0293", "Email": "hg@mail.com", "Address": "Cần Thơ"},
        {"SupplierID": 2, "SupplierName": "Domesco", "Phone": "0276", "Email": "dc@mail.com", "Address": "Đồng Tháp"},
    ]
}

_BRANCHES = [
    {"BranchId": 1, "BranchName": "Hà Nội"},
    {"BranchId": 2, "BranchName": "TP.HCM"},
]

_FEFO = [
    {"BatchID": "PG123", "ExpiryDate": "2026-10-01", "QuantityInStock": 5, "Price": 15000},
    {"BatchID": "AM099", "ExpiryDate": "2026-11-15", "QuantityInStock": 5, "Price": 22000},
]


def _match(value, needle: str) -> bool:
    return needle.lower() in (value or "").lower()


def _inventory_filter(payload: dict) -> list:
    items = list(_INVENTORY)
    if payload.get("Keyword"):
        items = [i for i in items if _match(i["MedicineName"], payload["Keyword"])
                 or _match(i["ManufacturerName"], payload["Keyword"])]
    if payload.get("IsLowStock"):
        items = [i for i in items if i["QuantityInStock"] <= 10]
    if payload.get("OutOfStock"):
        items = [i for i in items if i["QuantityInStock"] == 0]
    if payload.get("InStock"):
        items = [i for i in items if i["QuantityInStock"] > 0]
    if payload.get("IsExpiringSoon"):
        items = [i for i in items
                 if date.fromisoformat(i["ExpiryDate"]) <= TODAY + timedelta(days=180)]
    if payload.get("MinPrice"):
        items = [i for i in items if i["Price"] >= payload["MinPrice"]]
    if payload.get("MaxPrice"):
        items = [i for i in items if i["Price"] <= payload["MaxPrice"]]
    return items


def _sales_filter(payload: dict) -> list:
    items = list(_SALES)
    if payload.get("InvoiceId"):
        items = [i for i in items if i["InvoiceId"] == payload["InvoiceId"]]
    if payload.get("CustomerPhone"):
        items = [i for i in items if _match(i["CustomerPhone"], payload["CustomerPhone"])]
    if payload.get("PaymentStatus") is not None:
        items = [i for i in items if i["PaymentStatus"] == payload["PaymentStatus"]]
    if payload.get("MinTotal"):
        items = [i for i in items if i["TotalAmount"] >= payload["MinTotal"]]
    if payload.get("FromDate"):
        items = [i for i in items if i["CreatedAt"][:10] >= payload["FromDate"]]
    if payload.get("ToDate"):
        items = [i for i in items if i["CreatedAt"][:10] <= payload["ToDate"]]
    return items


def _customer_filter(payload: dict) -> list:
    items = list(_CUSTOMERS)
    if payload.get("Keyword"):
        items = [i for i in items if _match(i["CustomerName"], payload["Keyword"])
                 or _match(i["Phone"], payload["Keyword"])]
    if payload.get("Phone"):
        items = [i for i in items if _match(i["Phone"], payload["Phone"])]
    if payload.get("HasActiveDebt"):
        items = [i for i in items if i["DebtAmount"] > 0]
    if payload.get("MinWalletBalance"):
        items = [i for i in items if i["WalletBalance"] >= payload["MinWalletBalance"]]
    return items


def _receipt_filter(payload: dict) -> list:
    items = list(_RECEIPTS)
    if payload.get("Keyword"):
        items = [i for i in items if _match(i["SupplierName"], payload["Keyword"])
                 or _match(i["ReceiptNumber"], payload["Keyword"])]
    if payload.get("Status"):
        items = [i for i in items if _match(i["Status"], payload["Status"])]
    if payload.get("MinTotal"):
        items = [i for i in items if i["TotalAmount"] >= payload["MinTotal"]]
    if payload.get("FromDate"):
        items = [i for i in items if i["ReceiptDate"] >= payload["FromDate"]]
    if payload.get("ToDate"):
        items = [i for i in items if i["ReceiptDate"] <= payload["ToDate"]]
    return items


def _analytics(payload: dict) -> dict:
    rtype = payload.get("ReportType")
    if rtype == "TotalRevenue":
        return {"Items": [{"Label": "TotalRevenue", "TotalAmount": 987654321,
                           "FromDate": payload.get("FromDate"), "ToDate": payload.get("ToDate")}]}
    if rtype == "RevenueTrend":
        days = 7
        if payload.get("FromDate") and payload.get("ToDate"):
            try:
                days = max(1, (date.fromisoformat(payload["ToDate"])
                               - date.fromisoformat(payload["FromDate"])).days + 1)
            except ValueError:
                days = 7
        items = [{"Date": str(TODAY - timedelta(days=n)), "Revenue": 1500000 + n * 100000}
                 for n in range(days - 1, -1, -1)]
        return {"Items": items}
    if rtype == "TopSellingMedicine":
        rows = [
            {"MedicineName": "Paracetamol 500mg", "Quantity": 320, "Revenue": 4800000},
            {"MedicineName": "Amoxicillin 500mg", "Quantity": 180, "Revenue": 3960000},
            {"MedicineName": "Vitamin C 1000mg", "Quantity": 96, "Revenue": 1728000},
        ]
        return {"Items": rows[:payload.get("TopN") or len(rows)]}
    if rtype == "TopStockMedicine":
        return {"Items": [{"MedicineName": "Paracetamol 500mg", "Stock": 1500, "Value": 22500000}]}
    if rtype == "LowStockMedicine":
        return {"Items": [{"MedicineName": "Vitamin C 1000mg", "Stock": 5, "Value": 90000}]}
    if rtype == "TopCustomer":
        return {"Items": [{"CustomerName": "Nguyễn Văn An", "TotalSpent": 25000000, "InvoiceCount": 15}]}
    if rtype == "TopCustomerByInvoiceCount":
        return {"Items": [{"CustomerName": "Nguyễn Văn An", "InvoiceCount": 15},
                          {"CustomerName": "Lê Thị Hoa", "InvoiceCount": 8}]}
    if rtype == "TopUserByInvoices":
        return {"Items": [{"FullName": "Trần Thị Bình", "InvoiceCount": 42, "TotalRevenue": 52000000}]}
    return {"Items": []}


def _get_route(path: str, params: dict):
    if path == "api/invoice/fefobatches":
        return list(_FEFO)
    if path == "api/supplier/getall":
        return _SUPPLIERS
    if path == "api/branch/all":
        return _BRANCHES
    return {"error": f"mock backend: chua ro GET route '{path}'"}


def _post_route(path: str, payload: dict):
    if path == "api/ai/inventory/search":
        return {"Items": _inventory_filter(payload)}
    if path == "api/ai/sales/search":
        return {"Items": _sales_filter(payload)}
    if path == "api/ai/customer/search":
        return {"Items": _customer_filter(payload)}
    if path == "api/ai/goodsreceipt/search":
        return {"Items": _receipt_filter(payload)}
    if path == "api/ai/analytics/report":
        return _analytics(payload)
    return {"error": f"mock backend: chua ro POST route '{path}'"}


@contextlib.asynccontextmanager
async def mock_backend():
    """Chạy khối lệnh với backend_client bị thay bằng stub deterministic."""
    orig_get, orig_post = backend_client.get, backend_client.post

    async def fake_get(path: str, params: dict = None, token: str = None, branch_id: str = None):
        return _get_route((path or "").strip("/").lower(), params or {})

    async def fake_post(path: str, json_body: dict = None, token: str = None, branch_id: str = None):
        return _post_route((path or "").strip("/").lower(), json_body or {})

    backend_client.get = fake_get
    backend_client.post = fake_post
    try:
        yield
    finally:
        backend_client.get = orig_get
        backend_client.post = orig_post