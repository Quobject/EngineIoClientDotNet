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
            string result;
            queryObj.TryGetValue("foo", out result);                      
            Assert.Equal(result, "bar");

            // Multiple assignments
            queryObj = ParseQS.Decode("france=grenoble&germany=mannheim");

            queryObj.TryGetValue("france", out result);
            Assert.Equal(result, "grenoble");

            queryObj.TryGetValue("germany", out result);
            Assert.Equal(result, "mannheim");


            // Assignments containing non-alphanumeric characters
            queryObj = ParseQS.Decode("india=new%20delhi");
            queryObj.TryGetValue("india", out result);
            Assert.Equal(result, "new delhi");

        }

        //should construct a query string from an object'
        [Fact]
        public void Encode()
        {
            var imObj = new Dictionary<string, string> {{"a", "b"}};
            Assert.Equal(ParseQS.Encode(imObj), "a=b");

            imObj = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}};
            Assert.Equal(ParseQS.Encode(imObj), "a=b&c=d");


            imObj = new Dictionary<string, string> {{"a", "b"}, {"c", "tobi rocks"}};
            Assert.Equal("a=b&c=tobi%20rocks", ParseQS.Encode(imObj));

        }


    }
}
