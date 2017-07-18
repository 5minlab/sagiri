using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Sagiri {
    class CommandTree : IEnumerable<CommandAttribute> {

        private Dictionary<string, CommandTree> m_subcommands;
        private CommandAttribute m_command;

        public CommandTree() {
            m_subcommands = new Dictionary<string, CommandTree>();
        }

        public void Add(CommandAttribute cmd) {
            _add(cmd.m_command.ToLower().Split(' '), 0, cmd);
        }

        private void _add(string[] commands, int command_index, CommandAttribute cmd) {
            if (commands.Length == command_index) {
                m_command = cmd;
                return;
            }

            string token = commands[command_index];
            if (!m_subcommands.ContainsKey(token)) {
                m_subcommands[token] = new CommandTree();
            }
            m_subcommands[token]._add(commands, command_index + 1, cmd);
        }

        public IEnumerator<CommandAttribute> GetEnumerator() {
            if (m_command != null && m_command.m_command != null)
                yield return m_command;

            foreach (KeyValuePair<string, CommandTree> entry in m_subcommands) {
                foreach (CommandAttribute cmd in entry.Value) {
                    if (cmd != null && cmd.m_command != null)
                        yield return cmd;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public string Complete(string partialCommand) {
            return _complete(partialCommand.Split(' '), 0, "");
        }

        public string _complete(string[] partialCommand, int index, string result) {
            if (partialCommand.Length == index && m_command != null) {
                // this is a valid command... so we do nothing
                return result;
            } else if (partialCommand.Length == index) {
                // This is valid but incomplete.. print all of the subcommands
                Shell.LogCommand(result);
                foreach (string key in m_subcommands.Keys.OrderBy(m => m)) {
                    Shell.Log(result + " " + key);
                }
                return result + " ";
            } else if (partialCommand.Length == (index + 1)) {
                string partial = partialCommand[index];
                if (m_subcommands.ContainsKey(partial)) {
                    result += partial;
                    return m_subcommands[partial]._complete(partialCommand, index + 1, result);
                }

                // Find any subcommands that match our partial command
                List<string> matches = new List<string>();
                foreach (string key in m_subcommands.Keys.OrderBy(m => m)) {
                    if (key.StartsWith(partial)) {
                        matches.Add(key);
                    }
                }

                if (matches.Count == 1) {
                    // Only one command found, log nothing and return the complete command for the user input
                    return result + matches[0] + " ";
                } else if (matches.Count > 1) {
                    // list all the options for the user and return partial
                    Shell.LogCommand(result + partial);
                    foreach (string match in matches) {
                        Shell.Log(result + match);
                    }
                }
                return result + partial;
            }

            string token = partialCommand[index];
            if (!m_subcommands.ContainsKey(token)) {
                return result;
            }
            result += token + " ";
            return m_subcommands[token]._complete(partialCommand, index + 1, result);
        }

        public void Run(string commandStr) {
            // Split user input on spaces ignoring anything in qoutes
            Regex regex = new Regex(@""".*?""|[^\s]+");
            MatchCollection matches = regex.Matches(commandStr);
            string[] tokens = new string[matches.Count];
            for (int i = 0; i < tokens.Length; ++i) {
                tokens[i] = matches[i].Value.Replace("\"", "");
            }
            _run(tokens, 0);
        }

        static string[] emptyArgs = new string[0] { };
        private void _run(string[] commands, int index) {
            if (commands.Length == index) {
                RunCommand(emptyArgs);
                return;
            }

            string token = commands[index].ToLower();
            if (!m_subcommands.ContainsKey(token)) {
                RunCommand(commands.Skip(index).ToArray());
                return;
            }
            m_subcommands[token]._run(commands, index + 1);
        }

        private void RunCommand(string[] args) {
            if (m_command == null) {
                Shell.Log("command not found");
            } else if (m_command.m_runOnMainThread) {
                Shell.Queue(m_command, args);
            } else {
                m_command.m_callback(args);
            }
        }
    }
}
