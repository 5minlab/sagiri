using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;

namespace Assets.Sagiri {
    struct QueuedCommand {
        public CommandAttribute command;
        public string[] args;
    }

    class Shell {
        static Shell Instance = new Shell();

        /// <summary>
        /// Max number of lines in the console output
        /// </summary>
        const int MAX_LINES = 100;

        /// <summary>
        /// Maximum number of commands stored in the history
        /// </summary>
        const int MAX_HISTORY = 50;

        /// <summary>
        /// Prefix for user inputted command
        /// </summary>
        const string COMMAND_OUTPUT_PREFIX = "> ";

        readonly CommandTree m_commands = new CommandTree();
        readonly Queue<QueuedCommand> m_commandQueue = new Queue<QueuedCommand>();
        readonly List<string> m_history = new List<string>();
        readonly List<string> m_output = new List<string>();

        public Shell() {
            RegisterAttributes();
        }

        /* Logs user input to output */
        public static void LogCommand(string cmd) {
            Log(COMMAND_OUTPUT_PREFIX + cmd);
        }

        /* Logs string to output */
        public static void Log(string str) {
            Instance.m_output.Add(str);
            if (Instance.m_output.Count > MAX_LINES) {
                Instance.m_output.RemoveAt(0);
            }
        }

        /* Find command based on partial string */
        public static string Complete(string partialCommand) {
            return Instance.m_commands.Complete(partialCommand);
        }

        /* Get a previously ran command from the history */
        public static string PreviousCommand(int index) {
            return index >= 0 && index < Instance.m_history.Count ? Instance.m_history[index] : null;
        }

        public static void Update() {
            while (Instance.m_commandQueue.Count > 0) {
                QueuedCommand cmd = Instance.m_commandQueue.Dequeue();
                cmd.command.m_callback(cmd.args);
            }
        }

        /* Returns the output */
        public static string Output() {
            return string.Join("\n", Instance.m_output.ToArray());
        }

        /* Register a new console command */
        public static void RegisterCommand(string command, string desc, CommandAttribute.Callback callback, bool runOnMainThread = true) {
            if (command == null || command.Length == 0) {
                throw new Exception("Command String cannot be empty");
            }

            CommandAttribute cmd = new CommandAttribute(command, desc, runOnMainThread);
            cmd.m_callback = callback;

            Instance.m_commands.Add(cmd);
        }

        /* Queue a command to be executed on update on the main thread */
        public static void Queue(CommandAttribute command, string[] args) {
            QueuedCommand queuedCommand = new QueuedCommand();
            queuedCommand.command = command;
            queuedCommand.args = args;
            Instance.m_commandQueue.Enqueue(queuedCommand);
        }

        /* Update history with a new command */
        private void RecordCommand(string command) {
            m_history.Insert(0, command);
            if (m_history.Count > MAX_HISTORY)
                m_history.RemoveAt(m_history.Count - 1);
        }

        /* Execute a command */
        public static void Run(string str) {
            if (str.Length > 0) {
                LogCommand(str);
                Instance.RecordCommand(str);
                Instance.m_commands.Run(str);
            }
        }

        private void RegisterAttributes() {
#if !NETFX_CORE
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                // HACK: IL2CPP crashes if you attempt to get the methods of some classes in these assemblies.
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("mscorlib")) {
                    continue;
                }
                foreach (Type type in assembly.GetTypes()) {
                    // FIXME add support for non-static methods (FindObjectByType?)
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                        CommandAttribute[] attrs = method.GetCustomAttributes(typeof(CommandAttribute), true) as CommandAttribute[];
                        if (attrs.Length == 0)
                            continue;

                        CommandAttribute.Callback cb = Delegate.CreateDelegate(typeof(CommandAttribute.Callback), method, false) as CommandAttribute.Callback;
                        if (cb == null) {
                            CommandAttribute.CallbackSimple cbs = Delegate.CreateDelegate(typeof(CommandAttribute.CallbackSimple), method, false) as CommandAttribute.CallbackSimple;
                            if (cbs != null) {
                                cb = delegate (string[] args) {
                                    cbs();
                                };
                            }
                        }

                        if (cb == null) {
                            Debug.LogError(string.Format("Method {0}.{1} takes the wrong arguments for a console command.", type, method.Name));
                            continue;
                        }

                        // try with a bare action
                        foreach (CommandAttribute cmd in attrs) {
                            if (string.IsNullOrEmpty(cmd.m_command)) {
                                Debug.LogError(string.Format("Method {0}.{1} needs a valid command name.", type, method.Name));
                                continue;
                            }

                            cmd.m_callback = cb;
                            m_commands.Add(cmd);
                        }
                    }
                }
            }
#endif
        }

#if !NETFX_CORE
        /* Clear all output from console */
        [Command("clear", "clears console output", false)]
        public static void Clear() {
            Instance.m_output.Clear();
        }

        /* Print a list of all console commands */
        [Command("help", "prints commands", false)]
        public static void Help() {

            string help = "Commands:";
            foreach (CommandAttribute cmd in Instance.m_commands.OrderBy(m => m.m_command)) {
                help += string.Format("\n{0} : {1}", cmd.m_command, cmd.m_help);
            }

            Log(help);
        }

        [Route("^/shell/out$")]
        public static void Output(RequestContext context) {
            context.Response.WriteString(Output());
        }

        [Route("^/shell/run$")]
        public static void Run(RequestContext context) {
            string command = Uri.UnescapeDataString(context.Request.QueryString.Get("command"));
            if (!string.IsNullOrEmpty(command))
                Run(command);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.StatusDescription = "OK";
        }

        [Route("^/shell/commandHistory$")]
        public static void History(RequestContext context) {
            string index = context.Request.QueryString.Get("index");

            string previous = null;
            if (!string.IsNullOrEmpty(index))
                previous = PreviousCommand(System.Int32.Parse(index));

            context.Response.WriteString(previous);
        }

        [Route("^/shell/complete$")]
        public static void Complete(RequestContext context) {
            string partialCommand = context.Request.QueryString.Get("command");

            string found = null;
            if (partialCommand != null)
                found = Complete(partialCommand);

            context.Response.WriteString(found);
        }
#endif
    }
}
