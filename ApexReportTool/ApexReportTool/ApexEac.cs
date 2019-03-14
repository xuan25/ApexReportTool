using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ApexReportTool
{
    /// <summary>
    /// A class for submit reports to Apex Easy Anti-Cheat
    /// Author: Xuan525
    /// Date: 08/03/2019
    /// </summary>
    class ApexEac
    {
        public class GetVerificationException : Exception
        {
            public GetVerificationException() : base() { }
        }

        public class InvalidParameterException : Exception
        {
            public InvalidParameterException(string message) : base(message) { }
        }

        private const string PAGE_URL = "https://www.easy.ac/zh-tw/support/apexlegends/contact/report/";
        private const string POST_URL = "https://www.easy.ac/api/v1/contact/report/apexlegends/";
        private const string TOKEN_REGEX = "<input id=\"csrf_token\" name=\"csrf_token\" type=\"hidden\" value=\"(?<token>.+?)\">";

        private class Verification
        {
            public CookieCollection CookieCollection;
            public string Token;

            public Verification(string token, CookieCollection cookieCollection)
            {
                CookieCollection = cookieCollection;
                Token = token;
            }
        }

        private static Verification GetVerification()
        {
            try
            {
                string result = "";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(PAGE_URL);
                req.Method = "GET";
                req.CookieContainer = new CookieContainer();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();

                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                Match match = Regex.Match(result, TOKEN_REGEX);
                if (match.Success)
                    return new Verification(match.Groups["token"].Value, resp.Cookies);
                else
                    throw new GetVerificationException();
            }
            catch (Exception)
            {
                throw new GetVerificationException();
            }
            
        }

        private static string PostForm(Dictionary<string, string> dic, CookieCollection cookieCollection)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(POST_URL);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(cookieCollection);

            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
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
            return result;
        }

        /// <summary>
        /// Submit an report to ApexEac
        /// </summary>
        /// <param name="playerId">Id of the user (not the player you want to report)</param>
        /// <param name="firstName">First name of the user</param>
        /// <param name="lastName">Last name of the user</param>
        /// <param name="email">Email of the user</param>
        /// <param name="message">Details of what you want to report</param>
        public static bool Submit(string playerId, string firstName, string lastName, string email, string message)
        {
            Verification verification = GetVerification();

            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("player_id", playerId);
            dic.Add("first_name", firstName);
            dic.Add("last_name", lastName);
            dic.Add("email", email);
            dic.Add("csrf_token", verification.Token);
            dic.Add("message", message);

            string result = PostForm(dic, verification.CookieCollection);

            if (!result.Contains("\"success\":true"))
                throw new InvalidParameterException(result);
            return true;
        }
    }
}
