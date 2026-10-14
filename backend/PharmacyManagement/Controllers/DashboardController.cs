using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting(RateLimitPolicies.Dashboard)]
    [Authorize(Roles = "admin,manage_supply,manage_branch,user_sale,user_warehouse")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetTotalRevenueAsync(branchId);
            return Success(result);
        }

        [HttpGet("total-orders")]
        public async Task<IActionResult> GetTotalOrders()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetTotalOrdersAsync(branchId);
            return Success(result);
        }

        [HttpGet("revenue-by-branch")]
        public async Task<IActionResult> GetRevenueByBranch()
        {
            var result = await _service.GetRevenueByBranchAsync();
            return Success(result);
        }

        [HttpGet("new-customers")]
        public async Task<IActionResult> GetNewCustomers()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetNewCustomersAsync(branchId);
            return Success(result);
        }

        [HttpGet("revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend([FromQuery] int days = 7)
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetRevenueTrendAsync(branchId, days);
            return Success(result);
        }

        [HttpGet("top-medicines")]
        public async Task<IActionResult> GetTopMedicines([FromQuery] int top = 10)
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetTopMedicinesAsync(branchId, top);
            return Success(result);
        }

        [HttpGet("expiring-batches")]
        public async Task<IActionResult> GetExpiringBatches()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetExpiringBatchesAsync(branchId);
            return Success(result);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] decimal threshold = 10)
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetLowStockAsync(branchId, threshold);
            return Success(result);
        }

        [HttpGet("destroy-queue")]
        public async Task<IActionResult> GetDestroyQueue()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetDestroyQueueAsync(branchId);
            return Success(result);
        }

        [HttpGet("inventory-capital")]
        public async Task<IActionResult> GetInventoryCapital()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetInventoryCapitalAsync(branchId);
            return Success(result);
        }

        [HttpGet("pending-imports")]
        public async Task<IActionResult> GetPendingImports()
        {
            long? branchId = IsAdminOrManageSupply() ? null : GetBranchIdFromHeader();
            var result = await _service.GetPendingImportsAsync(branchId);
            return Success(result);
        }

        [HttpGet("stock-transfer-requests")]
        public async Task<IActionResult> GetStockTransferRequests()
        {
            var result = await _service.GetStockTransferRequestsAsync();
            return Success(result);
        }

        [HttpGet("pending-stock-transfers")]
        public async Task<IActionResult> GetPendingStockTransfers()
        {
            var result = await _service.GetPendingStockTransfersAsync();
            return Success(result);
        }

        [HttpGet("branch-revenue")]
        public async Task<IActionResult> GetBranchRevenue()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetBranchRevenueAsync(branchId);
            return Success(result);
        }

        [HttpGet("branch-orders")]
        public async Task<IActionResult> GetBranchOrders()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetBranchOrdersAsync(branchId);
            return Success(result);
        }

        [HttpGet("out-of-stock")]
        public async Task<IActionResult> GetOutOfStock()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetOutOfStockAsync(branchId);
            return Success(result);
        }

        [HttpGet("employee-progress")]
        public async Task<IActionResult> GetEmployeeProgress()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetEmployeeProgressAsync(branchId);
            return Success(result);
        }

        [HttpGet("shift-revenue")]
        public async Task<IActionResult> GetShiftRevenue()
        {
            var branchId = GetBranchIdFromHeader();
            var userId = GetUserIdFromToken();
            var result = await _service.GetShiftRevenueAsync(branchId, userId);
            return Success(result);
        }

        [HttpGet("shift-orders")]
        public async Task<IActionResult> GetShiftOrders()
        {
            var branchId = GetBranchIdFromHeader();
            var userId = GetUserIdFromToken();
            var result = await _service.GetShiftOrdersAsync(branchId, userId);
            return Success(result);
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            var result = await _service.GetPromotionsAsync();
            return Success(result);
        }

        [HttpGet("counter-alerts")]
        public async Task<IActionResult> GetCounterAlerts()
        {
            var branchId = GetBranchIdFromHeader();
            var result = await _service.GetCounterAlertsAsync(branchId);
            return Success(result);
        }
    }
}
