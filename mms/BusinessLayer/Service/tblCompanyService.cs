using SharedLayer.Models.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class tblcompanyservice : ItblCompanyService
    {
        private readonly ItblCompanyService _companyRepository;

        public tblcompanyservice(ItblCompanyService menuRepository)
        {
            _companyRepository = menuRepository;
        }
        public async Task<IEnumerable<tblCompany>> GetTblCompanies()
        {
            return await _companyRepository.GetTblCompanies();
        }

        public async Task<tblCompany> tblcompanyByName(string menuCode)
        {
            return await _companyRepository.tblcompanyByName(menuCode);
        }
        public async Task Update(tblCompany menu)
        {
            await _companyRepository.Update(menu);
        }
    }
}
