using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTOs.Role;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;

namespace PharmacyManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("ROLE_CREATE")]
        public async Task<IActionResult> Create(CreateRoleRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Success(result, "Create role successfully.");
        }

        [HttpPut("{id}")]
        [HasPermission("ROLE_UPDATE")]
        public async Task<IActionResult> Update(long id, UpdateRoleRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Success(result, "Update role successfully.");
        }

        [HttpDelete("{id}")]
        [HasPermission("ROLE_DELETE")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return Success(null, "Delete role successfully.");
        }

        [HttpGet("{id}")]
        [HasPermission("ROLE_VIEW")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result, "Get role successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("ROLE_VIEW")]
        public async Task<IActionResult> GetAll(int page, int count = 10)
        {
            var result = await _service.GetAllAsync(page, count);
            return Success(result, "Get roles successfully.");
        }

        [HttpGet("All")]
        [HasPermission("Role_View")]
        public async Task<IActionResult> GetAllRole()
        {
            var result = await _service.GetAllRoleAsync();
            return Success(result, "Get all roles successfully.");
        }
    }
}
