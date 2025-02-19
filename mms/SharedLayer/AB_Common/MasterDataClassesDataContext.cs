using SharedLayer.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SharedLayer.AB_Common
{
    public class MasterDataContext : DbContext
    {
        public MasterDataContext() : base("name=MasterDBConnectionString")
        {
        }
        
        public DbSet<MstUser> MstUsers { get; set; }
        public DbSet<MstUserRole> MstUserRoles { get; set; }
        public DbSet<MstMenuDefWeb> MstMenuDefWebs { get; set; }
        public DbSet<MstRole> MstRoles { get; set; }
        public DbSet<MstApplication> MstApplications { get; set; }      
        public DbSet<MstWorkUnitType> MstWorkUnitTypes { get; set; }      
        public DbSet<MstRoleMenuRight> MstRoleMenuRights { get; set; }
        public DbSet<MstFiscalYear> MstFiscalYears { get; set; }

    }

}