using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IMstMenuDefWebRepository
    {
        Task<IEnumerable<MstMenuDefWeb>> GetAllMenusAsync();
        Task<MstMenuDefWeb> GetMenuByIdAsync(int menuCode);
        List<MstMenuDefWeb> GetAllMenu();
        List<MstMenuDefWeb> GetAllActiveMenu(long userid);
        Task AddMenuAsync(MstMenuDefWeb menu);
        Task UpdateMenuAsync(MstMenuDefWeb menu);
        Task DeleteMenuAsync(int menuCode);
        Task<bool> MenuCodeExistsAsync(int menuCode);
        Task<IEnumerable<MstMenuDefWeb>> GetSubMenusAsync(int parentCode);
    }
}
