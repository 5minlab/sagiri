using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Sagiri {
    struct LogEntry {
        public readonly int uid;

        public readonly string log;
        public readonly string stacktrace;

        public readonly LogType level;
        public readonly DateTime time;

        static int nextUid = 1;

        public LogEntry(string log, string stacktrace, LogType lv, DateTime time) {
            uid = nextUid;
            nextUid += 1;

            this.log = log;
            this.stacktrace = stacktrace;
            this.level = lv;
            this.time = time;
        }

        public string ToJson() {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat(@"""id"":{0}", uid);
            sb.Append(",");
            // "2016.09.24.04.23.04"
            sb.AppendFormat(@"""tm"":""{0}""", time.ToString("yyyy.MM.dd.HH.mm.ss"));
            sb.Append(",");
            sb.AppendFormat(@"""t"":""{0}""", level.ToString());
            sb.Append(",");
            sb.AppendFormat(@"""l"":""{0}""", log.Replace("\n", "\\n"));
            sb.Append(",");
            sb.AppendFormat(@"""s"":""{0}""", stacktrace.Replace("\n", "\\n"));
            sb.Append("}");
            return sb.ToString();
        }
    }

    class LogEntryListJsonBuilder {
        readonly List<LogEntry> list = new List<LogEntry>();

        public void Clear() {
            list.Clear();
        }

        public void Add(LogEntry e) {
            list.Add(e);
        }

        public string Build() {
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
}
