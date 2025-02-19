using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using MMS.Models.Inventory;
using System.Configuration;

namespace MMS
{
    public partial class PurchaseReportViewer : System.Web.UI.Page
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["INVENTORYConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["MenuTitle"] = "Purchase Report";

            if (!IsPostBack)
            {
                if (Request.QueryString["Id"] != null)
                {
                    string refNo = Request.QueryString["Id"];
                    LoadReport(refNo);
                }
            }
        }

        private void LoadReport(string refNo)
        {
            try
            {
                var purchasedData = GetPurchaseData(refNo);
                Session["Success"] = "Successfully Processed.";

                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/PurchaseReport.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", purchasedData));
                ReportViewer1.LocalReport.Refresh();
            }
            catch (Exception ex)
            {
                Session["Error"] = "An error occurred while processing the report.";
                Response.Redirect("~/Error/Error");
            }
        }

        private IEnumerable<PurchaseVM> GetPurchaseData(string refNo)
        {
            var purchaseDetails = new List<PurchaseVM>();
            string query = @"
        SELECT * 
        FROM ViewTranDt 
        WHERE RefNo = @RefNo;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RefNo", refNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var purchase = new PurchaseVM
                            {
                                RefNo = reader["RefNo"].ToString(),
                                ProdCode = reader["ProdCode"].ToString(),
                                ProdDesc = reader["ProdDesc"].ToString(),
                                RefDate = reader.GetDateTime(reader.GetOrdinal("RefDate")),
                                SuppInvNo = reader["SuppInvNo"].ToString(),
                                StoreName = reader["StoreName"].ToString(),
                                SupplierName = reader["SupplierName"].ToString(),
                                SupplierAcct = reader["SupplierAcct"].ToString(),
                                MainPG = reader["MainPG"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Type = reader["Type"].ToString(),
                                Qty = reader.IsDBNull(reader.GetOrdinal("Qty")) ? 0 : reader.GetInt32(reader.GetOrdinal("Qty")),
                                UnitCost = reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Description = reader["Description"].ToString(),
                                CrtBy = Session["UserName"]?.ToString() ?? string.Empty 
                            };

                            purchaseDetails.Add(purchase);
                        }
                    }
                }
            }

            return purchaseDetails;
        }
    }
}
