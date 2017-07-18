using System.Collections.Generic;
using System.Text;

namespace Assets.Sagiri {
    interface IJsonSerializable {
        string ToJson();
    }

    public class JsonString : IJsonSerializable {
        readonly string val;

        public JsonString(string val) {
            this.val = val;
        }
        public string ToJson() {
            var content = Filter(val);
            var sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(content);
            sb.Append("\"");
            return sb.ToString();
        }

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

        public static string Filter(string val) {
            var sb = new StringBuilder(val);
            foreach (var t in tuples) {
                sb.Replace(t.prev, t.next);
            }
            return sb.ToString();
        }
    }

    class JsonInt : IJsonSerializable {
        readonly int val;
        public JsonInt(int val) {
            this.val = val;
        }
        public string ToJson() {
            return val.ToString();
        }
    }

    class JsonArray : IJsonSerializable {
        readonly List<IJsonSerializable> list = new List<IJsonSerializable>();
        public void Add(IJsonSerializable v) {
            list.Add(v);
        }

        public void Clear() {
            list.Clear();
        }

        public string ToJson() {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < list.Count; i++) {
                var r = list[i];
                sb.Append(r.ToJson());
                if (i < list.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    class JsonObject : IJsonSerializable {
        class Pair {
            public string key;
            public IJsonSerializable value;
            public Pair(string key, IJsonSerializable val) {
                this.key = key;
                this.value = val;
            }
        }

        readonly List<Pair> pairs = new List<Pair>();

        public IJsonSerializable this[string key]
        {
            set
            {
                var p = new Pair(key, value);
                pairs.Add(p);
            }
        }

        public void Clear() {
            pairs.Clear();
        }

        public string ToJson() {
            var sb = new StringBuilder();
            sb.Append("{");
            for(int i = 0; i < pairs.Count; i++) {
                var p = pairs[i];
                sb.AppendFormat(@"""{0}"":{1}", p.key, p.value.ToJson());
                if(i < pairs.Count-1) {
                    sb.Append(",");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
