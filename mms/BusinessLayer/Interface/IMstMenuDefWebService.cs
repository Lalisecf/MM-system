using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface IMstMenuDefWebService
    {
        Task<IEnumerable<MstMenuDefWeb>> GetAllMenusAsync();
        Task<MstMenuDefWeb> GetMenuByIdAsync(int menuCode);
        Task AddMenuAsync(MstMenuDefWeb menu);
        Task UpdateMenuAsync(MstMenuDefWeb menu);
        Task DeleteMenuAsync(int menuCode);
        Task<bool> MenuCodeExistsAsync(int menuCode);
        Task<IEnumerable<MstMenuDefWeb>> GetSubMenusAsync(int parentCode);
    }
}
