using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Modules;
using System.Collections.Generic;
using Xunit;


namespace Quobject.EngineIoClientDotNet_Tests.ModulesTests
{
    public class ParseQsTests
    {
        //should parse a querystring and return an object
        [Fact]
        public void Decode()
        {
            // Single assignment
            var queryObj = ParseQS.Decode("foo=bar");
            Assert.Equal("bar", queryObj["foo"]);

            // Multiple assignments
            queryObj = ParseQS.Decode("france=grenoble&germany=mannheim");
            Assert.Equal("grenoble", queryObj["france"]);
            Assert.Equal("mannheim", queryObj["germany"]);

            // Assignments containing non-alphanumeric characters
            queryObj = ParseQS.Decode("india=new%20delhi");
            Assert.Equal("new delhi", queryObj["india"]);
        }

        //should construct a query string from an object'
        [Fact]
        public void Encode()
        {

            Dictionary<string, string> obj;

            obj = new Dictionary<string, string> {{"a", "b"}};
            var imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.Equal("a=b", ParseQS.Encode(imObj));

            obj = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}};
            imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.Equal("a=b&c=d", ParseQS.Encode(imObj));

            obj = new Dictionary<string, string> {{"a", "b"}, {"c", "tobi rocks"}};
            imObj = ImmutableDictionary.Create<string, string>().AddRange(obj);
            Assert.Equal("a=b&c=tobi%20rocks", ParseQS.Encode(imObj));

        }


    }
}
