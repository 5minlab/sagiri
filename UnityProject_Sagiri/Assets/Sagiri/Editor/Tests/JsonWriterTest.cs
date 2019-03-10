using NUnit.Framework;
using Assets.Sagiri.Editor;

namespace Assets.Sagiri.Editor.Tests {
    class JsonStringTest {
        [Test]
        public void FilterTest() {
            var cases = new[]
            {
                new { a = "a\nb", b = @"a\nb" },
                new { a = "a\r\nb", b = @"a\nb" },
                new { a = @"a""b", b = @"a\""b" },
                new { a = @"a\b", b = @"a\\b" },
            };
            foreach (var c in cases) {
                var v = JsonString.Filter(c.a);
                Assert.AreEqual(c.b, v);
            }
        }
    }
}
