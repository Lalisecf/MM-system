
using SharedLayer.AB_Common;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class MstUserRoleRepository : IMstUserRoleRepository
    {
        private readonly MasterDataContext _context;
        IMstUserRepository _userRepository;
        public MstUserRoleRepository(MasterDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MstUserRole>> GetAllUserRolesAsync()
        {
            return await _context.MstUserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<MstUserRole> GetUserRoleAsync(int userId, int roleId)
        {
            return await _context.MstUserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task AddUserRoleAsync(MstUserRole userRole)
        {
            //await _context.MstUserRoles.AllAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserRoleAsync(int userId, int roleId)
        {
            var userRole = await GetUserRoleAsync(userId, roleId);
            if (userRole != null)
            {
                _context.MstUserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserRoleExistsAsync(int userId, int roleId)
        {
            return await _context.MstUserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
        public async Task<MstUserRole> GetAssignedRolesbyUserIdAsync(long? userId)
        {
            try
            {
                if (userId == null)
                {
                    throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
                }

                var assignedRole = await _context.MstUserRoles
                    .Where(sar => sar.UserId == userId)
                    .SingleOrDefaultAsync();

                return assignedRole;
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(inner.Message);
                throw;
            }
        }

    }
}
