using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Sagiri {
    public class Console {
        /// <summary>
        /// Max number of lines in the console output
        /// </summary>
        const int MAX_LINES = 100;

        readonly static Console Instance = new Console();
        readonly List<LogEntry> m_output = new List<LogEntry>();

        public static void Log(string str, string stacktrace, LogType lv, DateTime time) {
            Instance.m_output.Add(new LogEntry(str, stacktrace, lv, time));
            if (Instance.m_output.Count > MAX_LINES) {
                Instance.m_output.RemoveAt(0);
            }
        }

        /* Callback for Unity logging */
        public static void LogCallback(string logString, string stackTrace, LogType type) {
            var now = DateTime.Now;
            Console.Log(logString, stackTrace, type, now);
        }

        static readonly JsonArray jsonBuilder = new JsonArray();

#if !NETFX_CORE
        // Our routes
        [Route("^/console/fetch$")]
        public static void FetchLog(RequestContext context) {
            // 로그를 어디부터 이어서 받을지 정할수 있도록
            // 매번 전체 로그를 보낼 필요는 없을것이다
            var last = context.Request.QueryString.Get("last");
            int lastId = 0;
            if (!int.TryParse(last, out lastId)) {
                lastId = 0;
            }

            // 필요한것만 내려주도록
            jsonBuilder.Clear();
            foreach (var r in Instance.m_output) {
                if (r.uid > lastId) {
                    jsonBuilder.Add(r);
                }
            }
            var sb = new StringBuilder();
            jsonBuilder.AppendJson(sb);
            var json = sb.ToString();
            context.Response.WriteString(json, "application/json");
        }
#endif
    }
}