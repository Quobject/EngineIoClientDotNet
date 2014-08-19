using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Assert.Equal(queryObj["foo"], "bar");

            // Multiple assignments
            queryObj = ParseQS.Decode("france=grenoble&germany=mannheim");
            Assert.Equal(queryObj["france"], "grenoble");
            Assert.Equal(queryObj["germany"], "mannheim");

            // Assignments containing non-alphanumeric characters
            queryObj = ParseQS.Decode("india=new%20delhi");
            Assert.Equal(queryObj["india"], "new delhi");
        }

        //should construct a query string from an object'
        [Fact]
        public void Encode()
        {
            Dictionary<string, string> obj;

            obj = new Dictionary<string, string> {{"a", "b"}};
            var imObj = ImmutableDictionary<string, string>.Empty.AddRange(obj);
            Assert.Equal(ParseQS.Encode(imObj), "a=b");

            obj = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } };
            imObj = ImmutableDictionary<string, string>.Empty.AddRange(obj);
            Assert.Equal(ParseQS.Encode(imObj), "a=b&c=d");

            obj = new Dictionary<string, string> { { "a", "b" }, { "c", "tobi rocks" } };
            imObj = ImmutableDictionary<string, string>.Empty.AddRange(obj);
            Assert.Equal(ParseQS.Encode(imObj), "a=b&c=tobi%20rocks");

        }


    }
}
