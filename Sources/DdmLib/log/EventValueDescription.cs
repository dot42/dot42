using System;
using System.Globalization;

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

namespace Dot42.DdmLib.log
{
    /// <summary>
	/// Describes an <seealso cref="EventContainer"/> value.
	/// <p/>
	/// This is a stand-alone object, not linked to a particular Event. It describes the value, by
	/// name, type (<seealso cref="EventContainer.EventValueType"/>), and (if needed) value unit (<seealso cref="ValueType"/>).
	/// <p/>
	/// The index of the value is not contained within this class, and is instead dependent on the
	/// index of this particular object in the array of <seealso cref="EventValueDescription"/> returned by
	/// <seealso cref="EventLogParser#getEventInfoMap()"/> when queried for a particular event tag.
	/// 
	/// </summary>
	public sealed class EventValueDescription
	{

		/// <summary>
		/// Represents the type of a numerical value. This is used to display values of vastly different
		/// type/range in graphs.
		/// </summary>
		public enum ValueTypes
		{
			NOT_APPLICABLE = 0,
			OBJECTS = 1,
			BYTES = 2,
			MILLISECONDS = 3,
			ALLOCATIONS = 4,
			ID = 5,
			PERCENT = 6
		}

		private string mName;
		private EventContainer.EventValueTypes mEventValueType;
		private ValueTypes mValueType;

		/// <summary>
		/// Builds a <seealso cref="EventValueDescription"/> with a name and a type.
		/// <p/>
		/// If the type is <seealso cref="EventValueType#INT"/> or <seealso cref="EventValueType#LONG"/>, the
		/// <seealso cref="#mValueType"/> is set to <seealso cref="ValueType#BYTES"/> by default. It set to
		/// <seealso cref="ValueType#NOT_APPLICABLE"/> for all other <seealso cref="EventValueType"/> values. </summary>
		/// <param name="name"> </param>
		/// <param name="type"> </param>
		internal EventValueDescription(string name, EventContainer.EventValueTypes type)
		{
			mName = name;
			mEventValueType = type;
			if (mEventValueType == EventContainer.EventValueTypes.INT || mEventValueType == EventContainer.EventValueTypes.LONG)
			{
				mValueType = ValueTypes.BYTES;
			}
			else
			{
				mValueType = ValueTypes.NOT_APPLICABLE;
			}
		}

		/// <summary>
		/// Builds a <seealso cref="EventValueDescription"/> with a name and a type, and a <seealso cref="ValueType"/>.
		/// <p/> </summary>
		/// <param name="name"> </param>
		/// <param name="type"> </param>
		/// <param name="valueType"> </param>
		/// <exception cref="InvalidValueTypeException"> if type and valuetype are not compatible.
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EventValueDescription(String name, com.android.ddmlib.log.EventContainer.EventValueType type, ValueType valueType) throws InvalidValueTypeException
		internal EventValueDescription(string name, EventContainer.EventValueTypes type, ValueTypes valueType)
		{
			mName = name;
			mEventValueType = type;
			mValueType = valueType;
			mValueType.checkType(mEventValueType);
		}

		/// <returns> the Name. </returns>
		public string name
		{
			get
			{
				return mName;
			}
		}

		/// <returns> the <seealso cref="EventContainer.EventValueType"/>. </returns>
		public EventContainer.EventValueTypes eventValueType
		{
			get
			{
				return mEventValueType;
			}
		}

		/// <returns> the <seealso cref="ValueType"/>. </returns>
		public ValueType valueType
		{
			get
			{
				return mValueType;
			}
		}

		public override string ToString()
		{
			if (mValueType != ValueTypes.NOT_APPLICABLE)
			{
				return string.Format("{0} ({1}, {2})", mName, mEventValueType.ToString(), mValueType.ToString());
			}

			return string.Format("{0} ({1})", mName, mEventValueType.ToString());
		}

		/// <summary>
		/// Checks if the value is of the proper type for this receiver. </summary>
		/// <param name="value"> the value to check. </param>
		/// <returns> true if the value is of the proper type for this receiver. </returns>
		public bool checkForType(object value)
		{
			switch (mEventValueType)
			{
				case EventContainer.EventValueTypes.INT:
					return value is int;
				case EventContainer.EventValueTypes.LONG:
					return value is long;
				case EventContainer.EventValueTypes.STRING:
					return value is string;
				case EventContainer.EventValueTypes.LIST:
					return value is object[];
			}

			return false;
		}

		/// <summary>
		/// Returns an object of a valid type (based on the value returned by
		/// <seealso cref="#getEventValueType()"/>) from a String value.
		/// <p/>
		/// IMPORTANT <seealso cref="EventValueType#LIST"/> and <seealso cref="EventValueType#TREE"/> are not
		/// supported. </summary>
		/// <param name="value"> the value of the object expressed as a string. </param>
		/// <returns> an object or null if the conversion could not be done. </returns>
		public object getObjectFromString(string value)
		{
			switch (mEventValueType)
			{
				case EventContainer.EventValueTypes.INT:
					try
					{
						return int.Parse(value);
					}
					catch (Exception)
					{
						return null;
					}
				case EventContainer.EventValueTypes.LONG:
					try
					{
						return long.Parse(value);
					}
					catch (Exception)
					{
						return null;
					}
				case EventContainer.EventValueTypes.STRING:
					return value;
			}

			return null;
		}
	}
    public static partial class EnumExtensionMethods
    {
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void checkType(com.android.ddmlib.log.EventContainer.EventValueType type) throws InvalidValueTypeException
        public static void checkType(this EventValueDescription.ValueTypes instance, EventContainer.EventValueTypes type)
        {
            if ((type != EventContainer.EventValueTypes.INT && type != EventContainer.EventValueTypes.LONG) && instance != EventValueDescription.ValueTypes.NOT_APPLICABLE)
            {
                throw new InvalidValueTypeException(string.Format("{0} doesn't support type {1}", type, instance));
            }
        }
        /*public static int getValue(this ValueType instanceJavaToDotNetTempPropertyGetvalue)
        {
            return mValue;
        }*/
        public static string ToString(this ValueType instance)
        {
            return instance.ToString().ToLower(CultureInfo.GetCultureInfo("en-US"));
        }
    }

}