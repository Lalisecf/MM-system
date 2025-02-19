using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using static SharedLayer.AB_Common.SystemTools;

namespace SharedLayer.AB_Common
{
    public enum ParameterType
    {
        Department = 1,
        ProductGroup,
        NameValue
    }

    public class Utility
    {
        public static string InventoryConnectionString =>
            ConfigurationManager.ConnectionStrings["INVENTORYConnectionString"]?.ConnectionString
            ?? throw new InvalidOperationException("Inventory connection string is not configured.");

        public static string MasterConnectionString =>
            ConfigurationManager.ConnectionStrings["MasterDBConnectionString"]?.ConnectionString
            ?? throw new InvalidOperationException("MasterDB connection string is not configured.");

        public static string GetValue(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new ArgumentException("Parameter name cannot be null, empty, or whitespace.", nameof(paramName));
            }

            string connectionString = ConfigurationManager.ConnectionStrings["INVENTORYConnectionString"]?.ConnectionString
                ?? throw new InvalidOperationException("Inventory connection string is not configured.");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string query = "SELECT ParameterValue FROM tblNameValue WHERE ParameterName = @ParameterName";
                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@ParameterName", paramName);

                        object result = command.ExecuteScalar();
                        return Convert.ToString(result) ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
            }
        }

        public static int PeriodID(string period)
        {
            return Convert.ToInt32(period.Substring(period.Length - 2));
        }
        public static DateTime GetServerDate()
        {
            DateTime serverDate;
            ABData dataObj = new ABData(MasterConnectionString);
            try
            {
                dataObj.OpenConnection();
                serverDate = GetServerDate(dataObj);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
            finally
            {
                dataObj.CloseConnection();
            }
            return serverDate;
        }
        public static DateTime GetServerDate(ABData DataObj)
        {
            DateTime time;
            try
            {
                string commandText = "SELECT getdate()";
                object dataScalar = DataObj.GetDataScalar(commandText);
                time = (dataScalar == null) ? new DateTime(0x76c, 1, 1) : Convert.ToDateTime(dataScalar);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
            return time;
        }
        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');

                if (addresses.Length > 0)
                {
                    return addresses[0].Trim();
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static string GetValue(ref ABData DataObj, string paramName)
        {
            try
            {
                string result = Convert.ToString(DataObj.GetDataScalar("SELECT ParameterValue FROM tblNameValue WHERE ParameterName='" + paramName + "'"));
                return result;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return string.Empty;
            }
        }

        public static string GetMonthName(int periodId)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                string result = Convert.ToString(DataObj.GetDataScalar("SELECT PeriodName FROM PeriodSetup where PeriodId=" + periodId + ""));
                return result;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return string.Empty;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetNarationByDocNo(string docNo)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                string result = Convert.ToString(DataObj.GetDataScalar("SELECT top 1 Naration FROM tblJournal where DocNo='" + docNo + "'"));
                return result;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return string.Empty;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetUnitOfMeasur(string prodCode)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                string result = Convert.ToString(DataObj.GetDataScalar("select UnitMeas from tblItemMaster where ProdCode='" + prodCode + "'"));
                return result;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return string.Empty;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetNarationByReference(string refNo)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                string result = Convert.ToString(DataObj.GetDataScalar("SELECT top 1 Naration FROM tblJournal where RefNo='" + refNo + "'"));
                return result;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return string.Empty;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetTransactionName(string RefPrefix)
        {
            string tranDesc = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("select Description from tblTransactionType where Prefix='" + RefPrefix + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    tranDesc = Convert.ToString(result);
                }
                return tranDesc;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        //public static string GetNextNumber(int tranCode, string infix)
        //{
        //    return GetNextNumber(tranCode, infix, string.Empty);
        //}

        public async Task<string> GetNextNumbersAsync(int tranCode, string infix, string suffix)
        {
            const string query = @"
        SELECT 
            (Prefix + '-' +
            CASE WHEN @Infix IS NOT NULL AND @Infix <> '' THEN @Infix + '-' ELSE '' END +
            CASE WHEN @Suffix IS NOT NULL AND @Suffix <> '' THEN @Suffix + '-' ELSE '' END +
            CAST(NextNumber AS VARCHAR(10))) AS NextNumber
        FROM tblTransactionType
        WHERE TranCode = @TranCode";

            try
            {
                using (var connection = new SqlConnection(InventoryConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TranCode", tranCode);
                        command.Parameters.AddWithValue("@Infix", (object)infix ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Suffix", (object)suffix ?? DBNull.Value);

                        var result = await command.ExecuteScalarAsync();

                        return result != null ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetNextNumberAsync: {ex.Message}", ex);
            }
        }

        public static string GetNextNumberBuild(int tranCode, string infix, string suffix)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            string cmdText = null;
            object result = null;
            try
            {
                DataObj.OpenConnection();
                cmdText = "SELECT (Prefix+'-' +";
                if (infix != string.Empty)
                    cmdText += "'" + infix + "'+'-'+";
                if (suffix != string.Empty)
                    cmdText += "'" + suffix + "'+'-'+";
                cmdText += "cast(NextNumber As varchar(10))";
                cmdText += ") AS NextNumber FROM tblTransactionType WHERE TranCode=" + 19;

                result = DataObj.GetDataScalar(cmdText);
                if (result != null)
                    return result.ToString();
                else
                    return null;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowError(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetNextNumberSoft(int tranCode, string infix, string suffix)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            string cmdText = null;
            object result = null;
            try
            {
                DataObj.OpenConnection();
                cmdText = "SELECT (Prefix+'-' +";
                if (infix != string.Empty)
                    cmdText += "'" + infix + "'+'-'+";
                if (suffix != string.Empty)
                    cmdText += "'" + suffix + "'+'-'+";
                cmdText += "cast(NextNumber As varchar(10))";
                cmdText += ") AS NextNumber FROM tblTransactionType WHERE TranCode=" + 18;

                result = DataObj.GetDataScalar(cmdText);
                if (result != null)
                    return result.ToString();
                else
                    return null;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowError(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }
        public static string Qchar(string str)
        {
            return ((str != null) ? ("'" + str.Replace("'", "`").Trim() + "'") : "''");
        }

        public static string GetNextNumberAsync(int tranCode, string infix, string suffix)
        {
            const string query = @"
        SELECT 
            (Prefix + '-' +
            CASE WHEN @Infix IS NOT NULL AND @Infix <> '' THEN @Infix + '-' ELSE '' END +
            CASE WHEN @Suffix IS NOT NULL AND @Suffix <> '' THEN @Suffix + '-' ELSE '' END +
            CAST(NextNumber AS VARCHAR(10))) AS NextNumber
        FROM tblTransactionType
        WHERE TranCode = @TranCode";

            try
            {
                using (var connection = new SqlConnection(InventoryConnectionString))
                {
                    connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TranCode", tranCode);
                        command.Parameters.AddWithValue("@Infix", (object)infix ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Suffix", (object)suffix ?? DBNull.Value);

                        var result =  command.ExecuteScalarAsync();

                        return result != null ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetNextNumberAsync: {ex.Message}", ex);
            }
        }



        public static string GetRefPrifix(int TranCode)
        {
            string refPrifix = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("select Prefix from tblTransactionType where TranCode=" + TranCode + "");
                if (result != null && !Convert.IsDBNull(result))
                {
                    refPrifix = Convert.ToString(result);
                }

                return refPrifix;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string ConvertToEthiopicDate(DateTime gregorianDate)
        {
            string result = null;
            EthiopianCalendar cal = new EthiopianCalendar();
            result = cal.ConvertgregorianToEthiopic(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day);
            return result;
        }

        public static string ConvertToGregorianDate(string ethiopianDate)
        {
            if (ethiopianDate.Contains("' '"))
                ethiopianDate.Remove(ethiopianDate.IndexOf(' '));
            string result = null, day, month, year;
            day = ethiopianDate.Split(new char[] { '/' })[0];
            month = ethiopianDate.Split(new char[] { '/' })[1];
            year = ethiopianDate.Split(new char[] { '/' })[2];

            EthiopianCalendar cal = new EthiopianCalendar();
            result = cal.ConvertethiopicToGregorian(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            return result;
        }

        public static int ConvertToEthiopicYear(int gregorianYear)
        {
            EthiopianCalendar cal = new EthiopianCalendar(gregorianYear, 1, 1);
            return cal.gregorianToEthiopic(gregorianYear, 1, 1)[0];
        }

        public static string GetStockAccount(string storeCode, string prodCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT StockAccountSegment FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }

                result = DataObj.GetDataScalar("select StockCc from tblStore where StoreCode='" + storeCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = resultAccount + Convert.ToString(result);
                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetStoreAccount(string storeCode, string prodCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT AccountNo FROM tblStore WHERE StoreCode='" + storeCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetStoreCode(string ReqNo)
        {
            string resultStore = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT StoreCode FROM tblRequest WHERE ReqNo='" + ReqNo + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultStore = Convert.ToString(result);
                }

                return resultStore;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }


        public static string GetCostAccountClamable(string deptCode, string prodCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object deptType = DataObj.GetDataScalar("SELECT DeptType FROM tbldepartments WHERE DeptCode='" + deptCode + "'");
                object result = DataObj.GetDataScalar("SELECT ExpenseAccountSegment FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                object result1 = DataObj.GetDataScalar("SELECT IFBExpenseAccount FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                if (Convert.ToInt64(deptType) == 1)
                {
                    if (result != null && !Convert.IsDBNull(result))
                    {
                        resultAccount = Convert.ToString(result);
                    }

                    result = DataObj.GetDataScalar("SELECT DeptCc FROM tbldepartments WHERE DeptCode=" + deptCode);
                    if (result != null && !Convert.IsDBNull(result))
                    {
                        resultAccount += Convert.ToString(result);
                    }
                }
                else if (Convert.ToInt64(deptType) == 2)
                {
                    if (result1 != null && !Convert.IsDBNull(result1))
                    {
                        resultAccount = Convert.ToString(result1);
                    }

                    result1 = DataObj.GetDataScalar("SELECT DeptCc FROM tbldepartments WHERE DeptCode=" + deptCode);
                    if (result1 != null && !Convert.IsDBNull(result1))
                    {
                        resultAccount += Convert.ToString(result1);
                    }
                }
                else
                {

                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }
        public static string GetCostAccountSpecial(string deptCode, string prodCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT ExpenseAccountSegment FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }

                result = DataObj.GetDataScalar("SELECT DeptCc FROM tbldepartments WHERE DeptCode=" + deptCode);
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount += Convert.ToString(result);
                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }
        public static string GetCostAccount1(string deptCode, string prodCode, string mainpg)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object deptType = DataObj.GetDataScalar("SELECT DeptType FROM tbldepartments WHERE DeptCode='" + deptCode + "'");
                object result = DataObj.GetDataScalar("SELECT ExpenseAccountSegment FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                object result1 = DataObj.GetDataScalar("SELECT IFBExpenseAccount FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                if (Convert.ToInt64(deptType) == 1)
                {
                    if (result != null && !Convert.IsDBNull(result))
                    {
                        resultAccount = Convert.ToString(result);
                    }
                    if (mainpg != null && mainpg == "AB")
                    {
                        result = DataObj.GetDataScalar("SELECT MainCostCenter FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "BF")
                    {
                        result = DataObj.GetDataScalar("SELECT Furniture FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "GX")
                    {
                        result = DataObj.GetDataScalar("SELECT OfficeEquip FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "CP")
                    {
                        result = DataObj.GetDataScalar("SELECT MotorVehicle FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else
                    {
                        result = null;
                    }
                    if (result != null && !Convert.IsDBNull(result))
                    {
                        resultAccount += Convert.ToString(result);
                    }
                }
                else if (Convert.ToInt64(deptType) == 2)
                {
                    if (result1 != null && !Convert.IsDBNull(result1))
                    {
                        resultAccount = Convert.ToString(result1);
                    }
                    if (mainpg != null && mainpg == "AB")
                    {
                        result1 = DataObj.GetDataScalar("SELECT MainCostCenter FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "BF")
                    {
                        result1 = DataObj.GetDataScalar("SELECT Furniture FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "GX")
                    {
                        result1 = DataObj.GetDataScalar("SELECT OfficeEquip FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else if (mainpg != null && mainpg == "CP")
                    {
                        result1 = DataObj.GetDataScalar("SELECT MotorVehicle FROM tbldepartments WHERE DeptCode=" + deptCode);
                    }
                    else
                    {
                        result1 = null;
                    }
                    if (result1 != null && !Convert.IsDBNull(result1))
                    {
                        resultAccount += Convert.ToString(result1);
                    }

                }
                else
                {

                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }


        public static string GetCostCenter1(string deptCode, string mainpg)
        {
            string resultAccount = string.Empty;
            object result = "";
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                if (mainpg != null && mainpg == "SO")
                {
                    result = DataObj.GetDataScalar("SELECT MainCostCenter FROM tbldepartments WHERE DeptCode=" + deptCode);
                    if (result != null && !Convert.IsDBNull(result))
                    {
                        resultAccount = Convert.ToString(result);
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
                //AppMessageBox.ShowExclamation(Ex.Message);
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetCostAccount(string deptCode, string prodCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT ExpenseAccountSegment FROM tblItemMaster WHERE ProdCode='" + prodCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }

                result = DataObj.GetDataScalar("SELECT MainCostCenter FROM tbldepartments WHERE DeptCode=" + deptCode);
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount += Convert.ToString(result);
                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }


        public static string GetCostCenter(string deptCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT MainCostCenter FROM tblDepartments WHERE DeptCode='" + deptCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }
                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }


        public static string GetCostCenterClamable(string deptCode)
        {
            string resultAccount = string.Empty;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                object result = DataObj.GetDataScalar("SELECT DeptCc FROM tblDepartments WHERE DeptCode='" + deptCode + "'");
                if (result != null && !Convert.IsDBNull(result))
                {
                    resultAccount = Convert.ToString(result);
                }

                return resultAccount;
            }
            catch (Exception Ex)
            {
                return null;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }





        public static bool IsValidAccountNumber(string AccountNo)
        {
            return true;
        }



        public static int ReqStage(string reqNo)
        {
            string cmdText;
            object result;
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                cmdText = "select Status from tblRequest where ReqNo='" + reqNo + "'";
                result = DataObj.GetDataScalar(cmdText);
                if (result == null || Convert.IsDBNull(result))
                    return -1;
                else
                    return Convert.ToInt32(result);
            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);
                return -1;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetParameterValueByKeyField(string parametername, string returntype)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                return GetParameterValueByKeyField(parametername, returntype, DataObj);
            }
            catch (Exception ex)
            {
                //AppMessageBox.ShowError(ex.Message);
                return string.Empty;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string GetParameterValueByKeyField(string parametername, string returntype, ABData DataObj)
        {
            object result;
            try
            {
                result = DataObj.GetDataScalar("select ParameterValue from tblNameValue where ParameterName='" + parametername + "'");
                if (result == null || Convert.IsDBNull(result))
                    return (returntype == "Number" ? "0" : string.Empty);
                else
                    return Convert.ToString(result);

            }
            catch (Exception ex)
            {
                //AppMessageBox.ShowError("Unable to get parameter value for key field " + parametername + ": \n" + ex.Message);

                return (returntype == "Number" ? "0" : string.Empty);
            }

        }

        public static string getCostCenterFromStoreCode(string storeCode)
        {
            ABData DataObj = new ABData(InventoryConnectionString);
            try
            {
                DataObj.OpenConnection();
                return getCostCenterFromStoreCode(storeCode, DataObj);
            }
            catch (Exception ex)
            {
                //AppMessageBox.ShowError(ex.Message);
                return storeCode;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }

        public static string getCostCenterFromStoreCode(string storeCode, ABData DataObj)
        {
            object result;
            try
            {
                result = DataObj.GetDataScalar("select StockCc from tblStore where StoreCode='" + storeCode + "'");
                if (result == null || Convert.IsDBNull(result))
                    return storeCode;
                else
                    return Convert.ToString(result);

            }
            catch (Exception ex)
            {
                //AppMessageBox.ShowError("Unable to get Stock Cost center for store code " + storeCode + ": \n" + ex.Message);

                return storeCode;
            }

        }
        public static string GetSalesTaxCC()
        {

            return "";
        }
        public static bool IsValidePage(string UserId, string URL)
        {
            object result;
            string cmdText;
            ABData DataObj = new ABData(MasterConnectionString);
            try
            {
                DataObj.OpenConnection();
                cmdText = "SELECT HMDW.NavigateUrl FROM MstMenuDefWeb  HMDW inner join MstRoleMenuRights HMRW on HMDW.MenuCode=HMRW.MenuCode  where  HMDW.NavigateUrl like '" + URL + "%' and HMRW.RoleId=" + UserId;
                // cmdText = "SELECT HMDW.NavigateUrl FROM MstMenuDefWeb  HMDW inner join MstRoleMenuRights HMRW on HMDW.MenuCode=HMRW.MenuCode  where HMRW.MenuCode in (1,4,13,23,39,51,65,78) and HMDW.NavigateUrl like '" + URL + "%' and HMRW.RoleId=" + UserId;
                result = DataObj.GetDataScalar(cmdText);
                if (result == null || Convert.IsDBNull(result))
                    return false;
                else
                    return true;

            }
            catch (Exception Ex)
            {
                //AppMessageBox.ShowExclamation(Ex.Message);0972559496
                return false;
            }
            finally
            {
                DataObj.CloseConnection();
            }
        }
    }
}