using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Json;

namespace ApexReportTool
{
    /// <summary>
    /// A class for EA interface
    /// Author: Xuan525
    /// Date: 14/03/2019
    /// </summary>
    class EA
    {
        public class TokenExpiredException : Exception
        {
            public TokenExpiredException() : base() { }
        }
        public class PlayerNotFoundException : Exception
        {
            public PlayerNotFoundException() : base() { }
        }
        public class ReportFaildException : Exception
        {
            public ReportFaildException(string message) : base(message) { }
        }

        private string Token;
        private long UserId;

        public EA(string token)
        {
            Token = token;
            UserId = 0;
        }

        public long GetEAId()
        {
            if(UserId == 0)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string result = "";
                string url = "https://gateway.ea.com/proxy/identity/pids/me";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Headers.Add("Authorization", "Bearer " + Token);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                dynamic json = JsonParser.Parse(result);
                try
                {
                    UserId = json.pid.pidId;
                }
                catch (Exception)
                {
                    throw new TokenExpiredException();
                }
            }
            return UserId;
        }

        public string GetEAId(string searchTerm)
        {
            string userId = GetEAId().ToString();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string result = "";
            string url = string.Format("https://api3.origin.com/xsearch/users?userId={0}&searchTerm={1}&start=0", userId, searchTerm);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Headers.Add("authtoken", Token);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            try
            {
                dynamic json = JsonParser.Parse(result);
                return json.infoList[0].friendUserId;
            }
            catch (Exception)
            {
                throw new PlayerNotFoundException();
            }
            
        }

        public bool ReportCheat(string reportUsername, string comment)
        {
            string userId = GetEAId().ToString();
            string reportId = GetEAId(reportUsername);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string result = "";
            string url = string.Format("https://api2.origin.com/atom/users/{0}/reportUser/{1}", userId, reportId);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "text/plain;charset=UTF-8";
            req.Headers.Add("Origin", "https://www.origin.com");
            req.Headers.Add("X-Origin-Platform", "PCWIN");
            req.Headers.Add("AuthToken", Token);

            byte[] data = Encoding.UTF8.GetBytes(string.Format("<reportUser>\r\n<contentType>In Game</contentType>\r\n<reportReason>Cheating</reportReason>\r\n<comments>{0}</comments>\r\n</reportUser>", comment));
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            if (result == "<result>success</result>")
                return true;
            else
                throw new ReportFaildException(result);
        }
    }
}
