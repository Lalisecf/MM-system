using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public  interface ItblCompanyRepository
    {
        Task<IEnumerable<tblCompany>> GetTblCompanies();
        Task Update(tblCompany company);
        Task<tblCompany> tblcompanyByName(string menuCode);

    }
}
