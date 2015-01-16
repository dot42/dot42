using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

/*
 * Copyright (C) 2007 The Android Open Source Project
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

namespace Dot42.DdmLib
{


    /// <summary>
    /// Provides <seealso cref="Device"/> side file listing service.
    /// <p/>To get an instance for a known <seealso cref="Device"/>, call <seealso cref="Device#getFileListingService()"/>.
    /// </summary>
    public sealed class FileListingService
    {

        /// <summary>
        /// Pattern to find filenames that match "*.apk" </summary>
        private static readonly Regex sApkPattern = new Regex(".*\\.apk", RegexOptions.IgnoreCase); //$NON-NLS-1$

        private const string PM_FULL_LISTING = "pm list packages -f"; //$NON-NLS-1$

        /// <summary>
        /// Pattern to parse the output of the 'pm -lf' command.<br>
        /// The output format looks like:<br>
        /// </summary>
        private static readonly Regex sPmPattern = new Regex("^package:(.+?)=(.+)$"); //$NON-NLS-1$

        /// <summary>
        /// Top level data folder. </summary>
        public const string DIRECTORY_DATA = "data"; //$NON-NLS-1$
        /// <summary>
        /// Top level sdcard folder. </summary>
        public const string DIRECTORY_SDCARD = "sdcard"; //$NON-NLS-1$
        /// <summary>
        /// Top level mount folder. </summary>
        public const string DIRECTORY_MNT = "mnt"; //$NON-NLS-1$
        /// <summary>
        /// Top level system folder. </summary>
        public const string DIRECTORY_SYSTEM = "system"; //$NON-NLS-1$
        /// <summary>
        /// Top level temp folder. </summary>
        public const string DIRECTORY_TEMP = "tmp"; //$NON-NLS-1$
        /// <summary>
        /// Application folder. </summary>
        public const string DIRECTORY_APP = "app"; //$NON-NLS-1$

        private static readonly string[] sRootLevelApprovedItems = { DIRECTORY_DATA, DIRECTORY_SDCARD, DIRECTORY_SYSTEM, DIRECTORY_TEMP, DIRECTORY_MNT };

        public const long REFRESH_RATE = 5000L;
        /// <summary>
        /// Refresh test has to be slightly lower for precision issue.
        /// </summary>
        internal static readonly long REFRESH_TEST = (long)(REFRESH_RATE * .8);

        /// <summary>
        /// Entry type: File </summary>
        public const int TYPE_FILE = 0;
        /// <summary>
        /// Entry type: Directory </summary>
        public const int TYPE_DIRECTORY = 1;
        /// <summary>
        /// Entry type: Directory Link </summary>
        public const int TYPE_DIRECTORY_LINK = 2;
        /// <summary>
        /// Entry type: Block </summary>
        public const int TYPE_BLOCK = 3;
        /// <summary>
        /// Entry type: Character </summary>
        public const int TYPE_CHARACTER = 4;
        /// <summary>
        /// Entry type: Link </summary>
        public const int TYPE_LINK = 5;
        /// <summary>
        /// Entry type: Socket </summary>
        public const int TYPE_SOCKET = 6;
        /// <summary>
        /// Entry type: FIFO </summary>
        public const int TYPE_FIFO = 7;
        /// <summary>
        /// Entry type: Other </summary>
        public const int TYPE_OTHER = 8;

        /// <summary>
        /// Device side file separator. </summary>
        public const string FILE_SEPARATOR = "/"; //$NON-NLS-1$

        private const string FILE_ROOT = "/"; //$NON-NLS-1$


        /// <summary>
        /// Regexp pattern to parse the result from ls.
        /// </summary>
        private static Regex sLsPattern = new Regex("^([bcdlsp-][-r][-w][-xsS][-r][-w][-xsS][-r][-w][-xstST])\\s+(\\S+)\\s+(\\S+)\\s+([\\d\\s,]*)\\s+(\\d{4}-\\d\\d-\\d\\d)\\s+(\\d\\d:\\d\\d)\\s+(.*)$"); //$NON-NLS-1$

        private Device mDevice;
        private FileEntry mRoot;

        private List<Thread> mThreadList = new List<Thread>();

        /// <summary>
        /// Represents an entry in a directory. This can be a file or a directory.
        /// </summary>
        public sealed class FileEntry : IComparable<FileEntry>
        {
            /// <summary>
            /// Pattern to escape filenames for shell command consumption. </summary>
            private static readonly Regex sEscapePattern = new Regex("([\\\\()*+?\"'#/\\s])"); //$NON-NLS-1$

            internal FileEntry parent_Renamed;
            internal string name_Renamed;
            internal string info_Renamed;
            internal string permissions_Renamed;
            internal string size_Renamed;
            internal string date_Renamed;
            internal string time_Renamed;
            internal string owner;
            internal string group;
            internal int type_Renamed;
            internal bool isAppPackage;

            internal bool isRoot;

            /// <summary>
            /// Indicates whether the entry content has been fetched yet, or not.
            /// </summary>
            internal long fetchTime = 0;

            internal readonly List<FileEntry> mChildren = new List<FileEntry>();

            /// <summary>
            /// Creates a new file entry. </summary>
            /// <param name="parent"> parent entry or null if entry is root </param>
            /// <param name="name"> name of the entry. </param>
            /// <param name="type"> entry type. Can be one of the following: <seealso cref="FileListingService#TYPE_FILE"/>,
            /// <seealso cref="FileListingService#TYPE_DIRECTORY"/>, <seealso cref="FileListingService#TYPE_OTHER"/>. </param>
            internal FileEntry(FileEntry parent, string name, int type, bool isRoot)
            {
                this.parent_Renamed = parent;
                this.name_Renamed = name;
                this.type_Renamed = type;
                this.isRoot = isRoot;

                checkAppPackageStatus();
            }

            /// <summary>
            /// Returns the name of the entry
            /// </summary>
            public string name
            {
                get
                {
                    return name_Renamed;
                }
            }

            /// <summary>
            /// Returns the size string of the entry, as returned by <code>ls</code>.
            /// </summary>
            public string size
            {
                get
                {
                    return size_Renamed;
                }
            }

            /// <summary>
            /// Returns the size of the entry.
            /// </summary>
            public int sizeValue
            {
                get
                {
                    return Convert.ToInt32(size_Renamed);
                }
            }

            /// <summary>
            /// Returns the date string of the entry, as returned by <code>ls</code>.
            /// </summary>
            public string date
            {
                get
                {
                    return date_Renamed;
                }
            }

            /// <summary>
            /// Returns the time string of the entry, as returned by <code>ls</code>.
            /// </summary>
            public string time
            {
                get
                {
                    return time_Renamed;
                }
            }

            /// <summary>
            /// Returns the permission string of the entry, as returned by <code>ls</code>.
            /// </summary>
            public string permissions
            {
                get
                {
                    return permissions_Renamed;
                }
            }

            /// <summary>
            /// Returns the extra info for the entry.
            /// <p/>For a link, it will be a description of the link.
            /// <p/>For an application apk file it will be the application package as returned
            /// by the Package Manager.
            /// </summary>
            public string info
            {
                get
                {
                    return info_Renamed;
                }
            }

            /// <summary>
            /// Return the full path of the entry. </summary>
            /// <returns> a path string using <seealso cref="FileListingService#FILE_SEPARATOR"/> as separator. </returns>
            public string fullPath
            {
                get
                {
                    if (isRoot)
                    {
                        return FILE_ROOT;
                    }
                    StringBuilder pathBuilder = new StringBuilder();
                    fillPathBuilder(pathBuilder, false);

                    return pathBuilder.ToString();
                }
            }

            /// <summary>
            /// Return the fully escaped path of the entry. This path is safe to use in a
            /// shell command line. </summary>
            /// <returns> a path string using <seealso cref="FileListingService#FILE_SEPARATOR"/> as separator </returns>
            public string fullEscapedPath
            {
                get
                {
                    StringBuilder pathBuilder = new StringBuilder();
                    fillPathBuilder(pathBuilder, true);

                    return pathBuilder.ToString();
                }
            }

            /// <summary>
            /// Returns the path as a list of segments.
            /// </summary>
            public string[] pathSegments
            {
                get
                {
                    List<string> list = new List<string>();
                    fillPathSegments(list);

                    return list.ToArray();
                }
            }

            /// <summary>
            /// Returns true if the entry is a directory, false otherwise;
            /// </summary>
            public int type
            {
                get
                {
                    return type_Renamed;
                }
            }

            /// <summary>
            /// Returns if the entry is a folder or a link to a folder.
            /// </summary>
            public bool directory
            {
                get
                {
                    return type_Renamed == TYPE_DIRECTORY || type_Renamed == TYPE_DIRECTORY_LINK;
                }
            }

            /// <summary>
            /// Returns the parent entry.
            /// </summary>
            public FileEntry parent
            {
                get
                {
                    return parent_Renamed;
                }
            }

            /// <summary>
            /// Returns the cached children of the entry. This returns the cache created from calling
            /// <code>FileListingService.getChildren()</code>.
            /// </summary>
            public FileEntry[] cachedChildren
            {
                get
                {
                    return mChildren.ToArray();
                }
            }

            /// <summary>
            /// Returns the child <seealso cref="FileEntry"/> matching the name.
            /// This uses the cached children list. </summary>
            /// <param name="name"> the name of the child to return. </param>
            /// <returns> the FileEntry matching the name or null. </returns>
            public FileEntry findChild(string name)
            {
                foreach (FileEntry entry in mChildren)
                {
                    if (entry.name_Renamed.Equals(name))
                    {
                        return entry;
                    }
                }
                return null;
            }

            /// <summary>
            /// Returns whether the entry is the root.
            /// </summary>
            public bool root
            {
                get
                {
                    return isRoot;
                }
            }

            internal void addChild(FileEntry child)
            {
                mChildren.Add(child);
            }

            internal List<FileEntry> children
            {
                set
                {
                    mChildren.Clear();
                    mChildren.AddRange(value);
                }
            }

            internal bool needFetch()
            {
                if (fetchTime == 0)
                {
                    return true;
                }
                long current = Environment.TickCount;
                if (current - fetchTime > REFRESH_TEST)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Returns if the entry is a valid application package.
            /// </summary>
            public bool applicationPackage
            {
                get
                {
                    return isAppPackage;
                }
            }

            /// <summary>
            /// Returns if the file name is an application package name.
            /// </summary>
            public bool appFileName
            {
                get
                {
                    var m = sApkPattern.Match(name_Renamed);
                    return m.Success;
                }
            }

            /// <summary>
            /// Recursively fills the pathBuilder with the full path </summary>
            /// <param name="pathBuilder"> a StringBuilder used to create the path. </param>
            /// <param name="escapePath"> Whether the path need to be escaped for consumption by
            /// a shell command line. </param>
            internal void fillPathBuilder(StringBuilder pathBuilder, bool escapePath)
            {
                if (isRoot)
                {
                    return;
                }

                if (parent_Renamed != null)
                {
                    parent_Renamed.fillPathBuilder(pathBuilder, escapePath);
                }
                pathBuilder.Append(FILE_SEPARATOR);
                pathBuilder.Append(escapePath ? escape(name_Renamed) : name_Renamed);
            }

            /// <summary>
            /// Recursively fills the segment list with the full path. </summary>
            /// <param name="list"> The list of segments to fill. </param>
            internal void fillPathSegments(List<string> list)
            {
                if (isRoot)
                {
                    return;
                }

                if (parent_Renamed != null)
                {
                    parent_Renamed.fillPathSegments(list);
                }

                list.Add(name_Renamed);
            }

            /// <summary>
            /// Sets the internal app package status flag. This checks whether the entry is in an app
            /// directory like /data/app or /system/app
            /// </summary>
            private void checkAppPackageStatus()
            {
                isAppPackage = false;

                string[] segments = pathSegments;
                if (type_Renamed == TYPE_FILE && segments.Length == 3 && appFileName)
                {
                    isAppPackage = DIRECTORY_APP.Equals(segments[1]) && (DIRECTORY_SYSTEM.Equals(segments[0]) || DIRECTORY_DATA.Equals(segments[0]));
                }
            }

            /// <summary>
            /// Returns an escaped version of the entry name. </summary>
            /// <param name="entryName"> </param>
            public static string escape(string entryName)
            {
                return sEscapePattern.Replace(entryName, "\\\\$1");
                //return sEscapePattern.matcher(entryName).replaceAll("\\\\$1"); //$NON-NLS-1$
            }

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public int CompareTo(FileEntry other)
            {
                return name.CompareTo(other.name);
            }
        }

        private class LsReceiver : MultiLineReceiver
        {

            private List<FileEntry> mEntryList;
            private List<string> mLinkList;
            private FileEntry[] mCurrentChildren;
            private FileEntry mParentEntry;

            /// <summary>
            /// Create an ls receiver/parser. </summary>
            /// <param name="currentChildren"> The list of current children. To prevent
            ///      collapse during update, reusing the same FileEntry objects for
            ///      files that were already there is paramount. </param>
            /// <param name="entryList"> the list of new children to be filled by the
            ///      receiver. </param>
            /// <param name="linkList"> the list of link path to compute post ls, to figure
            ///      out if the link pointed to a file or to a directory. </param>
            public LsReceiver(FileEntry parentEntry, List<FileEntry> entryList, List<string> linkList)
            {
                mParentEntry = parentEntry;
                mCurrentChildren = parentEntry.cachedChildren;
                mEntryList = entryList;
                mLinkList = linkList;
            }

            public override void processNewLines(string[] lines)
            {
                foreach (string line in lines)
                {
                    // no need to handle empty lines.
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // run the line through the regexp
                    var m = sLsPattern.Match(line);
                    if (m.Success == false)
                    {
                        continue;
                    }

                    // get the name
                    string name = m.Groups[6].Value;

                    // if the parent is root, we only accept selected items
                    if (mParentEntry.root)
                    {
                        bool found = false;
                        foreach (string approved in sRootLevelApprovedItems)
                        {
                            if (approved.Equals(name))
                            {
                                found = true;
                                break;
                            }
                        }

                        // if it's not in the approved list we skip this entry.
                        if (found == false)
                        {
                            continue;
                        }
                    }

                    // get the rest of the groups
                    string permissions = m.Groups[0].Value;
                    string owner = m.Groups[1].Value;
                    string group = m.Groups[2].Value;
                    string size = m.Groups[3].Value;
                    string date = m.Groups[4].Value;
                    string time = m.Groups[5].Value;
                    string info = null;

                    // and the type
                    int objectType = TYPE_OTHER;
                    switch (permissions[0])
                    {
                        case '-':
                            objectType = TYPE_FILE;
                            break;
                        case 'b':
                            objectType = TYPE_BLOCK;
                            break;
                        case 'c':
                            objectType = TYPE_CHARACTER;
                            break;
                        case 'd':
                            objectType = TYPE_DIRECTORY;
                            break;
                        case 'l':
                            objectType = TYPE_LINK;
                            break;
                        case 's':
                            objectType = TYPE_SOCKET;
                            break;
                        case 'p':
                            objectType = TYPE_FIFO;
                            break;
                    }


                    // now check what we may be linking to
                    if (objectType == TYPE_LINK)
                    {
                        string[] segments = StringHelperClass.StringSplit(name, "\\s->\\s", true); //$NON-NLS-1$

                        // we should have 2 segments
                        if (segments.Length == 2)
                        {
                            // update the entry name to not contain the link
                            name = segments[0];

                            // and the link name
                            info = segments[1];

                            // now get the path to the link
                            string[] pathSegments = StringHelperClass.StringSplit(info, FILE_SEPARATOR, true);
                            if (pathSegments.Length == 1)
                            {
                                // the link is to something in the same directory,
                                // unless the link is ..
                                if ("..".Equals(pathSegments[0])) //$NON-NLS-1$
                                {
                                    // set the type and we're done.
                                    objectType = TYPE_DIRECTORY_LINK;
                                }
                                else
                                {
                                    // either we found the object already
                                    // or we'll find it later.
                                }
                            }
                        }

                        // add an arrow in front to specify it's a link.
                        info = "-> " + info; //$NON-NLS-1$;
                    }

                    // get the entry, either from an existing one, or a new one
                    FileEntry entry = getExistingEntry(name);
                    if (entry == null)
                    {
                        entry = new FileEntry(mParentEntry, name, objectType, false); // isRoot
                    }

                    // add some misc info
                    entry.permissions_Renamed = permissions;
                    entry.size_Renamed = size;
                    entry.date_Renamed = date;
                    entry.time_Renamed = time;
                    entry.owner = owner;
                    entry.group = group;
                    if (objectType == TYPE_LINK)
                    {
                        entry.info_Renamed = info;
                    }

                    mEntryList.Add(entry);
                }
            }

            /// <summary>
            /// Queries for an already existing Entry per name </summary>
            /// <param name="name"> the name of the entry </param>
            /// <returns> the existing FileEntry or null if no entry with a matching
            /// name exists. </returns>
            private FileEntry getExistingEntry(string name)
            {
                for (int i = 0; i < mCurrentChildren.Length; i++)
                {
                    FileEntry e = mCurrentChildren[i];

                    // since we're going to "erase" the one we use, we need to
                    // check that the item is not null.
                    if (e != null)
                    {
                        // compare per name, case-sensitive.
                        if (name.Equals(e.name_Renamed))
                        {
                            // erase from the list
                            mCurrentChildren[i] = null;

                            // and return the object
                            return e;
                        }
                    }
                }

                // couldn't find any matching object, return null
                return null;
            }

            public override bool cancelled
            {
                get
                {
                    return false;
                }
            }

            public virtual void finishLinks()
            {
                // TODO Handle links in the listing service
            }
        }

        /// <summary>
        /// Classes which implement this interface provide a method that deals with asynchronous
        /// result from <code>ls</code> command on the device.
        /// </summary>
        /// <seealso cref= FileListingService#getChildren(com.android.ddmlib.FileListingService.FileEntry, boolean, com.android.ddmlib.FileListingService.IListingReceiver) </seealso>
        public interface IListingReceiver
        {
            void setChildren(FileEntry entry, FileEntry[] children);

            void refreshEntry(FileEntry entry);
        }

        /// <summary>
        /// Creates a File Listing Service for a specified <seealso cref="Device"/>. </summary>
        /// <param name="device"> The Device the service is connected to. </param>
        internal FileListingService(Device device)
        {
            mDevice = device;
        }

        /// <summary>
        /// Returns the root element. </summary>
        /// <returns> the <seealso cref="FileEntry"/> object representing the root element or
        /// <code>null</code> if the device is invalid. </returns>
        public FileEntry root
        {
            get
            {
                if (mDevice != null)
                {
                    if (mRoot == null)
                    {
                        mRoot = new FileEntry(null, "", TYPE_DIRECTORY, true); // isRoot -  name -  parent
                    }

                    return mRoot;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the children of a <seealso cref="FileEntry"/>.
        /// <p/>
        /// This method supports a cache mechanism and synchronous and asynchronous modes.
        /// <p/>
        /// If <var>receiver</var> is <code>null</code>, the device side <code>ls</code>
        /// command is done synchronously, and the method will return upon completion of the command.<br>
        /// If <var>receiver</var> is non <code>null</code>, the command is launched is a separate
        /// thread and upon completion, the receiver will be notified of the result.
        /// <p/>
        /// The result for each <code>ls</code> command is cached in the parent
        /// <code>FileEntry</code>. <var>useCache</var> allows usage of this cache, but only if the
        /// cache is valid. The cache is valid only for <seealso cref="FileListingService#REFRESH_RATE"/> ms.
        /// After that a new <code>ls</code> command is always executed.
        /// <p/>
        /// If the cache is valid and <code>useCache == true</code>, the method will always simply
        /// return the value of the cache, whether a <seealso cref="IListingReceiver"/> has been provided or not.
        /// </summary>
        /// <param name="entry"> The parent entry. </param>
        /// <param name="useCache"> A flag to use the cache or to force a new ls command. </param>
        /// <param name="receiver"> A receiver for asynchronous calls. </param>
        /// <returns> The list of children or <code>null</code> for asynchronous calls.
        /// </returns>
        /// <seealso cref= FileEntry#getCachedChildren() </seealso>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
        //ORIGINAL LINE: public FileEntry[] getChildren(final FileEntry entry, boolean useCache, final IListingReceiver receiver)
        public FileEntry[] getChildren(FileEntry entry, bool useCache, IListingReceiver receiver)
        {
            // first thing we do is check the cache, and if we already have a recent
            // enough children list, we just return that.
            if (useCache && entry.needFetch() == false)
            {
                return entry.cachedChildren;
            }

            // if there's no receiver, then this is a synchronous call, and we
            // return the result of ls
            if (receiver == null)
            {
                doLs(entry);
                return entry.cachedChildren;
            }

            // this is a asynchronous call.
            // we launch a thread that will do ls and give the listing
            // to the receiver

            ThreadStart runner = () => {
#if TODO
                				doLs(entry);
                
                				receiver.setChildren(entry, entry.getCachedChildren());
                
                				final FileEntry[] children = entry.getCachedChildren();
                				if (children.length > 0 && children[0].isApplicationPackage())
                				{
                					final HashMap<String, FileEntry> map = new HashMap<String, FileEntry>();
                
                					for (FileEntry child : children)
                					{
                						String path = child.getFullPath();
                						map.put(path, child);
                					}
                
                					// call pm.
                					String command = PM_FULL_LISTING;
                					try
                					{
                						/*mDevice.executeShellCommand(command, new MultiLineReceiver()
                						{
                							@Override public void processNewLines(String[] lines)
                							{
                								for (String line : lines)
                								{
                									if (line.length() > 0)
                									{
                										// get the filepath and package from the line
                										Matcher m = sPmPattern.matcher(line);
                										if (m.matches())
                										{
                											// get the children with that path
                											FileEntry entry = map.get(m.group(1));
                											if (entry != null)
                											{
                												entry.info = m.group(2);
                												receiver.refreshEntry(entry);
                											}
                										}
                									}
                								}
                							}
                							@Override public boolean isCancelled()
                							{
                								return false;
                							}
                						});*/
                					}
                					catch (Exception e)
                					{
                						// adb failed somehow, we do nothing.
                					}
                				}
                
                
                				// if another thread is pending, launch it
                				synchronized(mThreadList)
                				{
                					// first remove ourselves from the list
                					mThreadList.remove(this);
                
                					// then launch the next one if applicable.
                					if (mThreadList.size() > 0)
                					{
                						Thread t = mThreadList.get(0);
                						t.start();
                					}
                				}
                			}
