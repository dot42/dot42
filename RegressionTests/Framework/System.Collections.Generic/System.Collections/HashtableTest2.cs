// ArrayListTest.cs - NUnit Test Cases for the System.Collections.ArrayList class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell (http://www.novell.com)
// 

using System;
using System.Collections;

using NUnit.Framework;

namespace Dot42.Tests.System.Collections
{
    [TestFixture]
    public class HashtableTest
    {

        [Test]
        public void TestNull()
        {
            var hashTable = new Hashtable();
            hashTable.Add("one-null", null);
            hashTable.Add("one-obj", 12);
            hashTable["two-null"] = null;
            hashTable["two-obj"] = 24;

            Assert.IsNull(hashTable["one-null"]);
            Assert.AssertEquals(12, hashTable["one-obj"]);
            Assert.IsNull(hashTable["two-null"]);
            Assert.AssertEquals(24, hashTable["two-obj"]);
        }
    }
}

