using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Models;
using PharmacyManagement.Repositories.Interfaces;

namespace PharmacyManagement.Repositories.Implements
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly PharmacySystemDbContext _context;

        public RefreshTokenRepository(PharmacySystemDbContext Context)
        {
            _context = Context;
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshToken.AddAsync(refreshToken);
        }

        public async Task RevokeByHashAsync(string hash)
        {
            await _context.RefreshToken
                .Where(x => x.Token == hash)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRevoked, true));
        }

        public async Task<Models.RefreshToken?> GetByHashAsync(string hash)
        {
            return await _context.RefreshToken
                .Include(x => x.User)
                    .ThenInclude(x => x.UserBranch)
                        .ThenInclude(ub => ub.Role)
                .Include(x => x.User)
                    .ThenInclude(x => x.UserBranch)
                        .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(x =>
                    x.Token == hash &&
                    x.IsRevoked == false);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
