/*
 * Copyright (C) 2008 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dot42.DdmLib.log
{
    /// <summary>
    /// Represents an event and its data.
    /// </summary>
    public class EventContainer
    {
        public enum CompareMethods
        {
            EQUAL_TO,
            LESSER_THAN,
            LESSER_THAN_STRICT,
            GREATER_THAN,
            GREATER_THAN_STRICT,
            BIT_CHECK
        }

        /// <summary>
        /// Comparison method for <seealso cref="EventContainer#testValue(int, Object, com.android.ddmlib.log.EventContainer.CompareMethod)"/>
        /// 
        /// </summary>
        public class CompareMethod
        {
            public static readonly CompareMethod EQUAL_TO = new CompareMethod(CompareMethods.EQUAL_TO, "equals", "==");
            public static readonly CompareMethod LESSER_THAN = new CompareMethod(CompareMethods.LESSER_THAN, "less than or equals to", "<=");
            public static readonly CompareMethod LESSER_THAN_STRICT = new CompareMethod(CompareMethods.LESSER_THAN_STRICT, "less than", "<");
            public static readonly CompareMethod GREATER_THAN = new CompareMethod(CompareMethods.GREATER_THAN, "greater than or equals to", ">=");
            public static readonly CompareMethod GREATER_THAN_STRICT = new CompareMethod(CompareMethods.GREATER_THAN_STRICT, "greater than", ">");
            public static readonly CompareMethod BIT_CHECK = new CompareMethod(CompareMethods.BIT_CHECK, "bit check", "&");

            private readonly CompareMethods method;
            private readonly string mName;
            private readonly string mTestString;

            private CompareMethod(CompareMethods method, string name, string testString)
            {
                this.method = method;
                mName = name;
                mTestString = testString;
            }

            public CompareMethods Method { get { return method; } }
            public string Name { get { return mName; } }

            /// <summary>
            /// Returns the display string.
            /// </summary>
            public override string ToString()
            {
                return mName;
            }

            /// <summary>
            /// Returns a short string representing the comparison.
            /// </summary>
            public string testString()
            {
                return mTestString;
            }
        }


        /// <summary>
        /// Type for event data.
        /// </summary>
        public enum EventValueTypes
        {
            UNKNOWN = 0,
            INT = 1,
            LONG = 2,
            STRING = 3,
            LIST = 4,
            TREE = 5
        }

        public static class EventValueType
        {
            private static readonly Regex STORAGE_PATTERN = new Regex("^(\\d+)@(.*)$"); //$NON-NLS-1$

            /// <summary>
            /// Returns a <seealso cref="EventValueType"/> from an integer value, or <code>null</code> if no match
            /// was found. </summary>
            /// <param name="value"> the integer value. </param>
            internal static EventValueTypes getEventValueType(int value)
            {
                return
                    Enum.GetValues(typeof (EventValueTypes)).Cast<EventValueTypes>().FirstOrDefault(
                        x => (int) x == value);
            }

            /// <summary>
            /// Returns a storage string for an <seealso cref="Object"/> of type supported by
            /// <seealso cref="EventValueType"/>.
            /// <p/>
            /// Strings created by this method can be reloaded with
            /// <seealso cref="#getObjectFromStorageString(String)"/>.
            /// <p/>
            /// NOTE: for now, only <seealso cref="#STRING"/>, <seealso cref="#INT"/>, and <seealso cref="#LONG"/> are supported. </summary>
            /// <param name="object"> the object to "convert" into a storage string. </param>
            /// <returns> a string storing the object and its type or null if the type was not recognized. </returns>
            public static String getStorageString(object value)
            {
                if (value is string)
                {
                    return ((int) EventValueTypes.STRING) + "@" + (string) value; //$NON-NLS-1$
                }
                else if (value is int)
                {
                    return ((int) EventValueTypes.INT) + "@" + value; //$NON-NLS-1$
                }
                else if (value is long)
                {
                    return ((int) EventValueTypes.LONG) + "@" + value; //$NON-NLS-1$
                }

                return null;
            }

            /// <summary>
            /// Creates an <seealso cref="Object"/> from a storage string created with
            /// <seealso cref="#getStorageString(Object)"/>. </summary>
            /// <param name="value"> the storage string </param>
            /// <returns> an <seealso cref="Object"/> or null if the string or type were not recognized. </returns>
            public static Object getObjectFromStorageString(String value)
            {
                var m = STORAGE_PATTERN.Match(value);
                if (m.Success)
                {
                    try
                    {
                        var type = getEventValueType(int.Parse(m.Groups[0].Value));

                        if (type == EventValueTypes.UNKNOWN)
                        {
                            return null;
                        }

                        switch (type)
                        {
                            case EventValueTypes.STRING:
                                return m.Groups[1].Value;
                            case EventValueTypes.INT:
                                return int.Parse(m.Groups[1].Value);
                            case EventValueTypes.LONG:
                                return long.Parse(m.Groups[1].Value);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                return null;
            }
        }

        public int mTag;
        public int pid; // generating process's pid
        public int tid; // generating process's tid
        public int sec; // seconds since Epoch
        public int nsec; // nanoseconds

        private object mData;

        /// <summary>
        /// Creates an <seealso cref="EventContainer"/> from a <seealso cref="LogReceiver.LogEntry"/>. </summary>
        /// <param name="entry">  the LogEntry from which pid, tid, and time info is copied. </param>
        /// <param name="tag"> the event tag value </param>
        /// <param name="data"> the data of the EventContainer. </param>
        internal EventContainer(LogReceiver.LogEntry entry, int tag, object data)
        {
            getType(data);
            mTag = tag;
            mData = data;

            pid = entry.pid;
            tid = entry.tid;
            sec = entry.sec;
            nsec = entry.nsec;
        }

        /// <summary>
        /// Creates an <seealso cref="EventContainer"/> with raw data
        /// </summary>
        internal EventContainer(int tag, int pid, int tid, int sec, int nsec, object data)
        {
            getType(data);
            mTag = tag;
            mData = data;

            this.pid = pid;
            this.tid = tid;
            this.sec = sec;
            this.nsec = nsec;
        }

        /// <summary>
        /// Returns the data as an int. </summary>
        /// <exception cref="InvalidTypeException"> if the data type is not <seealso cref="EventValueType#INT"/>. </exception>
        /// <seealso cref= #getType() </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public final Integer getInt() throws InvalidTypeException
        public int @int
        {
            get
            {
                if (getType(mData) == EventValueTypes.INT)
                {
                    return (int) mData;
                }

                throw new InvalidTypeException();
            }
        }

        /// <summary>
        /// Returns the data as a long. </summary>
        /// <exception cref="InvalidTypeException"> if the data type is not <seealso cref="EventValueType#LONG"/>. </exception>
        /// <seealso cref= #getType() </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public final Long getLong() throws InvalidTypeException
        public long @long
        {
            get
            {
                if (getType(mData) == EventValueTypes.LONG)
                {
                    return (long) mData;
                }

                throw new InvalidTypeException();
            }
        }

        /// <summary>
        /// Returns the data as a String. </summary>
        /// <exception cref="InvalidTypeException"> if the data type is not <seealso cref="EventValueType#STRING"/>. </exception>
        /// <seealso cref= #getType() </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public final String getString() throws InvalidTypeException
        public string @string
        {
            get
            {
                if (getType(mData) == EventValueTypes.STRING)
                {
                    return (string) mData;
                }

                throw new InvalidTypeException();
            }
        }

        /// <summary>
        /// Returns a value by index. The return type is defined by its type. </summary>
        /// <param name="valueIndex"> the index of the value. If the data is not a list, this is ignored. </param>
        public virtual object getValue(int valueIndex)
        {
            return getValue(mData, valueIndex, true);
        }

        /// <summary>
        /// Returns a value by index as a double. </summary>
        /// <param name="valueIndex"> the index of the value. If the data is not a list, this is ignored. </param>
        /// <exception cref="InvalidTypeException"> if the data type is not <seealso cref="EventValueType#INT"/>,
        /// <seealso cref="EventValueType#LONG"/>, <seealso cref="EventValueType#LIST"/>, or if the item in the
        /// list at index <code>valueIndex</code> is not of type <seealso cref="EventValueType#INT"/> or
        /// <seealso cref="EventValueType#LONG"/>. </exception>
        /// <seealso cref= #getType() </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public double getValueAsDouble(int valueIndex) throws InvalidTypeException
        public virtual double getValueAsDouble(int valueIndex)
        {
            return getValueAsDouble(mData, valueIndex, true);
        }

        /// <summary>
        /// Returns a value by index as a String. </summary>
        /// <param name="valueIndex"> the index of the value. If the data is not a list, this is ignored. </param>
        /// <exception cref="InvalidTypeException"> if the data type is not <seealso cref="EventValueType#INT"/>,
        /// <seealso cref="EventValueType#LONG"/>, <seealso cref="EventValueType#STRING"/>, <seealso cref="EventValueType#LIST"/>,
        /// or if the item in the list at index <code>valueIndex</code> is not of type
        /// <seealso cref="EventValueType#INT"/>, <seealso cref="EventValueType#LONG"/>, or <seealso cref="EventValueType#STRING"/> </exception>
        /// <seealso cref= #getType() </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public String getValueAsString(int valueIndex) throws InvalidTypeException
        public virtual string getValueAsString(int valueIndex)
        {
            return getValueAsString(mData, valueIndex, true);
        }

        /// <summary>
        /// Returns the type of the data.
        /// </summary>
        public virtual EventValueTypes type
        {
            get { return getType(mData); }
        }

        /// <summary>
        /// Returns the type of an object.
        /// </summary>
        public EventValueTypes getType(object data)
        {
            if (data is int)
            {
                return EventValueTypes.INT;
            }
            if (data is long)
            {
                return EventValueTypes.LONG;
            }
            if (data is string)
            {
                return EventValueTypes.STRING;
            }
            if (data is object[])
            {
                // loop through the list to see if we have another list
                object[] objects = (object[]) data;
                foreach (object obj in objects)
                {
                    var type = getType(obj);
                    if (type == EventValueTypes.LIST || type == EventValueTypes.TREE)
                    {
                        return EventValueTypes.TREE;
                    }
                }
                return EventValueTypes.LIST;
            }

            return EventValueTypes.UNKNOWN;
        }

        /// <summary>
        /// Checks that the <code>index</code>-th value of this event against a provided value. </summary>
        /// <param name="index"> the index of the value to test </param>
        /// <param name="value"> the value to test against </param>
        /// <param name="compareMethod"> the method of testing </param>
        /// <returns> true if the test passed. </returns>
        /// <exception cref="InvalidTypeException"> in case of type mismatch between the value to test and the value
        /// to test against, or if the compare method is incompatible with the type of the values. </exception>
        /// <seealso cref= CompareMethod </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public boolean testValue(int index, Object value, CompareMethod compareMethod) throws InvalidTypeException
        public virtual bool testValue(int index, object value, CompareMethod compareMethod)
        {
            var type = getType(mData);
            if (index > 0 && type != EventValueTypes.LIST)
            {
                throw new InvalidTypeException();
            }

            object data = mData;
            if (type == EventValueTypes.LIST)
            {
                data = ((object[]) mData)[index];
            }

            if (data.GetType().Equals(data.GetType()) == false)
            {
                throw new InvalidTypeException();
            }

            switch (compareMethod.Method)
            {
                case EventContainer.CompareMethods.EQUAL_TO:
                    return data.Equals(value);
                case EventContainer.CompareMethods.LESSER_THAN:
                    if (data is int)
                    {
                        return (((int) data).CompareTo((int) value) <= 0);
                    }
                    else if (data is long?)
                    {
                        return (((long) data).CompareTo((long) value) <= 0);
                    }

                    // other types can't use this compare method.
                    throw new InvalidTypeException();
                case EventContainer.CompareMethods.LESSER_THAN_STRICT:
                    if (data is int)
                    {
                        return (((int) data).CompareTo((int) value) < 0);
                    }
                    else if (data is long)
                    {
                        return (((long) data).CompareTo((long) value) < 0);
                    }

                    // other types can't use this compare method.
                    throw new InvalidTypeException();
                case EventContainer.CompareMethods.GREATER_THAN:
                    if (data is int)
                    {
                        return (((int) data).CompareTo((int) value) >= 0);
                    }
                    else if (data is long?)
                    {
                        return (((long) data).CompareTo((long) value) >= 0);
                    }

                    // other types can't use this compare method.
                    throw new InvalidTypeException();
                case EventContainer.CompareMethods.GREATER_THAN_STRICT:
                    if (data is int)
                    {
                        return (((int) data).CompareTo((int) value) > 0);
                    }
                    else if (data is long?)
                    {
                        return (((long) data).CompareTo((long) value) > 0);
                    }

                    // other types can't use this compare method.
                    throw new InvalidTypeException();
                case EventContainer.CompareMethods.BIT_CHECK:
                    if (data is int)
                    {
                        return ((int) ((int) data) & (int) ((int) value)) != 0;
                    }
                    else if (data is long?)
                    {
                        return ((long) ((long) data) & (long) ((long) value)) != 0;
                    }

                    // other types can't use this compare method.
                    throw new InvalidTypeException();
                default:
                    throw new InvalidTypeException();
            }
        }

        private object getValue(object data, int valueIndex, bool recursive)
        {
            var type = getType(data);

            switch (type)
            {
                case EventContainer.EventValueTypes.INT:
                case EventContainer.EventValueTypes.LONG:
                case EventContainer.EventValueTypes.STRING:
                    return data;
                case EventContainer.EventValueTypes.LIST:
                    if (recursive)
                    {
                        object[] list = (object[]) data;
                        if (valueIndex >= 0 && valueIndex < list.Length)
                        {
                            return getValue(list[valueIndex], valueIndex, false);
                        }
                    }
                    break;
            }

            return null;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private final double getValueAsDouble(Object data, int valueIndex, boolean recursive) throws InvalidTypeException
        private double getValueAsDouble(object data, int valueIndex, bool recursive)
        {
            var type = getType(data);

            switch (type)
            {
                case EventContainer.EventValueTypes.INT:
                    return (int) data;
                case EventContainer.EventValueTypes.LONG:
                    return (long) data;
                case EventContainer.EventValueTypes.STRING:
                    throw new InvalidTypeException();
                case EventContainer.EventValueTypes.LIST:
                    if (recursive)
                    {
                        var list = (object[]) data;
                        if (valueIndex >= 0 && valueIndex < list.Length)
                        {
                            return getValueAsDouble(list[valueIndex], valueIndex, false);
                        }
                    }
                    break;
            }

            throw new InvalidTypeException();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private final String getValueAsString(Object data, int valueIndex, boolean recursive) throws InvalidTypeException
        private string getValueAsString(object data, int valueIndex, bool recursive)
        {
            var type = getType(data);

            switch (type)
            {
                case EventValueTypes.INT:
                    return ((int) data).ToString();
                case EventValueTypes.LONG:
                    return ((long) data).ToString();
                case EventValueTypes.STRING:
                    return (string) data;
                case EventValueTypes.LIST:
                    if (recursive)
                    {
                        object[] list = (object[]) data;
                        if (valueIndex >= 0 && valueIndex < list.Length)
                        {
                            return getValueAsString(list[valueIndex], valueIndex, false);
                        }
                    }
                    else
                    {
                        throw new InvalidTypeException("getValueAsString() doesn't support EventValueType.TREE");
                    }
                    break;
            }

            throw new InvalidTypeException("getValueAsString() unsupported type:" + type);
        }

    }

    public static partial class EnumExtensionMethods
    {
        public static int getValue(this EventContainer.EventValueTypes valueType)
        {
            return (int) valueType;
        }

        public static string ToString(this EventContainer.EventValueTypes instance)
        {
            return instance.ToString().ToLower(CultureInfo.GetCultureInfo("en-US"));
        }
    }
}