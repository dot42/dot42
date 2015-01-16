using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace RestInterface
{
    [DataContract]
    public enum MyEnum 
    {
        One,
        Two,
        Three
    }

    [DataContract]
    public struct MyStruct
    {
        [DataMember]
        public bool MyBool { get; set; }

        [DataMember]
        public byte MyByte { get; set; }
    }

    [DataContract]
    public class SimpleObject
    {
        [DataMember]
        public MyEnum MyEnum { get; set; }

        [DataMember]
        public MyStruct MyStruct { get; set; }

        [DataMember]
        public bool MyBool { get; set; }

        [DataMember]
        public byte MyByte { get; set; }

        [DataMember]
        public char MyChar { get; set; }

        [DataMember]
        public DateTime MyDateTime { get; set; }

        [DataMember]
        public double MyDouble { get; set; }

        /* verify error with Guid, see case: 1306000763
        [DataMember]
        public Guid MyGuid { get; set; }
        */

        [DataMember]
        public Int16 MyInt16 { get; set; }
        
        [DataMember]
        public Int32 MyInt32 { get; set; }

        [DataMember]
        public Int64 MyInt64 { get; set; }

        [DataMember]
        public SByte MySByte { get; set; }

        [DataMember]
        public Single MySingle { get; set; }

        [DataMember]
        public string MyString { get; set; }

        [DataMember]
        public TimeSpan MyTimeSpan { get; set; }

        [DataMember]
        public UInt16 MyUInt16 { get; set; }

        [DataMember]
        public UInt32 MyUInt32 { get; set; }

        [DataMember]
        public UInt64 MyUInt64 { get; set; }
    }

    [DataContract]
    public class ComplexObject
    {
        [DataMember]
        public MyEnum[] MyEnums { get; set; }

        [DataMember]
        public MyStruct[] MyStructs { get; set; }

        [DataMember(Name = "RenamedInt", IsRequired = true, EmitDefaultValue = false)]
        public int MyInt { get; set; }

        [DataMember]
        public bool[] MultipleBool { get; set; }

        [DataMember]
        public byte[] MultipleByte { get; set; }

        [DataMember]
        public char[] MultipleChar { get; set; }

        [DataMember]
        public DateTime[] MultipleDateTime { get; set; }

        [DataMember]
        public double[] MultipleDouble { get; set; }

        /* verify error with Guid, see case: 1306000763
        [DataMember]
        public Guid[] MultipleGuid { get; set; }
        */

        [DataMember]
        public Int16[] MultipleInt16 { get; set; }

        [DataMember]
        public Int32[] MultipleInt32 { get; set; }

        [DataMember]
        public Int64[] MultipleInt64 { get; set; }

        [DataMember]
        public SByte[] MultipleSByte { get; set; }

        [DataMember]
        public Single[] MultipleSingle { get; set; }

        [DataMember]
        public string[] MultipleString { get; set; }

        [DataMember]
        public TimeSpan[] MultipleTimeSpan { get; set; }

        [DataMember]
        public UInt16[] MultipleUInt16 { get; set; }

        [DataMember]
        public UInt32[] MultipleUInt32 { get; set; }

        [DataMember]
        public UInt64[] MultipleUInt64 { get; set; }

        [DataMember]
        public SimpleObject SimpleObject { get; set; }

        [DataMember]
        public SimpleObject[] MultipleSimpleObjects { get; set; }
     }

    [ServiceContract]
    [DataContractFormat]
    public interface IDataTest
    {
        #region Get and Create some types

        [OperationContract]
        [WebGet(UriTemplate = "/int", ResponseFormat = WebMessageFormat.Json)]
        int GetInt();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/int", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CreateInt(int myInt);

        [OperationContract]
        [WebGet(UriTemplate = "/ints", ResponseFormat = WebMessageFormat.Json)]
        int[] GetInts();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/ints", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CreateInts(int[] myInts);

        [OperationContract]
        [WebGet(UriTemplate = "/enum", ResponseFormat = WebMessageFormat.Json)]
        MyEnum GetMyEnum();

        #endregion

        #region update all primitive types

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/bool", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        bool UpdateBool(bool obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/bools", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        bool[] UpdateBools(bool[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/byte", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        byte UpdateByte(byte obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/bytes", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        byte[] UpdateBytes(byte[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/char", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        char UpdateChar(char obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/chars", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        char[] UpdateChars(char[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/datetime", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        DateTime UpdateDateTime(DateTime obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/datetimes", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        DateTime[] UpdateDateTimes(DateTime[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/double", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        double UpdateDouble(double obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/doubles", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        double[] UpdateDoubles(double[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/guid", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Guid UpdateGuid(Guid obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/guids", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Guid[] UpdateGuids(Guid[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/short", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        short UpdateShort(short obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/shorts", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        short[] UpdateShorts(short[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/int", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        int UpdateInt(int myInt);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ints", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        int[] UpdateInts(int[] myInts);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/long", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        long UpdateLong(long obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/longs", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        long[] UpdateLongs(long[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/sbyte", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        sbyte UpdateSByte(sbyte obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/sbytes", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        sbyte[] UpdateSBytes(sbyte[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/single", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        float UpdateFloat(float obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/singles", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        float[] UpdateFloats(float[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/string", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string UpdateString(string obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/strings", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string[] UpdateStrings(string[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/timespan", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        TimeSpan UpdateTimeSpan(TimeSpan obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/timespans", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        TimeSpan[] UpdateTimeSpans(TimeSpan[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ushort", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ushort UpdateUShort(ushort obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ushorts", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ushort[] UpdateUShorts(ushort[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/uint", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        uint UpdateUInt(uint obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/uints", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        uint[] UpdateUInts(uint[] objs);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ulong", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ulong UpdateULong(ulong obj);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ulongs", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ulong[] UpdateULongs(ulong[] objs);

        #endregion

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/MyEnum", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        MyEnum UpdateMyEnum(MyEnum myEnum);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/MyEnums", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        MyEnum[] UpdateMyEnums(MyEnum[] myEnums);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/MyStruct", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        MyStruct UpdateMyStruct(MyStruct myStruct);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/MyStructs", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        MyStruct[] UpdateMyStructs(MyStruct[] myStructs);

        [OperationContract]
        [WebGet(UriTemplate = "/SimpleObject", ResponseFormat = WebMessageFormat.Json)]
        SimpleObject GetSimpleObject();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SimpleObject", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CreateSimpleObject(SimpleObject simpleObject);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/SimpleObject", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        SimpleObject UpdateSimpleObject(SimpleObject simpleObject);

        [OperationContract]
        [WebGet(UriTemplate = "/ComplexObject", ResponseFormat = WebMessageFormat.Json)]
        ComplexObject GetComplexObject();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/ComplexObject", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CreateComplexObject(ComplexObject complexObject);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ComplexObject", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ComplexObject UpdateComplexObject(ComplexObject complexObject);

        [OperationContract]
        [WebGet(UriTemplate = "/ComplexObjects", ResponseFormat = WebMessageFormat.Json)]
        ComplexObject[] GetComplexObjects();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/ComplexObjects", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CreateComplexObjects(ComplexObject[] complexObjects);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/ComplexObjects", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        ComplexObject[] UpdateComplexObjects(ComplexObject[] complexObjects);
    }
}
