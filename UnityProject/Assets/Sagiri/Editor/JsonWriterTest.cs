using NUnit.Framework;

namespace Assets.Sagiri.Editor {
    class JsonWriterTest {
        [Test]
        public void ToJsonString_String_Test() {
            var cases = new[]
            {
                new { a = "a\nb", b = @"a\nb" },
                new { a = "a\r\nb", b = @"a\nb" },
                new { a = @"a""b", b = @"a\""b" },
                new { a = @"a\b", b = @"a\\b" },
            };
            foreach(var c in cases) {
                var v = JsonWriter.ToJsonString(c.a);
                Assert.AreEqual(c.b, v);
            }
        }
    }
}
