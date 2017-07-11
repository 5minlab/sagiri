package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"path"
	"time"
)

const (
	Port = "55056"
)

type LogRow struct {
	Time  time.Time `json:"time"`
	Level string    `json:"lv"`
	Log   string    `json:"log"`
}

func newDummyLogs() []LogRow {
	tf := "2006-01-02 15:04:05 +6"
	t1, _ := time.Parse(tf, "2017-07-11 17:49:49 +9")
	t2, _ := time.Parse(tf, "2017-07-11 17:50:17 +9")
	return []LogRow{
		{
			t1,
			"Log",
			"Start",
		},
		{
			t2,
			"Error",
			"UnityEngine.Debug:LogError(Object, Object)\nMain:Update() (at Assets/Sagiri/Examples/Main.cs:18)\n",
		},
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
	logs := newDummyLogs()
	renderJSON(w, logs)
}

func main() {
	http.HandleFunc("/", handler)
	http.HandleFunc("/console/out", handlerConsoleOut)

	addr := ":" + Port
	fmt.Println("run server on", addr)
	http.ListenAndServe(addr, nil)
}
