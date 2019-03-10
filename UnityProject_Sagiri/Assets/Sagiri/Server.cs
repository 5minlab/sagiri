using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace Assets.Sagiri {
#if NETFX_CORE
    public class RequestContext {
    }
#else
    public class RequestContext {
        public HttpListenerContext context;
        public Match match;
        public bool pass;
        public string path;
        public int currentRoute;

        public HttpListenerRequest Request { get { return context.Request; } }
        public HttpListenerResponse Response { get { return context.Response; } }

        public RequestContext(HttpListenerContext ctx) {
            context = ctx;
            match = null;
            pass = false;
            path = WWW.UnEscapeURL(context.Request.Url.AbsolutePath);
            if (path == "/")
                path = "/index.html";
            currentRoute = 0;
        }
    }
#endif

#if !NETFX_CORE
    public class Server : MonoBehaviour {
        /// <summary>
        /// allow only one server object
        /// </summary>
        static Server s_instance = null;

        [SerializeField]
        int portCandidate = 55055;

        [SerializeField]
        int maxRetryCount = 10;

        [SerializeField]
        bool dontDestroyOnLoad = false;

        [SerializeField]
        [ShowOnly]
        int _port = -1;
        public int Port
        {
            get { return _port; }
            private set { _port = value; }
        }

        public string Host
        {
            get
            {
#if UNITY_WSA
                return "127.0.0.1";
#else
                // error CS0619: `UnityEngine.Network' is obsolete: `
                // The legacy networking system has been removed in Unity 2018.2.
                // Use Unity Multiplayer and NetworkIdentity instead.'
                // return Network.player.ipAddress;
                return "127.0.0.1";
#endif
            }
        }

        [SerializeField]
        public bool RegisterLogCallback = false;

        [SerializeField]
        bool showGUI = true;

        private static Thread mainThread;
        private static string fileRoot;
        private static HttpListener listener;
        private static List<RouteAttribute> registeredRoutes;
        private static Queue<RequestContext> mainRequests = new Queue<RequestContext>();

        // List of supported files
        // FIXME add an api to register new types
        private static Dictionary<string, string> fileTypes = new Dictionary<string, string>
        {
            {"js",   "application/javascript"},
            {"json", "application/json"},
            {"jpg",  "image/jpeg" },
            {"jpeg", "image/jpeg"},
            {"gif",  "image/gif"},
            {"png",  "image/png"},
            {"css",  "text/css"},
            {"htm",  "text/html"},
            {"html", "text/html"},
            {"ico",  "image/x-icon"},
        };

        public virtual void Awake() {
            // check singleton
            if(s_instance == null) {
                s_instance = this;
            } else {
                GameObject.Destroy(gameObject);
                return;
            }

            if(dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }

            mainThread = Thread.CurrentThread;
            fileRoot = Path.Combine(Application.streamingAssetsPath, "Sagiri");

            // Start server
            // 이전에 사용한 서버에 문제가 생겨서 포트번호를 다시 쓸수 없을지 모른다
            // 그러면 다음 포트번호로 접속을 시도해보자
            var success = false;
            for(int i = 0; i < maxRetryCount; i++) {
                var portNum = portCandidate + i;
                try {
                    listener = new HttpListener();
                    listener.Prefixes.Add("http://*:" + portNum + "/");
                    listener.Start();
                    listener.BeginGetContext(ListenerCallback, null);
                    Debug.Log("Starting Sagiri Server on " + Host + ":" + portNum);

                    success = true;
                    Port = portNum;
                    StartCoroutine(HandleRequests());
                    break;

                } catch (SocketException e) {
                    Debug.LogException(e, this);
                    Debug.LogFormat(this, "Cannot use Port {0}, use next port", portNum);
                }
            }

            if(!success) {
                Debug.Log("Cannot execute Sagiri server");
            }
        }

        public void OnApplicationPause(bool paused) {
            if (paused) {
                listener.Stop();
            } else {
                listener.Start();
                listener.BeginGetContext(ListenerCallback, null);
            }
        }

        public virtual void OnDestroy() {
            if(s_instance == this) {
                s_instance = null;
            }

            listener.Stop();
            listener.Close();
            listener = null;
            Port = -1;
        }

        private void RegisterRoutes() {
            if (registeredRoutes == null) {
                registeredRoutes = new List<RouteAttribute>();

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    foreach (Type type in assembly.GetTypes()) {
                        // FIXME add support for non-static methods (FindObjectByType?)
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                            RouteAttribute[] attrs = method.GetCustomAttributes(typeof(RouteAttribute), true) as RouteAttribute[];
                            if (attrs.Length == 0)
                                continue;

                            RouteAttribute.Callback cbm = Delegate.CreateDelegate(typeof(RouteAttribute.Callback), method, false) as RouteAttribute.Callback;
                            if (cbm == null) {
                                Debug.LogError(string.Format("Method {0}.{1} takes the wrong arguments for a console route.", type, method.Name));
                                continue;
                            }

                            // try with a bare action
                            foreach (RouteAttribute route in attrs) {
                                if (route.m_route == null) {
                                    Debug.LogError(string.Format("Method {0}.{1} needs a valid route regexp.", type, method.Name));
                                    continue;
                                }

                                route.m_callback = cbm;
                                registeredRoutes.Add(route);
                            }
                        }
                    }
                }
                RegisterFileHandlers();
            }
        }

        static void FindFileType(RequestContext context, bool download, out string path, out string type) {
            path = Path.Combine(fileRoot, context.match.Groups[1].Value);

            string ext = Path.GetExtension(path).ToLower().TrimStart(new char[] { '.' });
            if (download || !fileTypes.TryGetValue(ext, out type))
                type = "application/octet-stream";
        }


        public delegate void FileHandlerDelegate(RequestContext context, bool download);
        static void WWWFileHandler(RequestContext context, bool download) {
            string path, type;
            FindFileType(context, download, out path, out type);

            WWW req = new WWW(path);
            while (!req.isDone) {
                Thread.Sleep(0);
            }

            if (string.IsNullOrEmpty(req.error)) {
                context.Response.ContentType = type;
                if (download)
                    context.Response.AddHeader("Content-disposition", string.Format("attachment; filename={0}", Path.GetFileName(path)));

                context.Response.WriteBytes(req.bytes);
                return;
            }

            if (req.error.StartsWith("Couldn't open file")) {
                context.pass = true;
            } else {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = string.Format("Fatal error:\n{0}", req.error);
            }
        }

        static void FileHandler(RequestContext context, bool download) {
            string path, type;
            FindFileType(context, download, out path, out type);

            if (File.Exists(path)) {
                context.Response.WriteFile(path, type, download);
            } else {
                context.pass = true;
            }
        }

        static void RegisterFileHandlers() {
            string pattern = string.Format("({0})", string.Join("|", fileTypes.Select(x => x.Key).ToArray()));
            RouteAttribute downloadRoute = new RouteAttribute(string.Format(@"^/download/(.*\.{0})$", pattern));
            RouteAttribute fileRoute = new RouteAttribute(string.Format(@"^/(.*\.{0})$", pattern));

            bool needs_www = fileRoot.Contains("://");
            downloadRoute.m_runOnMainThread = needs_www;
            fileRoute.m_runOnMainThread = needs_www;

            FileHandlerDelegate callback = FileHandler;
            if (needs_www)
                callback = WWWFileHandler;

            downloadRoute.m_callback = delegate (RequestContext context) { callback(context, true); };
            fileRoute.m_callback = delegate (RequestContext context) { callback(context, false); };

            registeredRoutes.Add(downloadRoute);
            registeredRoutes.Add(fileRoute);
        }

        void OnEnable() {
            if (RegisterLogCallback) {
                // Capture Console Logs
                Application.logMessageReceived += Console.LogCallback;
            }
        }

        void OnDisable() {
            if (RegisterLogCallback) {
                Application.logMessageReceived -= Console.LogCallback;
            }
        }

        void Update() {
            Shell.Update();
        }

        string hostAndPort = "";
        private void OnGUI() {
            if(!showGUI) { return; }

            if (hostAndPort == "" && Port > 0) {
                var sb = new StringBuilder();
                sb.Append("Sagiri Server ");
                sb.Append(Host);
                sb.Append(":");
                sb.Append(Port);
                hostAndPort = sb.ToString();
            }

            if(hostAndPort.Length > 0) {
                GUI.Label(new Rect(0, 0, 400, 100), hostAndPort);
            }
        }

        void ListenerCallback(IAsyncResult result) {
            RequestContext context = new RequestContext(listener.EndGetContext(result));

            HandleRequest(context);

            if (listener.IsListening) {
                listener.BeginGetContext(ListenerCallback, null);
            }
        }

        void HandleRequest(RequestContext context) {
            RegisterRoutes();

            try {
                bool handled = false;

                for (; context.currentRoute < registeredRoutes.Count; ++context.currentRoute) {
                    RouteAttribute route = registeredRoutes[context.currentRoute];
                    Match match = route.m_route.Match(context.path);
                    if (!match.Success)
                        continue;

                    if (!route.m_methods.IsMatch(context.Request.HttpMethod))
                        continue;

                    // Upgrade to main thread if necessary
                    if (route.m_runOnMainThread && Thread.CurrentThread != mainThread) {
                        lock (mainRequests) {
                            mainRequests.Enqueue(context);
                        }
                        return;
                    }

                    context.match = match;
                    route.m_callback(context);
                    handled = !context.pass;
                    if (handled)
                        break;
                }

                if (!handled) {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusDescription = "Not Found";
                }
            } catch (Exception exception) {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = string.Format("Fatal error:\n{0}", exception);

                Debug.LogException(exception);
            }

            context.Response.OutputStream.Close();
        }

        IEnumerator HandleRequests() {
            while (true) {
                while (mainRequests.Count == 0) {
                    // yield return new WaitForEndOfFrame();
                    yield return null;
                }

                RequestContext context = null;
                lock (mainRequests) {
                    context = mainRequests.Dequeue();
                }

                HandleRequest(context);
            }
        }
    }
#endif
}
