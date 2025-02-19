using System;
using System.Data.SqlClient;
using System.Linq;

namespace SharedLayer.AB_Common
{
    class PeriodEnd
    {
        private readonly FAMMDataClassesDataContext _context;
        private readonly MasterDataContext _mastercontext;
        private string _currentperiod;  // Class-level variable to store the current period

        // Constructor to initialize context and get the current period
        public PeriodEnd(FAMMDataClassesDataContext context, MasterDataContext mastercontext)
        {
            _context = context;
            _mastercontext = mastercontext;

            // Get current period from the tblCompanies table
            _currentperiod = _context.tblCompanies
                .Select(x => x.FiscalYear.ToString() + x.CurrentPeriod.ToString())
                .FirstOrDefault();
        }

        // Method to check if all transactions are journalized
        public bool AllTransactionJournalized()
        {
            try
            {
                var result = _context.tblTranDts
                    .Join(_context.tblTranHds,
                        b => b.RefNo,
                        a => a.RefNo,
                        (b, a) => b)
                    .Where(b => !b.Journalize)
                    .Select(b => b.RefNo)
                    .FirstOrDefault();

                return result == null;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }

        // Method to check if all transactions are posted
        public bool AllTransactionPosted()
        {
            try
            {
                var result = _context.tblJournals
                    .Where(j => !j.PostToGl)
                    .Select(j => j.RefNo)
                    .FirstOrDefault();

                return result == null;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }

        // Method to check if inventory is updated
        public bool InventoryUpdate()
        {
            try
            {
                var result = _context.tblStores
                    .Where(s => (bool)!s.InventoryUpdate)
                    .Select(s => s.InventoryUpdate)
                    .FirstOrDefault();

                return result == null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Method to update the main category
        public bool UpdateMainCategory()
        {
            try
            {
                var query = from im in _context.tblItemMasters
                            join cat in _context.tblCategories on im.ProductGroup equals cat.CatCode
                            select new { im, cat };

                foreach (var item in query)
                {
                    item.im.MainPG = item.cat.MainPG;
                    item.im.StockAccountSegment = item.cat.StockAccountSegment;
                    item.im.ExpenseAccountSegment = item.cat.ExpenseAccountSegment;
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }

        // Method to update transaction accounts
        public bool UpdateTransactionAccount(string RefNo, bool IsYtd)
        {
            try
            {
                string query = IsYtd
                    ? "UPDATE tblTrandtYtd SET StockAccountNo = cat.StockAccountSegment + tblTrandtYtd.Stockcc, CostAccountNo = cat.ExpenseAccountSegment + tblTrandtYtd.CostCenter FROM tblTrandtYtd INNER JOIN tblItemmaster cat ON tblTrandtYtd.ProdCode = cat.ProdCode WHERE tblTrandtYtd.RefNo = @RefNo"
                    : "UPDATE tblTrandt SET StockAccountNo = cat.StockAccountSegment + tblTrandt.Stockcc, CostAccountNo = cat.ExpenseAccountSegment + tblTrandt.CostCenter FROM tblTrandt INNER JOIN tblItemmaster cat ON tblTrandt.ProdCode = cat.ProdCode WHERE tblTrandt.RefNo = @RefNo";

                _context.Database.ExecuteSqlCommand(query, new SqlParameter("@RefNo", RefNo));
                return true;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }

        // Method for immediate backup (currently not implemented)
        public bool ImmediateBackup()
        {
            // Replace with your EF code to execute a backup if applicable
            return false;
        }

        // Method to get server date
        public string GetServerDate()
        {
            try
            {
                var serverDate = _context.Database.SqlQuery<DateTime>("SELECT GETDATE()").FirstOrDefault();
                return serverDate.ToString("dd-MMMM-yyyy h-mm-ss tt");
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return string.Empty;
            }
        }

        // Method to update item summary
        public bool UpdateItemSummary()
        {
            try
            {
                _context.Database.ExecuteSqlCommand("TRUNCATE TABLE tblItemSummary");

                _context.Database.ExecuteSqlCommand("EXEC Sp_UpdateItemSummary @PeriodId",
                    new SqlParameter("@PeriodId", _currentperiod));  // Use _currentperiod here

                return true;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }

        // Method to process month end, including year-end logic
        public bool MonthEnd(bool yearEnd)
        {
            int maxPeriod, currentPeriod, physicalYear;

            try
            {
                if (!ImmediateBackup() || !UpdateItemSummary())
                {
                    return false;
                }

                maxPeriod = _mastercontext.MstApplications
                    .Where(m => m.AppName == "FAMM")
                    .Select(m => m.MaximumPeriod)
                    .FirstOrDefault();

                physicalYear = _context.tblCompanies
                    .Select(c => c.FiscalYear)
                    .FirstOrDefault() ?? 0;

                currentPeriod = Convert.ToInt32(_currentperiod) % physicalYear;  // Use the current period stored earlier

                if ((currentPeriod == maxPeriod && !yearEnd) || (currentPeriod < maxPeriod && yearEnd))
                {
                    return false;
                }

                string cmdText = "INSERT INTO tblJournalYtd (Id, DocNo, CostCenter, AccountNo, RefNo, [Date], DateEth, PeriodId, Debit, Credit, PostToGl, Naration, Transfered, SubLedgerNo, CrtBy, CrtDt) ";
                cmdText += "SELECT Id, DocNo, CostCenter, AccountNo, RefNo, [Date], DateEth, @PeriodId AS PeriodId, Debit, Credit, PostToGl, Naration, Transfered, SubLedgerNo, CrtBy, CrtDt FROM tblJournal";

                _context.Database.ExecuteSqlCommand(cmdText, new SqlParameter("@PeriodId", _currentperiod));  // Use _currentperiod here

                return true;
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                return false;
            }
        }
    }
}
