using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLayer.Models;

namespace DataAccessLayer
{
    public interface IMstUserRepository
    {
        Task<IEnumerable<MstUser>> GetAllAsync();
        Task<MstUser> GetByIdAsync(int userId);
        Task<MstUser> GetByUsernameAsync(string username);  // Async version
        MstUser GetByUsername(string username);
        MstUser UpdateLogin(MstUser usermaster);       
        Task AddAsync(MstUser user);
        Task UpdateAsync(MstUser user);
        Task DeleteAsync(int userId);
        Task<bool> UserNameExistsAsync(string userName);
        Task<bool> EmailAddressExistsAsync(string emailAddress);
        Task<bool> EmployeeIdExistsAsync(int empId);
    }
}


