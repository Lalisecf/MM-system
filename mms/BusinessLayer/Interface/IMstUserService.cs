
using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer
{

    public interface IMstUserService
    {
        Task<IEnumerable<MstUser>> GetAllUsersAsync();
        Task<MstUser> GetUserByIdAsync(int userId);       
        Task AddUserAsync(MstUser user);
        Task UpdateUserAsync(MstUser user);
        Task DeleteUserAsync(int userId);
        Task<bool> UserNameExistsAsync(string userName);
        Task<bool> EmailAddressExistsAsync(string emailAddress);
        Task<bool> EmployeeIdExistsAsync(int empId);
    }
}
