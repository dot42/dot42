using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Dot42;
using log4net.Config;
using log4net.Ext.EventID;

using RestInterface;

namespace RestService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [RestServiceLogging("Data Service")]
    public class DataService : IDataTest
    {
        /// <summary>
        /// Logging component 
        /// </summary>
        private static readonly IEventIDLog _log = EventIDLogManager.GetLogger(typeof(DataService));

        static DataService()
        {
            var folder = Thread.GetDomain().BaseDirectory;
            var filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".config";
            var fullPath = Path.Combine(folder, filename);

            XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPath));
        }

        #region Implementation of IDataTest

        public MyEnum GetMyEnum()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return MyEnum.Two;
        }

        public int GetInt()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return 42;
        }

        public void CreateInt(int myInt)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
        }

        public int UpdateInt(int myInt)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return myInt;
        }

        public int[] GetInts()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return new []{24, 42};
        }

        public void CreateInts(int[] myInts)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
        }

        public int[] UpdateInts(int[] myInts)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return myInts;
        }

        #region all other datatypes

        public bool UpdateBool(bool obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public bool[] UpdateBools(bool[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public byte UpdateByte(byte obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public byte[] UpdateBytes(byte[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public char UpdateChar(char obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public char[] UpdateChars(char[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public DateTime UpdateDateTime(DateTime obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public DateTime[] UpdateDateTimes(DateTime[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public double UpdateDouble(double obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public double[] UpdateDoubles(double[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public Guid UpdateGuid(Guid obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public Guid[] UpdateGuids(Guid[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public short UpdateShort(short obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public short[] UpdateShorts(short[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public long UpdateLong(long obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public long[] UpdateLongs(long[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public sbyte UpdateSByte(sbyte obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public sbyte[] UpdateSBytes(sbyte[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public float UpdateFloat(float obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public float[] UpdateFloats(float[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public string UpdateString(string obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public string[] UpdateStrings(string[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public TimeSpan UpdateTimeSpan(TimeSpan obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public TimeSpan[] UpdateTimeSpans(TimeSpan[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public ushort UpdateUShort(ushort obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public ushort[] UpdateUShorts(ushort[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public uint UpdateUInt(uint obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public uint[] UpdateUInts(uint[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public ulong UpdateULong(ulong obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public ulong[] UpdateULongs(ulong[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        #endregion

        public MyEnum UpdateMyEnum(MyEnum obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public MyEnum[] UpdateMyEnums(MyEnum[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public MyStruct UpdateMyStruct(MyStruct obj)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return obj;
        }

        public MyStruct[] UpdateMyStructs(MyStruct[] objs)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return objs;
        }

        public SimpleObject GetSimpleObject()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
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

        private SimpleObject[] GetSimpleObjects()
        {
            return new[] { GetSimpleObject(), GetSimpleObject() };
        }

        public void CreateSimpleObject(SimpleObject simpleObject)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
        }

        public SimpleObject UpdateSimpleObject(SimpleObject simpleObject)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return simpleObject;
        }

        public ComplexObject GetComplexObject()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
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

        public void CreateComplexObject(ComplexObject complexObject)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
        }

        public ComplexObject UpdateComplexObject(ComplexObject complexObject)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return complexObject;
        }

        public ComplexObject[] GetComplexObjects()
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return new []{ GetComplexObject(), GetComplexObject()};
        }

        public void CreateComplexObjects(ComplexObject[] complexObjects)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
        }

        public ComplexObject[] UpdateComplexObjects(ComplexObject[] complexObjects)
        {
            SetHttpStatus(HttpStatusCode.OK, "OK");
            return complexObjects;
        }

        #endregion

        #region REST related

        private static void SetHttpStatus(HttpStatusCode statusCode, string message)
        {
            if (WebOperationContext.Current == null)
            {
                throw new Exception("Unable to get current WebOperationContext.");
            }

            WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = message;
        }

        #endregion
    }
}
