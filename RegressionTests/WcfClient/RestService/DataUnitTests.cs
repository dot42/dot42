using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

using NUnit.Framework;

using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;
using RestInterface;

namespace RestService
{
    //[TestFixture]
    public class DataUnitTests
    {
        private const string _hostAddress = "http://localhost:9222/RestService/DataService";

        [Test]
        public void CreateInt()
        {
            using (var server = new WebServiceHost( new DataService(), new Uri(_hostAddress)))
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                server.Open();
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateInt(42);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        [Test]
        public void CreateInts()
        {
            using (var server = new WebServiceHost(new DataService(), new Uri(_hostAddress)))
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                server.Open();
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateInts( new []{24,42});
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        [Test]
        public void CreateSimpleObject()
        {
            using (var server = new WebServiceHost(new DataService(), new Uri(_hostAddress)))
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                server.Open();
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateSimpleObject(GetSimpleObject());
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        [Test]
        public void CreateComplexObject()
        {
            using (var server = new WebServiceHost(new DataService(), new Uri(_hostAddress)))
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                server.Open();
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateComplexObject(GetComplexObject());
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        [Test]
        public void CreateComplexObjects()
        {
            using (var server = new WebServiceHost(new DataService(), new Uri(_hostAddress)))
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                server.Open();
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateComplexObjects(GetComplexObjects());
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }


        private static SimpleObject GetSimpleObject()
        {
            return new SimpleObject()
            {
                MyBool = true,
                MyByte = 42,
                MyChar = 't',
                MyDateTime = DateTime.Now,
                MyDouble = 33,
                //[verify error with Guid, see case: 1306000763] MyGuid = Guid.NewGuid(),
                MyInt16 = 78,
                MyInt32 = 123,
                MyInt64 = 123456,
                MySByte = 2,
                MySingle = 7,
                MyString = "dot42",
                MyTimeSpan = new TimeSpan(1, 2, 3, 4, 5),
                MyUInt16 = 12,
                MyUInt32 = 123,
                MyUInt64 = 1234
            };
        }

        private static SimpleObject[] GetSimpleObjects()
        {
            return new[] { GetSimpleObject(), GetSimpleObject() };
        }

        private static ComplexObject GetComplexObject()
        {
            return new ComplexObject()
            {
                MultipleBool = new bool[] { true, false },
                MultipleByte = new byte[] { 12, 43 },
                MultipleChar = new char[] { 'd', '0', 't' },
                MultipleDateTime = new DateTime[] { DateTime.Now, DateTime.Now.Subtract(TimeSpan.FromHours(12)) },
                MultipleDouble = new double[] { 12.34, 56.78 },
                //[verify error with Guid, see case: 1306000763] MultipleGuid = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },
                MultipleInt16 = new short[] { -12, -42 },
                MultipleInt32 = new int[] { -33, 66 },
                MultipleInt64 = new long[] { 123456789, 987654321 },
                MultipleSByte = new sbyte[] { 2, 5 },
                MultipleSingle = new float[] { 12.34f, 56.789f },
                MultipleString = new string[] { "hoi", "robert" },
                MultipleTimeSpan = new TimeSpan[] { TimeSpan.FromMinutes(12), TimeSpan.FromHours(2) },
                MultipleUInt16 = new ushort[] { 12, 34 },
                MultipleUInt32 = new uint[] { 56, 890 },
                MultipleUInt64 = new ulong[] { 123, 456789 },
                MyInt = 42,
                SimpleObject = GetSimpleObject(),
                MultipleSimpleObjects = GetSimpleObjects()
            };
        }

        private static ComplexObject[] GetComplexObjects()
        {
            return new[] { GetComplexObject(), GetComplexObject() };
        }


        #region Private HttpStatus Response Methods

        private struct HttpStatusResponse
        {
            public HttpStatusCode HttpStatusCode { get; private set; }
            public string HttpStatusDescription { get; private set; }

            public HttpStatusResponse(HttpStatusCode httpStatusCode, string httpStatusDescription)
                : this()
            {
                HttpStatusCode = httpStatusCode;
                HttpStatusDescription = httpStatusDescription;
            }
        }

        /// <summary>
        /// Determine the HttpStatusResponse from the protocol exception. 
        /// When this fails the Assert.Fail method will be called to prevent further execution.
        /// </summary>
        /// <param name="communicationException">The protocol exception which might the HttpStatusCode</param>
        /// <returns>The HttpStatusResponse enumeration</returns>
        private static HttpStatusResponse DetermineHttpStatusResponse(CommunicationException communicationException)
        {
            if (communicationException != null && communicationException.InnerException != null)
            {
                var webException = communicationException.InnerException as WebException;
                if (webException != null && webException.Response != null)
                {
                    var response = webException.Response as HttpWebResponse;
                    if (response != null)
                    {
                        return new HttpStatusResponse(response.StatusCode, response.StatusDescription);
                    }
                }
            }

            Assert.Fail("Failed to determine HttpStatusCode from CommunicationException.");
            return new HttpStatusResponse(HttpStatusCode.InternalServerError, "Failed to determine HttpStatusCode from CommunicationException.");
        }

        /// <summary>
        /// Validate the communicationException to see whether the HttpStatusCode matches the expectation
        /// </summary>
        /// <param name="communicationException"></param>
        /// <param name="httpStatusCode"></param>
        private static void ValidateHttpStatusResponse(CommunicationException communicationException, HttpStatusCode httpStatusCode)
        {
            HttpStatusResponse httpStatusResponse = DetermineHttpStatusResponse(communicationException);
            Assert.AreEqual(httpStatusCode, httpStatusResponse.HttpStatusCode, string.Format("Unexpected HttpStatusCode received : {0}", httpStatusResponse.HttpStatusDescription));
        }

        /// <summary>
        /// Validate the communicationException to see whether the HttpStatusCode matches the expectation
        /// </summary>
        /// <param name="httpStatusCode"></param>
        private static void ValidateHttpStatusResponse(HttpStatusCode httpStatusCode)
        {
            var currentWebOperationContext = WebOperationContext.Current;
            if (currentWebOperationContext == null)
                throw new ApplicationException("Failed to determine current WebOperationContext");

            var webResponse = currentWebOperationContext.IncomingResponse;
            Assert.AreEqual(httpStatusCode, webResponse.StatusCode, string.Format("Unexpected HttpStatusCode received : {0}", webResponse.StatusDescription));
        }

        #endregion
    }
}
