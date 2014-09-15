using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Modules;
using System.Collections.Generic;




namespace Quobject.EngineIoClientDotNet_Tests.ModulesTests
{
    [TestClass]
    public class ParseQsTests
    {
        //should parse a querystring and return an object
        [TestMethod]
        public void Decode()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            // Single assignment
            var queryObj = ParseQS.Decode("foo=bar");
            Assert.AreEqual(queryObj["foo"], "bar");

            // Multiple assignments
            queryObj = ParseQS.Decode("france=grenoble&germany=mannheim");
            Assert.AreEqual(queryObj["france"], "grenoble");
            Assert.AreEqual(queryObj["germany"], "mannheim");

            // Assignments containing non-alphanumeric characters
            queryObj = ParseQS.Decode("india=new%20delhi");
            Assert.AreEqual(queryObj["india"], "new delhi");
        }

        //should construct a query string from an object'
        [TestMethod]
        public void Encode()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Dictionary<string, string> obj;

            obj = new Dictionary<string, string> {{"a", "b"}};
            var imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.AreEqual(ParseQS.Encode(imObj), "a=b");

            obj = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}};
            imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.AreEqual(ParseQS.Encode(imObj), "a=b&c=d");

            obj = new Dictionary<string, string> {{"a", "b"}, {"c", "tobi rocks"}};
            imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.AreEqual(ParseQS.Encode(imObj), "a=b&c=tobi%20rocks");

        }


    }
}
