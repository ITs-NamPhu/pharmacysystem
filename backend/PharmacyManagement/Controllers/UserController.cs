using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTOs.User;
using PharmacyManagement.Services.Interfaces;
using PharmacyManagement.Validators.PermissionHandle;
using System.Security.Claims;

namespace PharmacyManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        [HasPermission("USER_CREATE")]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Success(result, "Create user successfully.");
        }

        [HttpDelete]
        [HasPermission("USER_DELETE")]
        public async Task<IActionResult> Delete(long userID)
        {
            var branchID = GetBranchIdFromHeader();
            await _service.DeleteAsync(userID, branchID);

            return Success(null, "Delete user successfully.");
        }

        [HttpPut]
        [HasPermission("USER_UPDATE")]
        public async Task<IActionResult> Update(long userID, UpdateUserRequest user)
        {
            var branchID = GetBranchIdFromHeader();
            var result = await _service.UpdateAsync(userID, branchID, user);

            return Success(result, "Update user successfully.");
        }

        [HttpGet("GetAll")]
        [HasPermission("User_View")]
        public async Task<IActionResult> GetAllUser(int page, int count = 10)
        {
            var branchID = GetBranchIdFromHeader();
            var result = await _service.GetAllUserbyBranchAsync_Service(branchID, page, count);

            return Success(result, "Get user successfully.");
        }

        [HttpGet("GetAllActive")]
        [HasPermission("USER_VIEW")]
        public async Task<IActionResult> GetAllUserActive(int page, int count = 10)
        {
            var branchID = GetBranchIdFromHeader();
            var result = await _service.GetAllUserActiveAsync_Service(branchID, page, count);

            return Success(result, "Get user successfully.");
        }

        [HttpGet("SingleUser")]
        [HasPermission("USER_VIEW")]
        public async Task<IActionResult> GetUserByBranch(long userID)
        {
            var branchID = GetBranchIdFromHeader();
            var result = await _service.GetUserByIDAsync_Service(userID, branchID);

            return Success(result, "Get user successfully.");
        }

        [HttpPut("{userId}/assignment")]
        [HasPermission("USER_UPDATE")]
        public async Task<IActionResult> AssignRole(long userId, UserAssignmentRequest request)
        {
            var callerUserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var callerBranchId = GetBranchIdFromHeader();

            var result = await _service.AssignRoleToUserAsync(
                userId, request, callerUserId, callerBranchId);

            return Success(result, "Assign role successfully.");
        }
    }
}
