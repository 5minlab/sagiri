package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"path"
	"strings"
)

/*
viewer
https://github.com/Sacred-Seed-Studio/Unity-File-Debug

server
https://github.com/proletariatgames/CUDLR
*/

const (
	Port = "55056"
)

type LogEntry struct {
	// "2016.09.24.04.23.04"
	Time  string `json:"tm"`
	Type  string `json:"t"`
	Log   string `json:"l"`
	Stack string `json:"s"`
}

func newDummyAssertLog() LogEntry {
	return LogEntry{
		Type: "Assert",
		Time: "2016.09.24.04.23.04",
		Log:  "Conditionless assertion",
		Stack: `UnityEngine.Debug:LogAssertion(Object)
Debug:LogAssertion(Object) (at Assets/UnityFileDebug/Lib/UnityFileDebug.cs:113)
Tester:Update() (at Assets/UnityFileDebug/Demo/Tester.cs:27)
`,
	}
}

func newDummyFormatLog() LogEntry {
	return LogEntry{
		Type: "Log",
		Time: "2016.09.24.04.23.04",
		Log:  "<color=green>This is a green message!</color>",
		Stack: `UnityEngine.Debug:LogFormat(String, Object[])
Debug:LogFormat(String, DLogType, Object[]) (at Assets/UnityFileDebug/Lib/UnityFileDebug.cs:138)
Tester:Update() (at Assets/UnityFileDebug/Demo/Tester.cs:28)
`,
	}
}

func newDummyWarningLog() LogEntry {
	return LogEntry{
		Type: "Warning",
		Time: "2016.09.24.04.23.04",
		Log:  "This is a bad thing that you should be aware of",
		Stack: `UnityEngine.Debug:Log(Object)
Debug:Log(Object, DLogType) (at Assets/UnityFileDebug/Lib/UnityFileDebug.cs:136)
Tester:Update() (at Assets/UnityFileDebug/Demo/Tester.cs:20)
`,
	}
}

func newDummyErrorLog() LogEntry {
	return LogEntry{
		Type: "Error",
		Time: "2016.09.24.04.23.04",
		Log:  "Error",
		Stack: `UnityEngine.Debug:LogError(Object)
Debug:LogError(Object, DLogType) (at Assets/UnityFileDebug/Lib/UnityFileDebug.cs:143)
Tester:Update() (at Assets/UnityFileDebug/Demo/Tester.cs:26)
`,
	}
}

func newDummyLogs() []LogEntry {
	return []LogEntry{
		newDummyAssertLog(),
		newDummyErrorLog(),
		newDummyWarningLog(),
		newDummyFormatLog(),
	}
}

func renderStatic(w http.ResponseWriter, r *http.Request, target string, contentType string) {
	cleaned := path.Clean(target)
	fp := path.Join(".", cleaned)
	cleanedFp := path.Clean(fp)

	w.Header().Set("Content-Type", contentType)
	http.ServeFile(w, r, cleanedFp)
}

func renderErrorJSON(w http.ResponseWriter, err error, errcode int) {
	type Response struct {
		Error string `json:"error"`
	}
	resp := Response{
		Error: err.Error(),
	}
	data, _ := json.Marshal(resp)
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(errcode)
	w.Write(data)
}

func renderJSON(w http.ResponseWriter, v interface{}) {
	data, err := json.Marshal(v)
	if err != nil {
		renderErrorJSON(w, err, http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func handler(w http.ResponseWriter, r *http.Request) {
	fp := r.URL.Path[len("/"):]
	if len(fp) == 0 {
		fp = "index.html"
	}

	ext := path.Ext(fp)
	table := map[string]string{
		"js":   "application/javascript",
		"json": "application/json",
		"jpg":  "image/jpeg",
		"jpeg": "image/jpeg",
		"gif":  "image/gif",
		"png":  "image/png",
		"css":  "text/css",
		"htm":  "text/html",
		"html": "text/html",
		"ico":  "image/x-icon",
	}
	contentType := table[ext]
	renderStatic(w, r, fp, contentType)
}

func handlerConsoleOut(w http.ResponseWriter, r *http.Request) {
	entries := []LogEntry{}

	flag := true
	for flag {
		select {
		case x, ok := <-channel:
			if ok {
				entries = append(entries, x)
			} else {
				// channel closed
				flag = false
			}
		default:
			// no value ready
			flag = false
		}
	}

	renderJSON(w, entries)
}

var channel chan LogEntry

func mainLogPush(ch chan LogEntry) {
	reader := bufio.NewReader(os.Stdin)
	for {
		fmt.Print("Enter type(log, assert, warning, error): ")
		text, _ := reader.ReadString('\n')
		text = strings.TrimRight(text, "\n\r")
		switch text {
		case "log":
			ch <- newDummyFormatLog()
		case "assert":
			ch <- newDummyAssertLog()
		case "warning":
			ch <- newDummyWarningLog()
		case "error":
			ch <- newDummyErrorLog()
		default:
			fmt.Printf("unknown log type: [%s]\n", text)
		}
	}
}

func main() {
	channel = make(chan LogEntry)

	go mainLogPush(channel)

	http.HandleFunc("/", handler)
	http.HandleFunc("/console/out", handlerConsoleOut)

	addr := ":" + Port
	fmt.Println("run server on", addr)
	http.ListenAndServe(addr, nil)
}
