using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using SharedLayer.Models.Inventory;
using SharedLayer.Models;

namespace SharedLayer.AB_Common
{
    public class SystemTools
    {
        private string db2ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ConnStringDb2"].ConnectionString;
            }
        }

        public static string inventoryConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["INVENTORYConnectionString"].ConnectionString;
            }
        }

        public static string MasterConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MasterDBConnectionString"].ConnectionString;
            }
        }
        public bool authorizeMakerRolefromCore(string username)
        {
            using (OdbcConnection odbcConnection = new OdbcConnection(db2ConnectionString))
            {
                string commandText = "SELECT BFUSERIDPK,BFGROUPNAMEPK from BANKFUSION.BFTB_USERPERMISSIONVIEW where BFGROUPNAMEPK in ('PayReconciliationAcct','AssMMSStaffBenAcct','MMSStaffBenAcc','BROperManager','CustSerOpreManager','BRAcc','FIAC','ReportConsolidationAcct') and BFUSERIDPK='" + username + "'";

                using (OdbcCommand command = new OdbcCommand(commandText, odbcConnection))
                {
                    command.CommandType = CommandType.Text;
                    odbcConnection.Open();
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        public string GetUserBranch(string username)
        {
            using (OdbcConnection odbcConnection = new OdbcConnection(db2ConnectionString))
            {
                odbcConnection.Open();
                string commandText = "SELECT BFBRANCHSORTCODE FROM BANKFUSION.BFTB_USER WHERE BFNAMEPK='" + username + "'";
                using (OdbcCommand command = new OdbcCommand(commandText, odbcConnection))
                {
                    object result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public int GetCurrentPeriod()
        {
            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                string commandText = @"SELECT [CurrentPeriod], [FiscalYear] FROM [INVENTORY].[dbo].[tblCompany]";

                using (SqlCommand command = new SqlCommand(commandText, sqlConnection))
                {
                    command.CommandType = CommandType.Text;
                    sqlConnection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int currentPeriod = DateTime.Now.Month;
                            int fiscalYear = DateTime.Now.Year;
                            return Convert.ToInt32(fiscalYear.ToString() + currentPeriod.ToString("D2"));
                        }
                        else
                        {
                            throw new InvalidOperationException("No company record found.");
                        }
                    }
                }
            }
        }


        public static DateTime GetServerDate()
        {
            DateTime serverDate;

            using (SqlConnection sqlConnection = new SqlConnection(MasterConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                    serverDate = GetServerDate(sqlConnection);
                }
                catch (Exception exception)
                {
                    throw new Exception(exception.Message, exception);
                }
            }

            return serverDate;
        }

        public static DateTime GetServerDate(SqlConnection sqlConnection)
        {
            DateTime time;
            try
            {
                string commandText = "SELECT getdate()";

                using (SqlCommand command = new SqlCommand(commandText, sqlConnection))
                {
                    object dataScalar = command.ExecuteScalar();
                    time = (dataScalar == null) ? new DateTime(0x76c, 1, 1) : Convert.ToDateTime(dataScalar);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }

            return time;
        }


        public static string GetStockAccount(string storeCode, string prodCode)
        {
            string resultAccount = string.Empty;

            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                string commandText1 = "SELECT StockAccountSegment FROM tblItemMaster WHERE ProdCode = @ProdCode";
                string commandText2 = "SELECT StockCc FROM tblStore WHERE StoreCode = @StoreCode";

                using (SqlCommand command1 = new SqlCommand(commandText1, sqlConnection))
                {
                    command1.Parameters.AddWithValue("@ProdCode", prodCode);

                    sqlConnection.Open();

                    using (SqlDataReader reader1 = command1.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            resultAccount = Convert.ToString(reader1["StockAccountSegment"]);
                        }
                    }
                }

                using (SqlCommand command2 = new SqlCommand(commandText2, sqlConnection))
                {
                    command2.Parameters.AddWithValue("@StoreCode", storeCode);

                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        if (reader2.Read())
                        {
                            resultAccount += Convert.ToString(reader2["StockCc"]);
                        }
                    }
                }
            }

            return resultAccount;
        }

        public static string GetCostAccount1(string deptCode, string prodCode, string mainpg)
        {
            string resultAccount = string.Empty;

            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                string deptTypeQuery = "SELECT DeptType FROM tbldepartments WHERE DeptCode = @DeptCode";
                string itemAccountQuery = "SELECT ExpenseAccountSegment FROM tblItemMaster WHERE ProdCode = @ProdCode";
                string itemIFBAccountQuery = "SELECT IFBExpenseAccount FROM tblItemMaster WHERE ProdCode = @ProdCode";

                try
                {
                    sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(deptTypeQuery, sqlConnection))
                    {
                        command.Parameters.AddWithValue("@DeptCode", deptCode);
                        object deptTypeResult = command.ExecuteScalar();
                        long deptType = Convert.ToInt64(deptTypeResult);

                        using (SqlCommand command1 = new SqlCommand(itemAccountQuery, sqlConnection))
                        {
                            command1.Parameters.AddWithValue("@ProdCode", prodCode);
                            object result = command1.ExecuteScalar();

                            if (deptType == 1)
                            {
                                if (result != null && !Convert.IsDBNull(result))
                                {
                                    resultAccount = Convert.ToString(result);
                                }

                                if (mainpg != null)
                                {
                                    string costCenterQuery = string.Empty;
                                    switch (mainpg)
                                    {
                                        case "AB":
                                            costCenterQuery = "SELECT MainCostCenter FROM tbldepartments WHERE DeptCode = @DeptCode";
                                            break;
                                        case "BF":
                                            costCenterQuery = "SELECT Furniture FROM tbldepartments WHERE DeptCode = @DeptCode";
                                            break;
                                        case "GX":
                                            costCenterQuery = "SELECT OfficeEquip FROM tbldepartments WHERE DeptCode = @DeptCode";
                                            break;
                                        case "CP":
                                            costCenterQuery = "SELECT MotorVehicle FROM tbldepartments WHERE DeptCode = @DeptCode";
                                            break;
                                        default:
                                            costCenterQuery = null;
                                            break;
                                    }

                                    if (costCenterQuery != null)
                                    {
                                        using (SqlCommand costCenterCommand = new SqlCommand(costCenterQuery, sqlConnection))
                                        {
                                            costCenterCommand.Parameters.AddWithValue("@DeptCode", deptCode);
                                            object result2 = costCenterCommand.ExecuteScalar();
                                            if (result2 != null && !Convert.IsDBNull(result2))
                                            {
                                                resultAccount += Convert.ToString(result2);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (deptType == 2)
                            {
                                using (SqlCommand command2 = new SqlCommand(itemIFBAccountQuery, sqlConnection))
                                {
                                    command2.Parameters.AddWithValue("@ProdCode", prodCode);
                                    object result1 = command2.ExecuteScalar();
                                    if (result1 != null && !Convert.IsDBNull(result1))
                                    {
                                        resultAccount = Convert.ToString(result1);
                                    }

                                    if (mainpg != null)
                                    {
                                        string costCenterQuery = string.Empty;

                                        switch (mainpg)
                                        {
                                            case "AB":
                                                costCenterQuery = "SELECT MainCostCenter FROM tbldepartments WHERE DeptCode = @DeptCode";
                                                break;
                                            case "BF":
                                                costCenterQuery = "SELECT Furniture FROM tbldepartments WHERE DeptCode = @DeptCode";
                                                break;
                                            case "GX":
                                                costCenterQuery = "SELECT OfficeEquip FROM tbldepartments WHERE DeptCode = @DeptCode";
                                                break;
                                            case "CP":
                                                costCenterQuery = "SELECT MotorVehicle FROM tbldepartments WHERE DeptCode = @DeptCode";
                                                break;
                                            default:
                                                costCenterQuery = null;
                                                break;
                                        }

                                        if (costCenterQuery != null)
                                        {
                                            using (SqlCommand costCenterCommand2 = new SqlCommand(costCenterQuery, sqlConnection))
                                            {
                                                costCenterCommand2.Parameters.AddWithValue("@DeptCode", deptCode);
                                                object result2 = costCenterCommand2.ExecuteScalar();
                                                if (result2 != null && !Convert.IsDBNull(result2))
                                                {
                                                    resultAccount += Convert.ToString(result2);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return resultAccount;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
        public static string GetCostCenter1(string deptCode, string mainpg)
        {
            string resultAccount = string.Empty;

            if (string.IsNullOrEmpty(deptCode) || string.IsNullOrEmpty(mainpg))
            {
                return null;
            }

            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    if (mainpg == "SO")
                    {
                        string query = "SELECT MainCostCenter FROM tbldepartments WHERE DeptCode = @DeptCode";

                        using (SqlCommand command = new SqlCommand(query, sqlConnection))
                        {
                            command.Parameters.AddWithValue("@DeptCode", deptCode);

                            object result = command.ExecuteScalar();

                            if (result != null && !Convert.IsDBNull(result))
                            {
                                resultAccount = Convert.ToString(result);
                            }
                        }
                    }
                    else
                    {
                        resultAccount = null;
                    }

                    return resultAccount;
                }
                catch (Exception Ex)
                {
                    return null;
                }
            }
        }
        public string GetTagPrefix(string MainPG, string departmentName, string oldMainPG, string TagCode, string ProdCode)
        {
            if (string.IsNullOrEmpty(MainPG) || string.IsNullOrEmpty(departmentName) || string.IsNullOrEmpty(oldMainPG) || string.IsNullOrEmpty(TagCode))
            {
                throw new Exception($"TagCode is required for product {ProdCode}. Please add a TagCode before proceeding.");
            }

            string departmentAbbreviation = GetAbbreviation(departmentName);
            string resultAccount = $"{MainPG}/{departmentAbbreviation}/{oldMainPG}/{TagCode}";

            return resultAccount;
        }

        private static string GetAbbreviation(string departmentName)
        {
            if (string.IsNullOrEmpty(departmentName))
                return string.Empty;

            return string.Concat(departmentName.Split(' ').Select(word => word[0])).ToUpper();
        }
        public static bool UpdateNextNumber(int tranCode)
        {
            bool isSuccess = false;

            if (tranCode <= 0)
            {
                return false;
            }

            const string query = "UPDATE tblTransactionType SET NextNumber = NextNumber + 1 WHERE TranCode = @TranCode";

            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        command.Parameters.AddWithValue("@TranCode", tranCode);

                        int rowsAffected = command.ExecuteNonQuery();

                        isSuccess = rowsAffected > 0;
                    }
                }
                catch (Exception Ex)
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }
        public int UpdateTagNextNumber(string ProdCode)
        {
            if (string.IsNullOrEmpty(ProdCode))
            {
                return -1;
            }

            const string query = "UPDATE tblItemMaster SET NextNumber = NextNumber + 1 WHERE ProdCode = @ProdCode";

            using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
            {
                try
                {
                    sqlConnection.Open();

                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        command.Parameters.AddWithValue("@ProdCode", ProdCode);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Now fetch the updated NextNumber
                            const string fetchQuery = "SELECT NextNumber FROM tblItemMaster WHERE ProdCode = @ProdCode";
                            using (SqlCommand fetchCommand = new SqlCommand(fetchQuery, sqlConnection))
                            {
                                fetchCommand.Parameters.AddWithValue("@ProdCode", ProdCode);
                                object result = fetchCommand.ExecuteScalar();
                                return result != null ? Convert.ToInt32(result) : -3;
                            }
                        }
                        else
                        {
                            return -4; // No rows updated, possibly invalid ProdCode
                        }
                    }
                }
                catch (Exception Ex)
                {
                    throw new ArgumentException("Error updating NextNumber");
                    return -2;
                }
            }
        }



        public static string GetCostCenterFromStoreCode(string storeCode)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(inventoryConnectionString))
                {
                    sqlConnection.Open();
                    return GetCostCenterFromStoreCode(storeCode, sqlConnection);
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error or show an appropriate message
                return storeCode;
            }
        }

        public static string GetCostCenterFromStoreCode(string storeCode, SqlConnection sqlConnection)
        {
            try
            {
                string query = "SELECT StockCc FROM tblStore WHERE StoreCode = @StoreCode";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@StoreCode", storeCode);

                    object result = command.ExecuteScalar();

                    if (result == null || Convert.IsDBNull(result))
                    {
                        return storeCode;
                    }
                    else
                    {
                        return Convert.ToString(result);
                    }
                }
            }
            catch (Exception ex)
            {
                return storeCode;
            }
        }


        public bool authorizeCheckerRolefromCore(string username)
        {
            using (OdbcConnection odbcConnection = new OdbcConnection(db2ConnectionString))
            {
                odbcConnection.Open();
                string commandText = "SELECT BFUSERIDPK,BFGROUPNAMEPK from BANKFUSION.BFTB_USERPERMISSIONVIEW where BFGROUPNAMEPK in ('SPSBAccountant','BRManager','BRAManager','FINDVM') and BFUSERIDPK='" + username + "'";

                using (OdbcCommand command = new OdbcCommand(commandText, odbcConnection))
                {
                    command.CommandType = CommandType.Text;
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        public bool UpdateOTP(string objUsers, string otp)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MMSContext"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string strUpdate = @"UPDATE [UserMaster] SET OTP = @OTP WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(strUpdate, connection))
                {
                    command.Parameters.Add(new SqlParameter("@Username", objUsers));
                    command.Parameters.Add(new SqlParameter("@OTP", otp));
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            using (BinaryReader reader = new BinaryReader(image.InputStream))
            {
                return reader.ReadBytes((int)image.ContentLength);
            }
        }

        public class SavingStatus
        {
            public string StatusID { get; set; }
            public string Description { get; set; }
        }

        public class Category
        {
            public string CatID { get; set; }
            public string Description { get; set; }
        }

        public class Store
        {
            public string StoreCode { get; set; }
            public string Description { get; set; }
        }

        public class Item
        {
            public string ItemID { get; set; }
            public string Description { get; set; }
        }

        public class Status
        {
            public string code { get; set; }
            public string description { get; set; }
        }

        public IEnumerable<Category> GetCategories()
        {
            return new List<Category>
            {
                new Category{CatID="1", Description = "Fixed Asset Category"},
                new Category{CatID="2", Description = "Stationary Category"},
                new Category{CatID="3", Description = "Other Category"},
                new Category{CatID="4", Description = "Software Category"},
            };
        }

        public IEnumerable<Item> GetItems()
        {
            return new List<Item>
            {
                new Item{ItemID="1", Description = "Fixed Asset Item"},
                new Item{ItemID="2", Description = "Stationary Item"},
                new Item{ItemID="3", Description = "Other Item"},
                new Item{ItemID="4", Description = "Software Item"},
            };
        }



        public List<SelectListItem> GetYears()
        {
            var list = new List<SelectListItem>();
            for (var i = 2019; i < 4099; i++)
            {
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            return list;
        }

        public List<SelectListItem> GetYears1()
        {
            var now = DateTime.Now.Year;
            var list = new List<SelectListItem>();
            for (var i = now - 1; i <= now; i++)
            {
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            return list;
        }


        public class ABData
        {
            public ABData(string connectionString)
            {
                // Initialize your connection here
            }

            public void OpenConnection()
            {
                // Open database connection logic
            }

            public void CloseConnection()
            {
                // Close database connection logic
            }

            public object GetDataScalar(string query)
            {
                return null;
            }
        }
    }
}