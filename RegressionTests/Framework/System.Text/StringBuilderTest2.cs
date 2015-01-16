// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// StringBuilderTest.dll - NUnit Test Cases for the System.Text.StringBuilder class
// 
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// NOTES: I've also run all these tests against the MS implementation of 
// System.Text.StringBuilder to confirm that they return the same results
// and they do.
//
// TODO: Add tests for the AppendFormat methods once the AppendFormat methods
// are implemented in the StringBuilder class itself
//
// TODO: Potentially add more variations on Insert / Append tests for all the
// possible types.  I don't really think that's necessary as they all
// pretty much just do .ToString().ToCharArray() and then use the Append / Insert
// CharArray function.  The ToString() bit for each type should be in the unit
// tests for those types, and the unit test for ToCharArray should be in the 
// string test type.  If someone wants to add those tests here for completness 
// (and some double checking) then feel free :)
//

using NUnit.Framework;
using System.Text;
using System;

namespace MonoTests.System.Text {

	[TestFixture]
	public class StringBuilderTest2  {


	[Test]
	public void Append1()
	{
		StringBuilder sb = new StringBuilder ();
		sb.Append('0', 3);
		Assert.AreEqual ("000", sb.ToString (), "#1");
	}

}

}
