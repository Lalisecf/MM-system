using System.IO;
using System.Net;
using System.Web;

namespace SharedLayer.AB_Common
{
    public class OZIKEAPI
    {
        public string SendMessage(string Lottertnumber, string mobileNumber)
        {
            string msg = "";
            string ozSURL = "http://10.10.12.142"; //where the SMS Gateway is running System.Web.HttpUtility
            string ozSPort = "9501"; //port number where the SMS Gateway is listening
            string ozMstUser = HttpUtility.UrlEncode("MMSusr"); //MstUsername for successful login
            string ozPassw = HttpUtility.UrlEncode("X72bT2VGcUdbVr9"); //MstUser's password
            string ozMessageType = "SMS:TEXT"; //type of message
            string ozRecipients = HttpUtility.UrlEncode(mobileNumber); //who will get the message
            string ozMessageData = HttpUtility.UrlEncode(Lottertnumber); //body of message

            string createdURL = ozSURL + ":" + ozSPort + "/httpapi" +
                "?action=sendMessage" +
                "&MstUsername=" + ozMstUser +
                "&password=" + ozPassw +
                "&messageType=" + ozMessageType +
                "&recipient=" + ozRecipients +
                "&messageData=" + ozMessageData;

            try
            {
                //Create the request and send data to the SMS Gateway Server by HTTP connection
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(createdURL);
                HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                StreamReader respStreamReader = new StreamReader(myResp.GetResponseStream());
                string responseString = respStreamReader.ReadToEnd();
                respStreamReader.Close();
                myResp.Close();
                msg = responseString;
            }

            catch (WebException ex)
            {
                string message = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
            return msg;
        }
    }
}
