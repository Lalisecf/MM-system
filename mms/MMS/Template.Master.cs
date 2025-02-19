using SharedLayer.Models;
using System;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace Master
{
    public partial class Template : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var user = Session["UserName"]?.ToString();
            var userid = Session["UserId"]?.ToString();
            var branchName = Session["BranchName"]?.ToString();
            var roleName = Session["RoleName"]?.ToString();
            lblTitle.Text = Session["MenuTitle"] == null ? "" : Session["MenuTitle"].ToString();
            if (user == null || userid == null || branchName == null || roleName == null)
            {
                Response.Redirect("~/Login/Login");
            }
        }
       
    }
}