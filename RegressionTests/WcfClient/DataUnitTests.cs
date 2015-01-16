using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

using NUnit.Framework;

using RestInterface;

namespace RestService
{
    [TestFixture]
    public partial class DataUnitTests
    {
        private string _hostAddress;

        [SetUp]
        public void SetupTest()
        {
            _hostAddress = string.Format("http://{0}:9222/RestService/DataService", ipAddress);
        }

        #region Create primitive types

        [Test]
        public void CreateInt()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
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
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateInts( new []{24,42});
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        #endregion

        #region Create complex types

        [Test]
        public void CreateSimpleObject()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
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
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
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
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.CreateComplexObjects(GetComplexObjects());
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        #endregion

        #region Update primitive types

        [Test]
        public void UpdateBool()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = false;
                    var result = channel.UpdateBool(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);

                    input = true;
                    result = channel.UpdateBool(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateBools()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new [] { true, false, true };
                    var result = channel.UpdateBools(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateByte()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (byte)0x42;
                    var result = channel.UpdateByte(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateBytes()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new byte[] { 24, 42, 33 };
                    var result = channel.UpdateBytes(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateChar()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = 't';
                    var result = channel.UpdateChar(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateChars()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { 'd', 'o', 't' };
                    var result = channel.UpdateChars(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateDateTime()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = DateTime.Now;
                    var result = channel.UpdateDateTime(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateDateTimes()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new [] {DateTime.Now, DateTime.Now.AddDays(4).AddHours(2)};
                    var result = channel.UpdateDateTimes(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateDouble()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = 42.12345;
                    var result = channel.UpdateDouble(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateDoubles()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { 12.345, 34.567 };
                    var result = channel.UpdateDoubles(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateFloat()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = 42.12345f;
                    var result = channel.UpdateFloat(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateFloats()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { 12.345f, 34.567f };
                    var result = channel.UpdateFloats(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        //[Test]
        public void UpdateGuid()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = Guid.NewGuid();
                    var result = channel.UpdateGuid(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        //[Test]
        public void UpdateGuids()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { Guid.NewGuid(), Guid.NewGuid() };
                    var result = channel.UpdateGuids(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateInt()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = 42;
                    var result = channel.UpdateInt(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateInts()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { 42, 24, 33 };
                    var result = channel.UpdateInts(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateLong()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = 42L;
                    var result = channel.UpdateLong(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateLongs()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { 42L, 24L, 33L };
                    var result = channel.UpdateLongs(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateSByte()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (sbyte)0x12;
                    var result = channel.UpdateSByte(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateSBytes()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { (sbyte)42, (sbyte)24, (sbyte)33 };
                    var result = channel.UpdateSBytes(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateShort()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (short)12;
                    var result = channel.UpdateShort(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateShorts()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { (short)42, (short)24, (short)33 };
                    var result = channel.UpdateShorts(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateString()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = "dot42";
                    var result = channel.UpdateString(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateStrings()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { "dot42", "dot24", "dot33" };
                    var result = channel.UpdateStrings(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateTimeSpan()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new TimeSpan(1,2,3,4,5);
                    var result = channel.UpdateTimeSpan(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateTimeSpans()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { new TimeSpan(1,2,3,4,5), new TimeSpan(4,4,5,6), new TimeSpan(3,3,3,3) };
                    var result = channel.UpdateTimeSpans(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateUInt()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (uint)42;
                    var result = channel.UpdateUInt(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateUInts()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { (uint)42, (uint)24, (uint)33 };
                    var result = channel.UpdateUInts(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateULong()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (ulong)42;
                    var result = channel.UpdateULong(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateULongs()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { (ulong)42, (ulong)24, (ulong)33 };
                    var result = channel.UpdateULongs(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateUShort()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = (ushort)42;
                    var result = channel.UpdateUShort(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateUShorts()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = new[] { (ushort)42, (ushort)24, (ushort)33 };
                    var result = channel.UpdateUShorts(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input.Length, result.Length);
                    Assert.AreEqual(input, result);
                }
            }
        }

        #endregion

        #region Update complex types

        [Test]
        public void UpdateMyEnum()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetMyEnum();
                    var result = channel.UpdateMyEnum(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateMyEnums()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetMyEnums();
                    var result = channel.UpdateMyEnums(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    Assert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateMyStruct()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetMyStruct();
                    var result = channel.UpdateMyStruct(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    MyAssert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateMyStructs()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetMyStructs();
                    var result = channel.UpdateMyStructs(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    MyAssert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateSimpleObject()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetSimpleObject();
                    var result = channel.UpdateSimpleObject(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    MyAssert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateComplexObject()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetComplexObject();
                    var result = channel.UpdateComplexObject(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    MyAssert.AreEqual(input, result);
                }
            }
        }

        [Test]
        public void UpdateComplexObjects()
        {
            using (var client = new WebChannelFactory<IDataTest>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var input = GetComplexObjects();
                    var result = channel.UpdateComplexObjects(input);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                    MyAssert.AreEqual(input, result);
                }
            }
        }

        #endregion

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
            /*
            var currentWebOperationContext = WebOperationContext.Current;
            if (currentWebOperationContext == null)
                throw new ApplicationException("Failed to determine current WebOperationContext");

            var webResponse = currentWebOperationContext.IncomingResponse;
            Assert.AreEqual(httpStatusCode, webResponse.StatusCode, string.Format("Unexpected HttpStatusCode received : {0}", webResponse.StatusDescription));
            */
        }

        #endregion

        #region Simple / complex class helpers

        private static MyEnum GetMyEnum()
        {
            return MyEnum.Two;
        }

        private static MyEnum[] GetMyEnums()
        {
            return new[] { MyEnum.One, MyEnum.Two, MyEnum.Three };
        }

        private static MyStruct GetMyStruct()
        {
            return new MyStruct()
                {
                    MyBool = true,
                    MyByte = 42
                };
        }

        private static MyStruct[] GetMyStructs()
        {
            return new[] {GetMyStruct(), GetMyStruct()};
        }

        private static SimpleObject GetSimpleObject()
        {
            return new SimpleObject()
                {
                    MyEnum = GetMyEnum(),
                    MyStruct = GetMyStruct(),
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
                    MyTimeSpan = new TimeSpan(1,2,3,4,5),
                    MyUInt16 = 12,
                    MyUInt32 = 123,
                    MyUInt64 = 1234
                };
        }

        private static SimpleObject[] GetSimpleObjects()
        {
            return new[] {GetSimpleObject(), GetSimpleObject()};
        }

        private static ComplexObject GetComplexObject()
        {
            return new ComplexObject()
                {
                    MyEnums = GetMyEnums(),
                    MyStructs = GetMyStructs(),
                    MultipleBool = new bool[] {true, false},
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
            return new[] { GetComplexObject() };
        }

        internal static class MyAssert
        {
            public static void AreEqual(MyStruct[] expected, MyStruct[] actual)
            {
                Assert.AreEqual(expected.Length, actual.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    AreEqual(expected[i], actual[i]);
                }
            }

            public static void AreEqual(MyStruct expected, MyStruct actual)
            {
                Assert.AreNotEqual(expected, actual);

                Assert.AreEqual(expected.MyBool, actual.MyBool);
                Assert.AreEqual(expected.MyByte, actual.MyByte);
            }

            public static void AreEqual(SimpleObject[] expected, SimpleObject[] actual)
            {
                Assert.AreEqual(expected.Length, actual.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    AreEqual(expected[i], actual[i]);
                }
            }

            public static void AreEqual(SimpleObject expected, SimpleObject actual)
            {
                Assert.AreNotEqual(expected, actual);

                Assert.AreEqual(expected.MyBool, actual.MyBool);
                Assert.AreEqual(expected.MyByte, actual.MyByte);
                Assert.AreEqual(expected.MyChar, actual.MyChar);
                Assert.AreEqual(expected.MyDateTime, actual.MyDateTime);
                Assert.AreEqual(expected.MyDouble, actual.MyDouble);
                //[verify error with Guid, see case: 1306000763] Assert.AreEqual(expected.MyGuid, actual.MyGuid);
                Assert.AreEqual(expected.MyInt16, actual.MyInt16);
                Assert.AreEqual(expected.MyInt32, actual.MyInt32);
                Assert.AreEqual(expected.MyInt64, actual.MyInt64);
                Assert.AreEqual(expected.MySByte, actual.MySByte);
                Assert.AreEqual(expected.MySingle, actual.MySingle);
                Assert.AreEqual(expected.MyString, actual.MyString);
                Assert.AreEqual(expected.MyTimeSpan, actual.MyTimeSpan);
                Assert.AreEqual(expected.MyUInt16, actual.MyUInt16);
                Assert.AreEqual(expected.MyUInt32, actual.MyUInt32);
                Assert.AreEqual(expected.MyUInt64, actual.MyUInt64);
            }

            public static void AreEqual(ComplexObject[] expected, ComplexObject[] actual)
            {
                Assert.AreEqual(expected.Length, actual.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    AreEqual(expected[i], actual[i]);
                }
            }

            public static void AreEqual(ComplexObject expected, ComplexObject actual)
            {
                Assert.AreNotEqual(expected, actual);

                Assert.AreEqual(expected.MultipleBool, actual.MultipleBool);
                Assert.AreEqual(expected.MultipleByte, actual.MultipleByte);
                Assert.AreEqual(expected.MultipleChar, actual.MultipleChar);
                Assert.AreEqual(expected.MultipleDateTime, actual.MultipleDateTime); 
                Assert.AreEqual(expected.MultipleDouble, actual.MultipleDouble);
                //[verify error with Guid, see case: 1306000763] Assert.AreEqual(expected.MultipleGuid, actual.MultipleGuid);
                Assert.AreEqual(expected.MultipleInt16, actual.MultipleInt16);
                Assert.AreEqual(expected.MultipleInt32, actual.MultipleInt32);
                Assert.AreEqual(expected.MultipleInt64, actual.MultipleInt64); 
                Assert.AreEqual(expected.MultipleSByte, actual.MultipleSByte);
                Assert.AreEqual(expected.MultipleSingle, actual.MultipleSingle);
                Assert.AreEqual(expected.MultipleString, actual.MultipleString);
                Assert.AreEqual(expected.MultipleTimeSpan, actual.MultipleTimeSpan);
                Assert.AreEqual(expected.MultipleUInt16, actual.MultipleUInt16);
                Assert.AreEqual(expected.MultipleUInt32, actual.MultipleUInt32);
                Assert.AreEqual(expected.MultipleUInt64, actual.MultipleUInt64);
             
                AreEqual(expected.MultipleSimpleObjects, actual.MultipleSimpleObjects);
                Assert.AreEqual(expected.MyInt, actual.MyInt);
                AreEqual(expected.SimpleObject, actual.SimpleObject);
            }
        }

        #endregion
    }
}
