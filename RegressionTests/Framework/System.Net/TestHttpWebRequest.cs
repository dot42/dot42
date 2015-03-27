using System.IO;
using System.Net;
using Uri = System.Uri;

namespace Dot42.Tests.System.Net
{
    class TestHttpWebRequest : AndroidWifiBasedTestCase
    {
        public void testWebRequestGet()
        {
            var uri = new Uri("http://www.google.com");
            using(var stream  = WebInvoke( uri, Verb.Get, null,true))
            {
                AssertTrue(stream.Length > 100);
            }
        }

        public void testSync()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.google.com");
            AssertNotNull("req:If Modified Since: ", req.IfModifiedSince);

            req.UserAgent = "MonoClient v1.0";
            AssertEquals("#A1","User-Agent".ToLower(), req.Headers.GetKey(0));
            AssertEquals("#A2", "MonoClient v1.0", req.Headers.Get(0));

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            AssertEquals("#B1", "OK", res.StatusCode.ToString());
            AssertEquals("#B2", "OK", res.StatusDescription);

            AssertEquals("#C1", "text/html; charset=ISO-8859-1", res.Headers.Get("Content-Type"));
            AssertNotNull("#C2", res.LastModified);
            //Assert.AreEqual(0, res.Cookies.Count, "#C3");

            res.Close();
        }

       

        private enum Verb
        {
            Get,
            Post,
            Put,
            Delete
        }

        private static Stream WebInvoke(Uri uri, Verb verb, Stream input, bool hasOutput)
        {
            var request = WebRequest.Create(uri);
            switch (verb)
            {
                case Verb.Get:
                    request.Method = "GET";
                    break;
                case Verb.Post:
                    request.Method = "POST";
                    break;
                case Verb.Put:
                    request.Method = "PUT";
                    break;
                case Verb.Delete:
                    request.Method = "DELETE";
                    break;
            }

            WebResponse response;

            if (input != null)
            {
                input.Seek(0, SeekOrigin.Begin);

                request.ContentType = "application/xml";
                request.ContentLength = input.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    var bytes = new byte[1024];
                    var read = 1;
                    while (read > 0)
                    {
                        read = input.Read(bytes, 0, bytes.Length);
                        requestStream.Write(bytes, 0, read);
                    }
                }
            }

            using (response = request.GetResponse())
			{
				if (!hasOutput)
					return null;
				var temp = new MemoryStream();
				response.GetResponseStream().CopyTo(temp);
				temp.Seek(0L, SeekOrigin.Begin);
				return temp;
			}
        }

        protected override void RunConnectivityTest()
        {
            testWebRequestGet();
        }
    }
}
