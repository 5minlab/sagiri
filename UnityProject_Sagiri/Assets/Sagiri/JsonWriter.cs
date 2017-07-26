using System.Collections.Generic;
using System.Text;

namespace Assets.Sagiri {
    interface IJsonSerializable {
        void AppendJson(StringBuilder sb);
    }

    public class JsonString : IJsonSerializable {
        readonly string val;

        public JsonString(string val) {
            this.val = val;
        }
        public void AppendJson(StringBuilder sb) {
            var content = Filter(val);
            sb.Append("\"");
            sb.Append(content);
            sb.Append("\"");
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
        public void AppendJson(StringBuilder sb) {
            sb.Append(val);
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

        public void AppendJson(StringBuilder sb) {
            sb.Append("[");
            for (int i = 0; i < list.Count; i++) {
                var r = list[i];
                r.AppendJson(sb);
                if (i < list.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append("]");
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

        public void AppendJson(StringBuilder sb) {
            sb.Append("{");
            for(int i = 0; i < pairs.Count; i++) {
                var p = pairs[i];

                sb.Append("\"");
                sb.Append(p.key);
                sb.Append("\"");

                sb.Append(":");

                p.value.AppendJson(sb);

                if(i < pairs.Count-1) {
                    sb.Append(",");
                }
            }
            sb.Append("}");
        }
    }
}
