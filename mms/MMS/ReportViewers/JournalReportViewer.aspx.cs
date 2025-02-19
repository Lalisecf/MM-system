using System;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using MMS.Models.Inventory;
using System.Configuration;

namespace MMS
{
    public partial class JournalReportViewer : System.Web.UI.Page
    {
        private string _connectionString = ConfigurationManager.ConnectionStrings["INVENTORYConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["MenuTitle"] = "Journal Report";

            if (!IsPostBack)
            {
                if (Request.QueryString["Id"] != null)
                {
                    string refNo = Request.QueryString["Id"];
                    string ydt = Request.QueryString["Ydt"];                    
                    LoadReport(refNo, ydt);
                }
            }
        }

        private void LoadReport(string refNo, string Ydt)
        {
            try
            {
                var journalData = GetJournalData(refNo, Ydt);
                Session["Success"] = "Successfully Processed.";
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/JournalReport.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", journalData));
                ReportViewer1.LocalReport.Refresh();
            }
            catch (Exception ex)
            {
                Session["Error"] = "An error occurred while processing the report.";
                Response.Redirect("~/Error/Error");
            }
        }

        private IEnumerable<JournalVM> GetJournalData(string refNo, string ydt)
        {
            var journalDetails = new List<JournalVM>();
            string query = "";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (ydt == "closedperiod")
                {
                    query = @"SELECT RefNo, Naration, 
                               CAST(ISNULL(Debit, 0) AS Decimal(18,2)) AS TotalAmount, 
                               ISNULL(CostAccountNo, 'N/A') AS CostAccountNo, 
                               ISNULL(StockAccountNo, 'N/A') AS StockAccountNo, 
                               ISNULL(PeriodId, 0) AS PeriodId 
                        FROM TblJournalYtd  WHERE RefNo = @RefNo";
                }
                else
                {
                    query = @"SELECT RefNo, Naration, 
                               CAST(ISNULL(Debit, 0) AS Decimal(18,2)) AS TotalAmount, 
                               ISNULL(CostAccountNo, 'N/A') AS CostAccountNo, 
                               ISNULL(StockAccountNo, 'N/A') AS StockAccountNo, 
                               ISNULL(PeriodId, 0) AS PeriodId 
                        FROM TblJournal WHERE RefNo = @RefNo";
                }
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RefNo", refNo);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            journalDetails.Add(new JournalVM
                            {
                                RefNo = reader.GetString(0),
                                Department = reader.GetString(1),
                                TotalAmount = reader.GetDecimal(2),
                                CostAccountNo = reader.GetString(3),
                                StockAccountNo = reader.GetString(4),
                                UserName = Session["UserName"].ToString(),
                                Period = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }

            return journalDetails;
        }
    }
}
