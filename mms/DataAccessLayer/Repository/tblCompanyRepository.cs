using SharedLayer.AB_Common;
using SharedLayer.Models.Inventory;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class tblCompanyRepository : ItblCompanyRepository
    {
        private readonly FAMMDataClassesDataContext _context;

        public tblCompanyRepository(FAMMDataClassesDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<tblCompany>> GetTblCompanies()
        {
            return await _context.tblCompanies.ToListAsync();
        }

        public async Task<tblCompany> tblcompanyByName(string id)
        {
            tblCompany tblCompany = await _context.tblCompanies.FindAsync(id);            
            return (tblCompany);
        }


        public async Task Update(tblCompany menu)
        {
            _context.Entry(menu).State = EntityState.Modified; 
            await _context.SaveChangesAsync();
        }

    }
}
