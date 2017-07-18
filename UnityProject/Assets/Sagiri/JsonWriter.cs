using System.Text;

namespace Assets.Sagiri {
    interface IJsonSerializable {
        string ToJson();
    }

    public class JsonWriter {
        struct ReplaceTuple {
            public string prev;
            public string next;
            public ReplaceTuple(string prev, string next) {
                this.prev = prev;
                this.next = next;
            }
        }

        static readonly ReplaceTuple[] tuples = new ReplaceTuple[]
        {
            // \n은 무시. \n으로 충분할거다
            new ReplaceTuple("\r", ""),

            // priority
            new ReplaceTuple(@"\", @"\\"),
            new ReplaceTuple(@"""", @"\"""),

            new ReplaceTuple("\b", "\\b"),
            new ReplaceTuple("\f", "\\f"),
            new ReplaceTuple("\n", "\\n"),
            new ReplaceTuple("\t", "\\t"),
        };

        public static string ToJsonString(string val) {
            var sb = new StringBuilder(val);
            foreach (var t in tuples) {
                sb.Replace(t.prev, t.next);
            }
            return sb.ToString();
        }
    }
}
