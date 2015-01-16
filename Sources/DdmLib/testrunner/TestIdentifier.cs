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

namespace Dot42.DdmLib.testrunner
{

	/// <summary>
	/// Identifies a parsed instrumentation test.
	/// </summary>
	public class TestIdentifier
	{

		private readonly string mClassName;
		private readonly string mTestName;

		/// <summary>
		/// Creates a test identifier.
		/// </summary>
		/// <param name="className"> fully qualified class name of the test. Cannot be null. </param>
		/// <param name="testName"> name of the test. Cannot be null. </param>
		public TestIdentifier(string className, string testName)
		{
			if (className == null || testName == null)
			{
				throw new System.ArgumentException("className and testName must " + "be non-null");
			}
			mClassName = className;
			mTestName = testName;
		}

		/// <summary>
		/// Returns the fully qualified class name of the test.
		/// </summary>
		public virtual string className
		{
			get
			{
				return mClassName;
			}
		}

		/// <summary>
		/// Returns the name of the test.
		/// </summary>
		public virtual string testName
		{
			get
			{
				return mTestName;
			}
		}

		/// <summary>
		/// Tests equality by comparing class and method name.
		/// </summary>
		public override bool Equals(object other)
		{
			if (!(other is TestIdentifier))
			{
				return false;
			}
			TestIdentifier otherTest = (TestIdentifier)other;
			return className.Equals(otherTest.className) && testName.Equals(otherTest.testName);
		}

		/// <summary>
		/// Generates hashCode based on class and method name.
		/// </summary>
		public override int GetHashCode()
		{
			return className.GetHashCode() * 31 + testName.GetHashCode();
		}

		/// <summary>
		/// Generates user friendly string.
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}#{1}", className, testName);
		}
	}

}