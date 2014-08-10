using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace Quobject.EngineIoClientDotNet_Tests.Modules_Tests
{
    public class ParseQS_Tests
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
            Assert.Equal(ParseQS.Encode(obj),"a=b");

            obj = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } };
            Assert.Equal(ParseQS.Encode(obj), "a=b&c=d");

            obj = new Dictionary<string, string> { { "a", "b" }, { "c", "tobi rocks" } };
            Assert.Equal(ParseQS.Encode(obj), "a=b&c=tobi%20rocks");

        }


    }
}
