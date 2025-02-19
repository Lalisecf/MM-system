using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SharedLayer.Models.Inventory;

namespace SharedLayer.AB_Common
{
    public class FAMMDataClassesDataContext : DbContext
    {
        public FAMMDataClassesDataContext() : base("name=INVENTORYConnectionString")
        {
        }

        public DbSet<tblCompany> tblCompanies { get; set; }
        public DbSet<LogHistory> logHistories { get; set; }
        public DbSet<tblRequestToApprove> tblRequestToApproves { get; set; }
        public DbSet<tblNameValue> tblNameValues { get; set; }
        public DbSet<tblUserStore> tblUserStores { get; set; }
        public DbSet<tblStore> tblStores { get; set; }
        public DbSet<tblPeriodLu> tblPeriodLus { get; set; }
        public DbSet<tblCategory> tblCategories { get; set; }
        public DbSet<tblFaMaster> tblFaMasters { get; set; }
        public DbSet<tblTranDt> tblTranDts { get; set; }
        public DbSet<tblTranDtCs> tblTranDtCs { get; set; }
        public DbSet<tblTranDtYtd> tblTranDtYtds { get; set; }
        public DbSet<tblTranHd> tblTranHds { get; set; }
        public DbSet<tblTranHdYtd> tblTranHdYtds { get; set; }
        public DbSet<tblItemDetail> tblItemDetails { get; set; }
        public DbSet<tblItemMaster> tblItemMasters { get; set; }
        public DbSet<tblTransactionType> tblTransactionTypes { get; set; }
        public DbSet<tblTransactionGroup> tblTransactionGroups { get; set; }
        public DbSet<CustomerMaster> customerMasters { get; set; }
        public DbSet<tblDepartment> tblDepartments { get; set; }
        public DbSet<tblDeprtLevels> tblDeprtLevels { get; set; }
        public DbSet<tblDeptType> tblDeptTypes { get; set; }
        public DbSet<tblSupplierCategory> tblSupplierCategories { get; set; }
        public DbSet<tblRequest> tblRequests { get; set; }
        public DbSet<tblRequestDetail> tblRequestDetails { get; set; }
        public DbSet<tblRequesterDeptRight> tblRequesterDeptRights { get; set; }
        public DbSet<tblItemSummary> tblItemSummaries { get; set; }
        public DbSet<tblCostBuildup> tblCostBuildups { get; set; }
        public DbSet<tblItemBrand> tblItemBrands { get; set; }
        public DbSet<tblCostSplitAI> tblCostSplitAIs { get; set; }

    }
}
