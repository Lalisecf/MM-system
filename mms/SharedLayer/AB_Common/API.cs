using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.Data.SqlClient;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharedLayer.AB_Common
{
    public class API
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MasterDBConnectionString"].ConnectionString;
            }
        }
        public class Token
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
            public string MstUserName { get; set; }

            [JsonProperty("expires_in")]
            public long ExpiresIn { get; set; }

            [JsonProperty(".issued")]
            public DateTime Issued { get; set; }
            [JsonProperty(".expires")]
            public DateTime Expires { get; set; }
        }

        public class Employee
        {
            public string Id { get; set; }
            public string empId { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string gender { get; set; }
            public string empName { get; set; }
            public string position { get; set; }
            public string branchcode { get; set; }
            public double salary { get; set; }
            public string unit { get; set; }
            public string amount { get; set; }
            public string benefitType { get; set; }
            public string gradeCode { get; set; }
            public string isHO { get; set; }
            public string birthDate { get; set; }
            public string employmentDate { get; set; }
            public string beginDate { get; set; }
            public string status { get; set; }
        }
        public class EmployeeParent
        {
            public string empId { get; set; }
            public string supervisorId { get; set; }
            public string chief { get; set; }
            public string deputyChief { get; set; }
            public string directorate { get; set; }
            public string deputyDirectorate { get; set; }
            public string unit { get; set; }
        }
        public class ADMstUsers
        {
            public string title { get; set; }
            public string mail { get; set; }
            public string givenName { get; set; }
            public string department { get; set; }
            public string displayName { get; set; }
        }

        public class Employeeold
        {
            public Guid Id { get; set; }
            public string EmployeeId { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string empName { get; set; }
            public string LastName { get; set; }
            public string Gender { get; set; }
            public string JobTitle { get; set; }
            public double salary { get; set; }
            public string Unit { get; set; }
            public string grade { get; set; }
            public DateTime HiringDate { get; set; }
            public DateTime dateOfBirth { get; set; }

        }

        public List<Employee> EmployeeList()
        {
            string result = "";
            List<Employee> employees = new List<Employee>();
            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("https://hr.awashbank.com/hr/api/getEmpInfo_data");
            request2.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse())
            using (Stream stream = response2.GetResponseStream())
            using (StreamReader reader22 = new StreamReader(stream))
            {
                result = reader22.ReadToEnd();
            }
            employees = JsonConvert.DeserializeObject<List<Employee>>(result).Where(c => c.status == "0").ToList();
            return employees = JsonConvert.DeserializeObject<List<Employee>>(result).Where(c => c.status == "0").ToList(); ;
        }
        public List<Employee> EmployeeBenefit()
        {
            string result = "";
            List<Employee> employees = new List<Employee>();
            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("https://hr.awashbank.com/hr/api/benefit");
            request2.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse())
            using (Stream stream = response2.GetResponseStream())
            using (StreamReader reader22 = new StreamReader(stream))
            {
                result = reader22.ReadToEnd();
            }
            employees = JsonConvert.DeserializeObject<List<Employee>>(result);
            return employees = JsonConvert.DeserializeObject<List<Employee>>(result);
        }
        public List<EmployeeParent> EmployeeListNew()
        {
            string result;
            List<EmployeeParent> employees;
            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("https://hr.awashbank.com/hr/api/emp_supervisor");
            request2.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse())
            using (Stream stream = response2.GetResponseStream())
            using (StreamReader reader22 = new StreamReader(stream))
            {
                result = reader22.ReadToEnd();
            }

            employees = JsonConvert.DeserializeObject<List<EmployeeParent>>(result);
            return employees;
        }

        public string GetADUser(string Username, string password)
        {
            try
            {
                string ldapServer = "LDAP://awash.local/dc=awash,dc=local";
                using (System.DirectoryServices.DirectoryEntry connection = new System.DirectoryServices.DirectoryEntry(ldapServer, Username, password))
                {
                    using (var search = new DirectorySearcher(connection)
                    {
                        Filter = "(samaccountname=" + Username + ")",
                        PropertiesToLoad = { "samaccountname" }
                    })
                    {
                        var result = search.FindOne();
                        if (result == null)
                        {
                            throw new Exception("User not found.");
                        }

                        return (string)result.Properties["samaccountname"][0];
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle as needed
                throw new Exception("Bad Request: " + ex.Message, ex);
            }
        }

        public string GetADUser1(string MstUser)
        {
            // MstUsername and password for LDAP authentication
            string ldapServer = "LDAP://awash.local/dc=awash,dc=local";
            string MstUsername = "MMS";
            string password = "Bfub@aib1";
            using (DirectoryEntry connection = new DirectoryEntry(ldapServer, MstUsername, password))
            {
                using (var search = new DirectorySearcher(connection)
                {
                    Filter = "(samaccountname=" + MstUser + ")",
                    PropertiesToLoad = { "samaccountname" },
                })
                {
                    try
                    {
                        return (string)search.FindOne().Properties["samaccountname"][0];
                    }
                    catch (Exception ex)
                    {
                        return "400";
                    }

                }
            }
        }

        public string GetADMstUsername(string MstUser)
        {
            // MstUsername and password for LDAP authentication
            string ldapServer = "LDAP://awash.local/dc=awash,dc=local";
            string MstUsername = "MMS";
            string password = "Bfub@aib1";
            // Create a DirectoryEntry object to bind to LDAP      
            using (DirectoryEntry connection = new DirectoryEntry(ldapServer, MstUsername, password))
            {
                using (var search = new DirectorySearcher(connection)
                {
                    Filter = "(samaccountname=" + MstUser + ")",
                    PropertiesToLoad = { "samaccountname" },
                })
                {
                    try
                    {
                        return (string)search.FindOne().Properties["samaccountname"][0];
                    }
                    catch (Exception ex)
                    {
                        return "400";
                    }

                }
            }
        }
        public string GetADMstUser1(string MstUser)
        {
            // MstUsername and password for LDAP authentication
            string ldapServer = "LDAP://awash.local/dc=awash,dc=local";
            string MstUsername = "MMS";
            string password = "Bfub@aib1";
            List<ADMstUsers> list = new List<ADMstUsers>();
            using (DirectoryEntry connection = new DirectoryEntry(ldapServer, MstUsername, password))
            {
                using (var search = new DirectorySearcher(connection)
                {
                    Filter = "(samaccountname=" + MstUser + ")",
                    PropertiesToLoad = { "mail" },
                })
                {
                    return (string)search.FindOne().Properties["mail"][0];
                }
            }
        }
        public static string GetIPAddress()
        {
            HttpContext context = HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        public static string GetNumericOTP()
        {
            const string chars = "0123456789";

            Random random = new Random();
            StringBuilder otp1 = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                int index = random.Next(chars.Length);
                otp1.Append(chars[index]);
            }
            var otp = otp1.ToString();
            return otp;
        }

        public static string GetComputerName()
        {
            HttpContext context = HttpContext.Current;
            string MstUserName = Dns.GetHostEntry(context.Request.ServerVariables["REMOTE_ADDR"]).HostName;
            return MstUserName;
        }
        public List<Employee> EmployeeList_old()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://10.10.12.149:93/hrwebapi/token");
            request.Method = "POST";
            //string content = "grant_type=password&MstUsername=MMSMstUser&password=Hananu@123";
            string content = "grant_type=password&MstUsername=MMSMstUser&password=MMSPassword@989";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(content);
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);

            //WebResponse response = request.GetResponse();
            //newStream.Write(byte1, 0, byte1.Length);

            var response = (HttpWebResponse)request.GetResponse();

            string token = "";
            using (var stream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(stream))
                {
                    token = sr.ReadToEnd();
                }
            }

            Token tkn = JsonConvert.DeserializeObject<Token>(token);
            HttpWebRequest mainRequest = (HttpWebRequest)WebRequest.Create("http://10.10.12.149:93/hrwebapi/api/MMSEmployeeInfo");
            mainRequest.Method = "GET";

            mainRequest.ContentLength = 0;
            mainRequest.Accept = "application/json";
            mainRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + tkn.AccessToken);


            var mainResponse = (HttpWebResponse)mainRequest.GetResponse();
            string result = string.Empty;
            List<Employee> employees = new List<Employee>();
            using (var stream = mainResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            employees = JsonConvert.DeserializeObject<List<Employee>>(result);
            return employees = JsonConvert.DeserializeObject<List<Employee>>(result);


        }
        public bool SendEmail(string receiver, string msg)
        {
            try
            {
                MailMessage Msg = new MailMessage();
                Msg.From = new MailAddress("MMS@awashbank.com");
                Msg.Subject = "MMS System Verification Code";

                Msg.To.Add(receiver);
                Msg.Body = msg;
                Msg.IsBodyHtml = true;
                Msg.Priority = MailPriority.High;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "webmail.awashbank.com";
                smtp.Port = 25;
                smtp.Credentials = new NetworkCredential("awash'\'MMS", "Bfub@aib1", "awash");
                smtp.EnableSsl = false;
                smtp.Send(Msg);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void IncrementFailedAttempts(string Username)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("IncrementFailedAttempts", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void ResetFailedAttempts(string Username)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("ResetFailedAttempts", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}