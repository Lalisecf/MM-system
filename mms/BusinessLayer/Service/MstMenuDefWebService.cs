using BusinessLayer.Services;
using DataAccessLayer;
using SharedLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class MstMenuDefWebService : IMstMenuDefWebService
    {
        private readonly IMstMenuDefWebRepository _menuRepository;

        public MstMenuDefWebService(IMstMenuDefWebRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<IEnumerable<MstMenuDefWeb>> GetAllMenusAsync()
        {
            return await _menuRepository.GetAllMenusAsync();
        }

        public async Task<MstMenuDefWeb> GetMenuByIdAsync(int menuCode)
        {
            return await _menuRepository.GetMenuByIdAsync(menuCode);
        }

        public async Task AddMenuAsync(MstMenuDefWeb menu)
        {
            if (!await _menuRepository.MenuCodeExistsAsync(menu.MenuCode))
            {
                await _menuRepository.AddMenuAsync(menu);
            }
        }

        public async Task UpdateMenuAsync(MstMenuDefWeb menu)
        {
            await _menuRepository.UpdateMenuAsync(menu);
        }

        public async Task DeleteMenuAsync(int menuCode)
        {
            await _menuRepository.DeleteMenuAsync(menuCode);
        }

        public async Task<bool> MenuCodeExistsAsync(int menuCode)
        {
            return await _menuRepository.MenuCodeExistsAsync(menuCode);
        }

        public async Task<IEnumerable<MstMenuDefWeb>> GetSubMenusAsync(int parentCode)
        {
            return await _menuRepository.GetSubMenusAsync(parentCode);
        }
    }
}
