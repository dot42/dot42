using System;
using System.IO;

using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class TestTryCatchSpecial
    {
        internal class StreamReader
        {
            public StreamReader(string s, bool y)
            {
                if(s == null) throw new ArgumentNullException();
                if(string.IsNullOrEmpty(s)) throw new ArgumentException();
                if(s != _codeFileName) throw new FileNotFoundException();
            }

            public void Close()
            {
                
            }
        }

        private static string TempFolder = "MonoTests.System.IO.Tests";
        private static string _codeFileName = "AFile.txt";

        [Test]
        public void TestCtor4()
        {
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader("", false);
                }
                catch (ArgumentException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 1: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "empty string error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader((string) null, false);
                }
                catch (ArgumentNullException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 2: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "null string error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader(TempFolder + "/nonexistentfile", false);
                }
                catch (FileNotFoundException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 3: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "fileNotFound error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader(TempFolder + "/nonexistentdir/file", false);
                }
                catch (FileNotFoundException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 4: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "dirNotFound error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], false);
                }
                catch (IOException)
                {
                    errorThrown = true;
                }
                catch (ArgumentException)
                {
                    // FIXME - the spec says 'IOExc', but the
                    //   compiler says 'ArgExc'...
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 5: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "invalid filename error not thrown");
            } 
            {
                // this is probably incestuous, but, oh well.
                StreamReader r = new StreamReader(_codeFileName, false);
                Assert.IsNotNull(r, "no stream reader");
                r.Close();
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader("", true);
                }
                catch (ArgumentException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 6: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "empty string error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader((string) null, true);
                }
                catch (ArgumentNullException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 7: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "null string error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader(TempFolder + "/nonexistentfile", true);
                }
                catch (FileNotFoundException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 8: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "fileNotFound error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader(TempFolder + "/nonexistentdir/file", true);
                }
                catch (FileNotFoundException)
                {
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 9: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "dirNotFound error not thrown");
            }
            {
                bool errorThrown = false;
                try
                {
                    new StreamReader("!$what? what? Huh? !$*#" + Path.InvalidPathChars[0], true);
                }
                catch (IOException)
                {
                    errorThrown = true;
                }
                catch (ArgumentException)
                {
                    // FIXME - the spec says 'IOExc', but the
                    //   compiler says 'ArgExc'...
                    errorThrown = true;
                }
                catch (Exception e)
                {
                    Assert.Fail("Incorrect exception thrown at 10: " + e.ToString());
                }
                Assert.IsTrue(errorThrown, "invalid filename error not thrown");
            }
            {
                // this is probably incestuous, but, oh well.
                StreamReader r = new StreamReader(_codeFileName, true);
                Assert.IsNotNull(r, "no stream reader");
                r.Close();
            }
        }

        [Test]
        public void TestInsideIf1()
        {
            object token = null;
            if (token != null)
            {
                try
                {
                }
                catch (ApplicationException)
                {
                }
            }
        }

        [Test]
        public void TestInsideIf2()
        {
            object token = null;
            if (token != null)
            {
                int savedStack = 2;
                try
                {
                    ICloneable expressionCreator = token as ICloneable;

                    if (expressionCreator != null)
                    {
                        expressionCreator.Clone();
                    }
                    else
                    {
                        throw new Exception("invalid calculator operation: " + token);
                    }
                }
                catch (ApplicationException)
                {
                    savedStack = 1;
                }
            }
            else
            {
                // the current executable is exhausted - pop it off the execution stack
                token = new object();
            }
        }
    }
}
