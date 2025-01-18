
using DataAccessLayer;
using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class MstUserRoleService : IMstUserRoleService
    {
        private readonly IMstUserRoleRepository _userRoleRepository;

        public MstUserRoleService(IMstUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<IEnumerable<MstUserRole>> GetAllUserRolesAsync()
        {
            return await _userRoleRepository.GetAllUserRolesAsync();
        }

        public async Task<MstUserRole> GetUserRoleAsync(int userId, int roleId)
        {
            return await _userRoleRepository.GetUserRoleAsync(userId, roleId);
        }

        public async Task AddUserRoleAsync(MstUserRole userRole)
        {
            if (!await _userRoleRepository.UserRoleExistsAsync(userRole.UserId, userRole.RoleId))
            {
                await _userRoleRepository.AddUserRoleAsync(userRole);
            }
        }

        public async Task DeleteUserRoleAsync(int userId, int roleId)
        {
            if (await _userRoleRepository.UserRoleExistsAsync(userId, roleId))
            {
                await _userRoleRepository.DeleteUserRoleAsync(userId, roleId);
            }
        }

        public async Task<bool> UserRoleExistsAsync(int userId, int roleId)
        {
            return await _userRoleRepository.UserRoleExistsAsync(userId, roleId);
        }
    }
}