#else
                throw new NotImplementedException();
#endif
            };

            var t = new Thread(runner);
            t.Name = "ls " + entry.fullPath;

            // we don't want to run multiple ls on the device at the same time, so we
            // store the thread in a list and launch it only if there's no other thread running.
            // the thread will launch the next one once it's done.
            lock (mThreadList)
            {
                // add to the list
                mThreadList.Add(t);

                // if it's the only one, launch it.
                if (mThreadList.Count == 1)
                {
                    t.Start();
                }
            }

            // and we return null.
            return null;
        }

        /// <summary>
        /// Returns the children of a <seealso cref="FileEntry"/>.
        /// <p/>
        /// This method is the explicit synchronous version of
        /// <seealso cref="#getChildren(FileEntry, boolean, IListingReceiver)"/>. It is roughly equivalent to
        /// calling
        /// getChildren(FileEntry, false, null)
        /// </summary>
        /// <param name="entry"> The parent entry. </param>
        /// <returns> The list of children </returns>
        /// <exception cref="TimeoutException"> in case of timeout on the connection when sending the command. </exception>
        /// <exception cref="AdbCommandRejectedException"> if adb rejects the command. </exception>
        /// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send any output
        ///            for a period longer than <var>maxTimeToOutputResponse</var>. </exception>
        /// <exception cref="IOException"> in case of I/O error on the connection. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public FileEntry[] getChildrenSync(final FileEntry entry) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
        public FileEntry[] getChildrenSync(FileEntry entry)
        {
            doLsAndThrow(entry);
            return entry.cachedChildren;
        }

        private void doLs(FileEntry entry)
        {
            try
            {
                doLsAndThrow(entry);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void doLsAndThrow(FileEntry entry) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
        private void doLsAndThrow(FileEntry entry)
        {
            // create a list that will receive the list of the entries
            List<FileEntry> entryList = new List<FileEntry>();

            // create a list that will receive the link to compute post ls;
            List<string> linkList = new List<string>();

            try
            {
                // create the command
                string command = "ls -l " + entry.fullEscapedPath; //$NON-NLS-1$

                // create the receiver object that will parse the result from ls
                LsReceiver receiver = new LsReceiver(entry, entryList, linkList);

                // call ls.
                mDevice.executeShellCommand(command, receiver);

                // finish the process of the receiver to handle links
                receiver.finishLinks();
            }
            finally
            {
                // at this point we need to refresh the viewer
                entry.fetchTime = Environment.TickCount;

                // sort the children and set them as the new children
                entryList.Sort();
                //Collections.sort(entryList, FileEntry.sEntryComparator);
                entry.children = entryList;
            }
        }

    }

}