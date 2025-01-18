
using SharedLayer.AB_Common;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class MstMenuDefWebRepository : IMstMenuDefWebRepository
    {
        private readonly MasterDataContext _context;

        public MstMenuDefWebRepository(MasterDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MstMenuDefWeb>> GetAllMenusAsync()
        {
            return await _context.MstMenuDefWebs
                .Include(m => m.ParentMenu)
                .Include(m => m.Application)
                .ToListAsync();
        }

        public async Task<MstMenuDefWeb> GetMenuByIdAsync(int menuCode)
        {
            return await _context.MstMenuDefWebs
                .Include(m => m.ParentMenu)
                .Include(m => m.Application)
                .FirstOrDefaultAsync(m => m.MenuCode == menuCode);
        }

        public async Task AddMenuAsync(MstMenuDefWeb menu)
        {
            _context.MstMenuDefWebs.Add(menu); // No async in EF6
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMenuAsync(MstMenuDefWeb menu)
        {
            _context.Entry(menu).State = EntityState.Modified; // Update manually
            await _context.SaveChangesAsync();
        }

        public List<MstMenuDefWeb> GetAllMenu()
        {
            try
            {
                var listofActiveMenu = (from menu in _context.MstMenuDefWebs

                                        select menu ).Where (menu=>menu.ParentCode==0).ToList();

                return listofActiveMenu;
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

        public List<MstMenuDefWeb> GetAllActiveMenu(long userid)
        {
            try
            {
                var listofActiveMenu = (from menu in _context.MstMenuDefWebs

                                        select menu).ToList();

                listofActiveMenu.Insert(0, new MstMenuDefWeb()
                {
                    MenuCode = -1,
                    Description = "---Select---"
                });

                return listofActiveMenu;
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

        public async Task DeleteMenuAsync(int menuCode)
        {
            var menu = await _context.MstMenuDefWebs.FindAsync(menuCode);
            if (menu != null)
            {
                _context.MstMenuDefWebs.Remove(menu);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> MenuCodeExistsAsync(int menuCode)
        {
            return await _context.MstMenuDefWebs.AnyAsync(m => m.MenuCode == menuCode);
        }

        public async Task<IEnumerable<MstMenuDefWeb>> GetSubMenusAsync(int parentCode)
        {
            return await _context.MstMenuDefWebs
                .Where(m => m.ParentCode == parentCode)
                .ToListAsync();
        }
    }
}
