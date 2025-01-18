using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IMstUserRoleRepository
    {
        Task<IEnumerable<MstUserRole>> GetAllUserRolesAsync();
        Task<MstUserRole> GetUserRoleAsync(int userId, int roleId);
        Task AddUserRoleAsync(MstUserRole userRole);
        Task DeleteUserRoleAsync(int userId, int roleId);
        Task<bool> UserRoleExistsAsync(int userId, int roleId);
        Task<MstUserRole> GetAssignedRolesbyUserIdAsync(long? userId);
    }
}
