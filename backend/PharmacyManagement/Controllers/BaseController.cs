using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.share;
using System.Diagnostics;
using System.Security.Claims;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected long GetBranchIdFromHeader()
        {
            if (Request.Headers.TryGetValue("X-Branch-Id", out var branchIdValue)
                && long.TryParse(branchIdValue.ToString(), out var branchId))
            {
                return branchId;
            }

            throw new Exceptions.BusinessException(
                "X-Branch-Id header is missing or invalid.",
                "HDR001",
                StatusCodes.Status400BadRequest);
        }

        protected long GetUserIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
            {
                return userId;
            }

            throw new Exceptions.BusinessException(
                "User ID not found in token.",
                "HDR002",
                StatusCodes.Status401Unauthorized);
        }

        protected string GetRoleNameFromToken()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        protected bool IsAdminOrManageSupply()
        {
            var role = GetRoleNameFromToken();
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            return role == "admin" || role == "manage_supply";
        }

        /// <summary>
        /// Xác định phạm vi chi nhánh cho các truy vấn AI.
        /// - Nếu header X-Branch-Id hợp lệ: luôn dùng header.
        /// - Admin / manage_supply không có header: null = lấy tất cả chi nhánh.
        /// - Role khác không có header: bắt buộc (lỗi HDR001).
        /// </summary>
        protected long? ResolveBranchScope()
        {
            if (Request.Headers.TryGetValue("X-Branch-Id", out var branchIdValue)
                && long.TryParse(branchIdValue.ToString(), out var branchId))
            {
                return branchId;
            }

            if (IsAdminOrManageSupply())
            {
                return null;
            }

            return GetBranchIdFromHeader();
        }

        protected IActionResult Success(object? data = null, string message = "Success", int encode = 0)
        {
            return Ok(new ApiResponse<object>
            {
                EC = encode,
                StatusCode = 200,
                EM = message,
                DT = data
            });
        }
    }


}
