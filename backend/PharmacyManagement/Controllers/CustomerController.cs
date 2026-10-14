using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmacyManagement.DTOs.Customer;
using PharmacyManagement.Infrastructure.RateLimiting;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("CUSTOMER_CREATE")]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Success(result, "Create customer successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("CUSTOMER_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateCustomerRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Success(result, "Update customer successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("CUSTOMER_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Success(null, "Delete customer successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("CUSTOMER_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get customer successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("CUSTOMER_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAll(int page, int count = 10)
        {
            var result = await _service.GetAllAsync(page, count);
            return Success(result, "Get customers successfully.");
        }

        [HttpGet("All")]
        [HasPermission("CUSTOMER_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetAllCustomer()
        {
            var result = await _service.GetAllCustomerAsync();
            return Success(result, "Get all customers successfully.");
        }

        [HttpGet("{id}/Wallet")]
        [HasPermission("CUSTOMER_VIEW")]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> GetWallet(long id)
        {
            var result = await _service.GetWalletAsync(id);
            return Success(result, "Get customer wallet successfully.");
        }
    }
}
