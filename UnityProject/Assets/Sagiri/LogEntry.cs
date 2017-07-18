using System;
using UnityEngine;

namespace Assets.Sagiri {
    class LogEntry : IJsonSerializable {
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

            json = new JsonObject();
            json["l"] = new JsonString(log);
            json["t"] = new JsonString(level.ToString());
            // "2016.09.24.04.23.04"
            json["tm"] = new JsonString(time.ToString("yyyy.MM.dd.HH.mm.ss"));
            json["s"] = new JsonString(stacktrace);
            json["id"] = new JsonInt(uid);
        }

        public string ToJson() {
            return json.ToJson();
        }
    }
}
