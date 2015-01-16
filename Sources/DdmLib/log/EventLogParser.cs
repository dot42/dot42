using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dot42.DdmLib.support;
using Dot42.DdmLib.utils;

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
    /// Parser for the "event" log.
    /// </summary>
    public sealed class EventLogParser
    {

        /// <summary>
        /// Location of the tag map file on the device </summary>
        private const string EVENT_TAG_MAP_FILE = "/system/etc/event-log-tags"; //$NON-NLS-1$

        /// <summary>
        /// Event log entry types.  These must match up with the declarations in
        /// java/android/android/util/EventLog.java.
        /// </summary>
        private const int EVENT_TYPE_INT = 0;

        private const int EVENT_TYPE_LONG = 1;
        private const int EVENT_TYPE_STRING = 2;
        private const int EVENT_TYPE_LIST = 3;

        private static readonly Regex PATTERN_SIMPLE_TAG = new Regex("^(\\d+)\\s+([A-Za-z0-9_]+)\\s*$"); //$NON-NLS-1$

        private static readonly Regex PATTERN_TAG_WITH_DESC = new Regex("^(\\d+)\\s+([A-Za-z0-9_]+)\\s*(.*)\\s*$");
                                      //$NON-NLS-1$

        private static readonly Regex PATTERN_DESCRIPTION = new Regex("\\(([A-Za-z0-9_\\s]+)\\|(\\d+)(\\|\\d+){0,1}\\)");
                                      //$NON-NLS-1$

        private static readonly Regex TEXT_LOG_LINE =
            new Regex(
                "(\\d\\d)-(\\d\\d)\\s(\\d\\d):(\\d\\d):(\\d\\d).(\\d{3})\\s+I/([a-zA-Z0-9_]+)\\s*\\(\\s*(\\d+)\\):\\s+(.*)");
                                      //$NON-NLS-1$

        private readonly SortedDictionary<int?, string> mTagMap = new SortedDictionary<int?, string>();

        private readonly SortedDictionary<int?, EventValueDescription[]> mValueDescriptionMap =
            new SortedDictionary<int?, EventValueDescription[]>();

        public EventLogParser()
        {
        }

        /// <summary>
        /// Inits the parser for a specific Device.
        /// <p/>
        /// This methods reads the event-log-tags located on the device to find out
        /// what tags are being written to the event log and what their format is. </summary>
        /// <param name="device"> The device. </param>
        /// <returns> <code>true</code> if success, <code>false</code> if failure or cancellation. </returns>
        public bool init(IDevice device)
        {
            // read the event tag map file on the device.
            try
            {
                var tempVar = new MultiLineReceiverAnonymousInnerClassHelper();
                tempVar.processNewLinesDelegateInstance =
                    (string[] lines) => {
                        foreach (string line in lines)
                        {
                            processTagLine(line);
                        }
                    };
                /*tempVar.isCancelledDelegateInstance =
                    () => {
                        return false;
                    };*/
                device.executeShellCommand("cat " + EVENT_TAG_MAP_FILE, tempVar);
            }
            catch (Exception)
            {
                // catch all possible exceptions and return false.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Inits the parser with the content of a tag file. </summary>
        /// <param name="tagFileContent"> the lines of a tag file. </param>
        /// <returns> <code>true</code> if success, <code>false</code> if failure. </returns>
        public bool init(string[] tagFileContent)
        {
            foreach (string line in tagFileContent)
            {
                processTagLine(line);
            }
            return true;
        }

        /// <summary>
        /// Inits the parser with a specified event-log-tags file. </summary>
        /// <param name="filePath"> </param>
        /// <returns> <code>true</code> if success, <code>false</code> if failure. </returns>
        public bool init(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        processTagLine(line);
                    }
                }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Processes a line from the event-log-tags file. </summary>
        /// <param name="line"> the line to process </param>
        private void processTagLine(string line)
        {
            // ignore empty lines and comment lines
            if (line.Length > 0 && line[0] != '#')
            {
                var m = PATTERN_TAG_WITH_DESC.Match(line);
                if (m.Success)
                {
                    try
                    {
                        int value = Convert.ToInt32(m.@group(1));
                        string name = m.group(2);
                        if (name != null && mTagMap[value] == null)
                        {
                            mTagMap.Add(value, name);
                        }

                        // special case for the GC tag. We ignore what is in the file,
                        // and take what the custom GcEventContainer class tells us.
                        // This is due to the event encoding several values on 2 longs.
                        // @see GcEventContainer
                        if (value == GcEventContainer.GC_EVENT_TAG)
                        {
                            mValueDescriptionMap.Add(value, GcEventContainer.valueDescriptions);
                        }
                        else
                        {

                            string description = m.group(3);
                            if (description != null && description.Length > 0)
                            {
                                EventValueDescription[] desc = processDescription(description);

                                if (desc != null)
                                {
                                    mValueDescriptionMap.Add(value, desc);
                                }
                            }
                        }
                    }
                    catch (SystemException)
                    {
                        // failed to convert the number into a string. just ignore it.
                    }
                }
                else
                {
                    m = PATTERN_SIMPLE_TAG.Match(line);
                    if (m.matches())
                    {
                        int value = Convert.ToInt32(m.group(1));
                        string name = m.group(2);
                        if (name != null && mTagMap[value] == null)
                        {
                            mTagMap.Add(value, name);
                        }
                    }
                }
            }
        }

        private EventValueDescription[] processDescription(string description)
        {
            string[] descriptions = StringHelperClass.StringSplit(description, "\\s*,\\s*", true); //$NON-NLS-1$

            List<EventValueDescription> list = new List<EventValueDescription>();

            foreach (string desc in descriptions)
            {
                var m = PATTERN_DESCRIPTION.Match(desc);
                if (m.Success)
                {
                    try
                    {
                        string name = m.Groups[0].Value;

                        string typeString = m.Groups[1].Value;
                        int typeValue = Convert.ToInt32(typeString);
                        EventContainer.EventValueTypes? eventValueType = EventContainer.EventValueType.getEventValueType(typeValue);
                        if (eventValueType == null)
                        {
                            // just ignore this description if the value is not recognized.
                            // TODO: log the error.
                        }

                        typeString = m.Groups[2].Value;
                        if (typeString != null && typeString.Length > 0)
                        {
                            //skip the |
                            typeString = typeString.Substring(1);

                            typeValue = Convert.ToInt32(typeString);
                            var valueType = (EventValueDescription.ValueTypes) typeValue;

                            list.Add(new EventValueDescription(name, eventValueType.Value, valueType));
                        }
                        else
                        {
                            list.Add(new EventValueDescription(name, eventValueType.Value));
                        }
                    }
                    catch (SystemException)
                    {
                        // just ignore this description if one number is malformed.
                        // TODO: log the error.
                    }
                    catch (InvalidValueTypeException)
                    {
                        // just ignore this description if data type and data unit don't match
                        // TODO: log the error.
                    }
                }
                else
                {
                    Log.e("EventLogParser", string.Format("Can't parse {0}", description)); //$NON-NLS-1$ - $NON-NLS-1$
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            return list.ToArray();

        }

        public EventContainer parse(LogReceiver.LogEntry entry)
        {
            if (entry.len < 4)
            {
                return null;
            }

            int inOffset = 0;

            int tagValue = ArrayHelper.swap32bitFromArray(entry.data, inOffset);
            inOffset += 4;

            string tag = mTagMap[tagValue];
            if (tag == null)
            {
                Log.e("EventLogParser", string.Format("unknown tag number: {0:D}", tagValue));
            }

            List<object> list = new List<object>();
            if (parseBinaryEvent(entry.data, inOffset, list) == -1)
            {
                return null;
            }

            object data;
            if (list.Count == 1)
            {
                data = list[0];
            }
            else
            {
                data = list.ToArray();
            }

            EventContainer @event = null;
            if (tagValue == GcEventContainer.GC_EVENT_TAG)
            {
                @event = new GcEventContainer(entry, tagValue, data);
            }
            else
            {
                @event = new EventContainer(entry, tagValue, data);
            }

            return @event;
        }

        public EventContainer parse(string textLogLine)
        {
            // line will look like
            // 04-29 23:16:16.691 I/dvm_gc_info(  427): <data>
            // where <data> is either
            // [value1,value2...]
            // or
            // value
            if (textLogLine.Length == 0)
            {
                return null;
            }

            // parse the header first
            var m = TEXT_LOG_LINE.Match(textLogLine);
            if (m.Success)
            {
                try
                {
                    int month = Convert.ToInt32(m.Groups[0].Value);
                    int day = Convert.ToInt32(m.Groups[1].Value);
                    int hours = Convert.ToInt32(m.Groups[2].Value);
                    int minutes = Convert.ToInt32(m.Groups[3].Value);
                    int seconds = Convert.ToInt32(m.Groups[4].Value);
                    int milliseconds = Convert.ToInt32(m.Groups[5].Value);

                    // convert into seconds since epoch and nano-seconds.
                    /*Calendar cal = new GregorianCalendar();
                    cal.set(cal.get(Calendar.YEAR), month - 1, day, hours, minutes, seconds);
                    int sec = (int) Math.Floor(cal.timeInMillis/1000);
                    int nsec = milliseconds*1000000;*/

                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0);
                    var date = new DateTime(DateTime.Now.Year, month, day, hours, minutes, 0);
                    var sec = (int)Math.Floor(date.Subtract(epoch).TotalSeconds + seconds);
                    var nsec = milliseconds*1000000;

                    string tag = m.group(7);

                    // get the numerical tag value
                    int tagValue = -1;
                    //JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
                    var tagSet = mTagMap;
                    foreach (var entry in tagSet)
                    {
                        if (tag.Equals(entry.Value))
                        {
                            tagValue = entry.Key.Value;
                            break;
                        }
                    }

                    if (tagValue == -1)
                    {
                        return null;
                    }

                    int pid = int.Parse(m.group(8));

                    object data = parseTextData(m.group(9), tagValue);
                    if (data == null)
                    {
                        return null;
                    }

                    // now we can allocate and return the EventContainer
                    EventContainer @event = null;
                    if (tagValue == GcEventContainer.GC_EVENT_TAG)
                    {
                        @event = new GcEventContainer(tagValue, pid, -1, sec, nsec, data); // tid
                    }
                    else
                    {
                        @event = new EventContainer(tagValue, pid, -1, sec, nsec, data); // tid
                    }

                    return @event;
                }
                catch (SystemException)
                {
                    return null;
                }
            }

            return null;
        }

        public IDictionary<int?, string> tagMap
        {
            get { return mTagMap; }
        }

        public IDictionary<int?, EventValueDescription[]> eventInfoMap
        {
            get { return mValueDescriptionMap; }
        }

        /// <summary>
        /// Recursively convert binary log data to printable form.
        /// 
        /// This needs to be recursive because you can have lists of lists.
        /// 
        /// If we run out of room, we stop processing immediately.  It's important
        /// for us to check for space on every output element to avoid producing
        /// garbled output.
        /// 
        /// Returns the amount read on success, -1 on failure.
        /// </summary>
        private static int parseBinaryEvent(byte[] eventData, int dataOffset, List<object> list)
        {

            if (eventData.Length - dataOffset < 1)
            {
                return -1;
            }

            int offset = dataOffset;

            int type = eventData[offset++];

            //fprintf(stderr, "--- type=%d (rem len=%d)\n", type, eventDataLen);

            switch (type)
            {
                case EVENT_TYPE_INT: // 32-bit signed int
                    {
                        int ival;

                        if (eventData.Length - offset < 4)
                        {
                            return -1;
                        }
                        ival = ArrayHelper.swap32bitFromArray(eventData, offset);
                        offset += 4;

                        list.Add(new int?(ival));
                    }
                    break;
                case EVENT_TYPE_LONG: // 64-bit signed long
                    {
                        long lval;

                        if (eventData.Length - offset < 8)
                        {
                            return -1;
                        }
                        lval = ArrayHelper.swap64bitFromArray(eventData, offset);
                        offset += 8;

                        list.Add(new long?(lval));
                    }
                    break;
                case EVENT_TYPE_STRING: // UTF-8 chars, not NULL-terminated
                    {
                        int strLen;

                        if (eventData.Length - offset < 4)
                        {
                            return -1;
                        }
                        strLen = ArrayHelper.swap32bitFromArray(eventData, offset);
                        offset += 4;

                        if (eventData.Length - offset < strLen)
                        {
                            return -1;
                        }

                        // get the string
                        try
                        {
                            string str = eventData.getString(offset, strLen, "UTF-8"); //$NON-NLS-1$
                            list.Add(str);
                        }
                        catch (ArgumentException)
                        {
                        }
                        offset += strLen;
                        break;
                    }
                case EVENT_TYPE_LIST: // N items, all different types
                    {

                        if (eventData.Length - offset < 1)
                        {
                            return -1;
                        }

                        int count = eventData[offset++];

                        // make a new temp list
                        List<object> subList = new List<object>();
                        for (int i = 0; i < count; i++)
                        {
                            int result = parseBinaryEvent(eventData, offset, subList);
                            if (result == -1)
                            {
                                return result;
                            }

                            offset += result;
                        }

                        list.Add(subList.ToArray());
                    }
                    break;
                default:
                    Log.e("EventLogParser", string.Format("Unknown binary event type {0:D}", type));
                        //$NON-NLS-1$ - $NON-NLS-1$
                    return -1;
            }

            return offset - dataOffset;
        }

        private object parseTextData(string data, int tagValue)
        {
            // first, get the description of what we're supposed to parse
            EventValueDescription[] desc = mValueDescriptionMap[tagValue];

            if (desc == null)
            {
                // TODO parse and create string values.
                return null;
            }

            if (desc.Length == 1)
            {
                return getObjectFromString(data, desc[0].eventValueType);
            }
            else if (data.StartsWith("[") && data.EndsWith("]"))
            {
                data = data.Substring(1, data.Length - 1 - 1);

                // get each individual values as String
                string[] values = StringHelperClass.StringSplit(data, ",", true);

                if (tagValue == GcEventContainer.GC_EVENT_TAG)
                {
                    // special case for the GC event!
                    object[] objects = new object[2];

                    objects[0] = getObjectFromString(values[0], EventContainer.EventValueTypes.LONG);
                    objects[1] = getObjectFromString(values[1], EventContainer.EventValueTypes.LONG);

                    return objects;
                }
                else
                {
                    // must be the same number as the number of descriptors.
                    if (values.Length != desc.Length)
                    {
                        return null;
                    }

                    object[] objects = new object[values.Length];

                    for (int i = 0; i < desc.Length; i++)
                    {
                        object obj = getObjectFromString(values[i], desc[i].eventValueType);
                        if (obj == null)
                        {
                            return null;
                        }
                        objects[i] = obj;
                    }

                    return objects;
                }
            }

            return null;
        }


        private object getObjectFromString(string value, EventContainer.EventValueTypes type)
        {
            try
            {
                switch (type)
                {
                    case EventContainer.EventValueTypes.INT:
                        return int.Parse(value);
                    case EventContainer.EventValueTypes.LONG:
                        return long.Parse(value);
                    case EventContainer.EventValueTypes.STRING:
                        return value;
                }
            }
            catch (Exception)
            {
                // do nothing, we'll return null.
            }

            return null;
        }

        /// <summary>
        /// Recreates the event-log-tags at the specified file path. </summary>
        /// <param name="filePath"> the file path to write the file. </param>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void saveTags(String filePath) throws java.io.IOException
        public void saveTags(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (int? key in mTagMap.Keys)
                {
                    // get the tag name
                    string tagName = mTagMap[key];

                    // get the value descriptions
                    EventValueDescription[] descriptors = mValueDescriptionMap[key];

                    string line = null;
                    if (descriptors != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(string.Format("{0:D} {1}", key, tagName)); //$NON-NLS-1$
                        bool first = true;
                        foreach (EventValueDescription evd in descriptors)
                        {
                            if (first)
                            {
                                sb.Append(" ("); //$NON-NLS-1$
                                first = false;
                            }
                            else
                            {
                                sb.Append(",("); //$NON-NLS-1$
                            }
                            sb.Append(evd.name);
                            sb.Append("|"); //$NON-NLS-1$
                            sb.Append((int) evd.eventValueType);
                            sb.Append("|"); //$NON-NLS-1$
                            sb.Append((int) evd.valueType);
                            sb.Append("|)"); //$NON-NLS-1$
                        }
                        sb.AppendLine(); //$NON-NLS-1$

                        line = sb.ToString();
                    }
                    else
                    {
                        line = string.Format("{0:D} {1}\n", key, tagName); //$NON-NLS-1$
                    }

                    writer.Write(line);
                }
            }
        }
    }
}