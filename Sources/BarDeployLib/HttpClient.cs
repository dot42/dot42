using System;
using System.Net;
using System.Net.Security;
using System.Xml.Linq;
using Dot42.BarDeployLib.Entity;

namespace Dot42.BarDeployLib
{
    /// <summary>
    /// HTTP helper
    /// </summary>
    internal sealed class HttpClient
    {
        private readonly CookieContainer cookies = new CookieContainer();

        static HttpClient()
        {
            // Accept all SSL server connections
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        /// <summary>
        /// Get a response for the given URL.
        /// </summary>
        internal HttpWebResponse Get(Uri uri)
        {
            var request = Initialize(WebRequest.Create(uri));
            request.Method = "GET";
            return (HttpWebResponse) request.GetResponse();
        }

        /// <summary>
        /// Get a response for the given URL.
        /// </summary>
        internal XDocument GetAsXml(Uri uri)
        {
            var response = Get(uri);
            return XDocument.Load(response.GetResponseStream());
        }

        /// <summary>
        /// Post data onto the given URL and return the response.
        /// </summary>
        internal HttpWebResponse Post(Uri uri, MultipartEntity entity)
        {
            var request = Initialize(WebRequest.Create(uri));
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentLength = entity.ContentLength;
            request.ContentType = entity.ContentType;
            var postData = entity.Content;
            request.GetRequestStream().Write(postData, 0, postData.Length);
            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary> 
        /// Initialize a webrequest.
        /// </summary>
        private HttpWebRequest Initialize(WebRequest request)
        {
            var httpRequest = (HttpWebRequest) request;
            httpRequest.AuthenticationLevel = AuthenticationLevel.None;            
            httpRequest.Timeout = 10000;
            httpRequest.UserAgent = "QNXWebClient/1.0";
            httpRequest.CookieContainer = cookies;
            httpRequest.Credentials = CredentialCache.DefaultCredentials;
            return httpRequest;
        }
    }
}
