using SharedLayer.Models.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface ItblCompanyService
    {
        Task<IEnumerable<tblCompany>> GetTblCompanies();
        Task Update(tblCompany company);
        Task<tblCompany> tblcompanyByName(string menuCode);
    }
}
