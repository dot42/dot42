using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Junit.Framework;

namespace Dot42.Tests.System.Xml.Linq
{
    public class TestXElement : TestCase
    {
        public void testNew1()
        {
            var e = new XElement("name");
            AssertNotNull(e);
            AssertEquals("name", e.Name.ToString());
        }

        public void testNew2()
        {
            var e = new XElement("root", new XElement("child"));
            AssertNotNull(e);
            AssertEquals("root", e.Name.ToString());
            AssertNotNull(e.Element("child"));
        }

        public void testAttr1()
        {
            var e = new XElement("root", new XAttribute("x", "1"));
            AssertNotNull(e);
            AssertNotNull(e.Attribute("x"));
        }

        public void testAttr2()
        {
            var e = new XElement("root", new XAttribute("x", "1"));
            AssertNotNull(e);
            AssertNotNull(e.Attribute("x"));
            e.Attribute("x").Remove();
            AssertNull(e.Attribute("x"));
        }

        public void testAttr3()
        {
            var e = new XElement("root", new XAttribute("x", "1"), new XAttribute("y", "2"));
            AssertNotNull(e);
            AssertNotNull(e.Attribute("x"));
            AssertNotNull(e.Attribute("y"));
            e.Attribute("x").Remove();
            AssertNull(e.Attribute("x"));
            AssertNotNull(e.Attribute("y"));
            e.Attribute("y").Remove();
            AssertNull(e.Attribute("y"));
        }

        public void testName2()
        {
            var e = new XElement("root");
            AssertEquals("root", e.Name.ToString());
            e.Name = "new";
            AssertEquals("new", e.Name.ToString());
        }

        public void test3()
        {
            XElement xmlTree = new XElement("Root",new XElement("Child",new XElement("GrandChild", "content")));

            int i = 0;
            
            IEnumerable<XElement> grandChild = xmlTree.Descendants("GrandChild");
            foreach (XElement el in grandChild.Ancestors())
            {
                switch (i)
                {
                    case 0:
                        AssertSame("Child", el.Name);
                        break;

                    case 1:
                        AssertSame("Root", el.Name);
                        break;

                    default:
                        Fail();
                        break;
                }

                i++;
            }
        }

        public void test4()
        {
            const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Meters xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Objects>
    <Object oGroup=""Passwords And Config"">
      <OBISCode>0.0.161.193.6.255</OBISCode>
      <Description>Password One</Description>
      <CanRead>Visible</CanRead>
      <CanWrite>Visible</CanWrite>
      <CanDisplay>Hidden</CanDisplay>
    </Object>
    <Object oGroup=""Passwords And Config"">
      <OBISCode>0.0.161.193.7.255</OBISCode>
      <Description>Password Two</Description>
      <CanRead>Visible</CanRead>
      <CanWrite>Visible</CanWrite>
      <CanDisplay>Hidden</CanDisplay>
    </Object>
 
 
  </Objects>
</Meters>";

            XDocument xObjectList = XDocument.Parse(xml);                                  
            var xObjects = xObjectList.Element("Meters");               
            var xObjects1 = xObjects.Element("Objects");               
            var xObjects2 = xObjects1.Elements("Object");
            var xObjects3 = xObjects2.SelectMany(p => p.Elements("OBISCode"));
            var xObjects4 = xObjects3.Select(p => p.Value);

            AssertNotNull(xObjects4);
            AssertSame(2, xObjects4.Count());
        }
    }
}
