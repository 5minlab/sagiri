using System;
using System.Text;
using UnityEngine;

namespace Assets.Sagiri {
    class LogEntry : IJsonSerializable {
        static readonly string[] logTypeStrTable;
        static LogEntry() {
            // optimize gc
            var enums = Enum.GetValues(typeof(LogType));

            Debug.Assert((int)LogType.Error == 0);
            Debug.Assert((int)LogType.Exception == 4);
            Debug.Assert((int)enums.Length == 5);

            logTypeStrTable = new string[enums.Length];
            for(int i = 0; i < enums.Length; i++) {
                var s = Enum.GetName(typeof(LogType), i);
                logTypeStrTable[i] = s;
            }
        }

        public readonly int uid;

        public readonly string log;
        public readonly string stacktrace;

        public readonly LogType level;
        public readonly DateTime time;

        static int nextUid = 1;

        readonly JsonObject json;

        public LogEntry(string log, string stacktrace, LogType lv, DateTime time) {
            uid = nextUid;
            nextUid += 1;

            this.log = log;
            this.stacktrace = stacktrace;
            this.level = lv;
            this.time = time;

            var lvName = logTypeStrTable[(int)lv];

            json = new JsonObject();
            json["l"] = new JsonString(log);
            json["t"] = new JsonString(lvName);
            // "2016.09.24.04.23.04"
            json["tm"] = new JsonString(time.ToString("yyyy.MM.dd.HH.mm.ss"));
            json["s"] = new JsonString(stacktrace);
            json["id"] = new JsonInt(uid);
        }

        public void AppendJson(StringBuilder sb) {
            json.AppendJson(sb);
        }
    }
}
