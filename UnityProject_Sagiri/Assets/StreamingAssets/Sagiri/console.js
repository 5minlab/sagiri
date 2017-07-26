// Variables
var mainPanel = document.getElementById('mainPanel');
var searchBox = document.getElementById('search');
var checkEvent = new CustomEvent('change', { checked: this.checked });
var theSearchableElements = [];
// TODO 100 is too small
var maxToShowByDefault = 100;
var largeFile = false;
var title = document.getElementById('title');
var startTime = document.getElementById('startTime');
var endTime = document.getElementById('endTime');
var largeNotice = document.getElementById('large');
var lastLogID = document.getElementById('last-log-id');

var counts = {
  assert: 0,
  error: 0,
  exception: 0,
  warning: 0,
  system: 0,
  log: 0,
  ai: 0,
  audio: 0,
  content: 0,
  logic: 0,
  gui: 0,
  input: 0,
  network: 0,
  physics: 0
};

var countElements = {
  assert: undefined,
  error: undefined,
  exception: undefined,
  warning: undefined,
  system: undefined,
  log: undefined,
  ai: undefined,
  audio: undefined,
  content: undefined,
  logic: undefined,
  gui: undefined,
  input: undefined,
  network: undefined,
  physics: undefined
};

var checklistElements = {
  assert: undefined,
  error: undefined,
  exception: undefined,
  warning: undefined,
  system: undefined,
  log: undefined,
  ai: undefined,
  audio: undefined,
  content: undefined,
  logic: undefined,
  gui: undefined,
  input: undefined,
  network: undefined,
  physics: undefined
};

var entryElements = {
  assert: [],
  error: [],
  exception: [],
  warning: [],
  system: [],
  log: [],
  ai: [],
  audio: [],
  content: [],
  logic: [],
  gui: [],
  input: [],
  network: [],
  physics: []
};

var checkboxElements = {
  all: document.getElementById('allCheckbox'),
  assert: document.getElementById('assertCheckbox'),
  error: document.getElementById('errorCheckbox'),
  exception: document.getElementById('exceptionCheckbox'),
  warning: document.getElementById('warningCheckbox'),
  system: document.getElementById('systemCheckbox'),
  log: document.getElementById('logCheckbox'),
  ai: document.getElementById('aiCheckbox'),
  audio: document.getElementById('audioCheckbox'),
  content: document.getElementById('contentCheckbox'),
  logic: document.getElementById('logicCheckbox'),
  gui: document.getElementById('guiCheckbox'),
  input: document.getElementById('inputCheckbox'),
  network: document.getElementById('networkCheckbox'),
  physics: document.getElementById('physicsCheckbox')
};

// Functions
// Initialize page
function init() {
  console.time('init()');
  // Get elements
  // Get count elements
  countElements["assert"] = document.getElementsByClassName("count assert")[0];
  countElements["error"] = document.getElementsByClassName("count error")[0];
  countElements["exception"] = document.getElementsByClassName("count exception")[0];
  countElements["warning"] = document.getElementsByClassName("count warning")[0];
  countElements["system"] = document.getElementsByClassName("count system")[0];
  countElements["log"] = document.getElementsByClassName("count log")[0];
  countElements["ai"] = document.getElementsByClassName("count ai")[0];
  countElements["audio"] = document.getElementsByClassName("count audio")[0];
  countElements["content"] = document.getElementsByClassName("count content")[0];
  countElements["logic"] = document.getElementsByClassName("count logic")[0];
  countElements["gui"] = document.getElementsByClassName("count gui")[0];
  countElements["input"] = document.getElementsByClassName("count input")[0];
  countElements["network"] = document.getElementsByClassName("count network")[0];
  countElements["physics"] = document.getElementsByClassName("count physics")[0];
  // Get checklist
  checklistElements["assert"] = document.getElementById("li-assert");
  checklistElements["error"] = document.getElementById("li-error");
  checklistElements["exception"] = document.getElementById("li-exception");
  checklistElements["warning"] = document.getElementById("li-warning");
  checklistElements["system"] = document.getElementById("li-system");
  checklistElements["log"] = document.getElementById("li-log");
  checklistElements["ai"] = document.getElementById("li-ai");
  checklistElements["audio"] = document.getElementById("li-audio");
  checklistElements["content"] = document.getElementById("li-content");
  checklistElements["logic"] = document.getElementById("li-logic");
  checklistElements["gui"] = document.getElementById("li-gui");
  checklistElements["input"] = document.getElementById("li-input");
  checklistElements["network"] = document.getElementById("li-network");
  checklistElements["physics"] = document.getElementById("li-physics");
  console.time('init()');
}//init

// Load a file
function handleFileSelect(evt) {
  console.time("handleFileselect()");
  var file = this.files[0];
  title.textContent = file.name.substr(0, file.name.indexOf('.'));
  var timesnip = file.name.replace(title.textContent + ".", '').substr(title.textContent.indexOf('.') + 1).replace('.json', '');
  startTime.textContent = new Date(timesnip.substr(0, 10).replace(/\./g, '-') + "T" + timesnip.substr(11, 2) + ":" + timesnip.substr(14, 2) + ":" + timesnip.substr(17, 2)).toLocaleString();
  endTime.textContent = file.lastModifiedDate.toLocaleString();
  if (file) {
    var reader = new FileReader();
    reader.onload = function (f) {
      populateLogs(JSON.parse(f.target.result));
      updateCounts();
      updateEntriesList();
      setCheckboxesOn();
    }
    reader.readAsText(file);
  }
  console.timeEnd("handleFileselect()");
}//handleFileselect

// Parse a file, clear existing logs, add new to document
function populateLogs(jsonFile) {
  console.time("populateLogs()");
  mainPanel.innerHTML = "";
  // Reset our counts
  Object.keys(counts).forEach(function (k) {
    counts[k] = 0;
  });
  // Loop through each log entry and add some html
  var entry, inner, time, type;
  jsonFile.forEach(function (log) {
    type = log.t.toLowerCase();
    counts[type] = ++counts[type];
    entry = document.createElement('div');
    entry.classList.add('entry', type);
    entry.classList.add('hidden');
    time = log.tm ? new Date(log.tm.substr(0, 10).replace(/\./g, '-') + "T" + log.tm.substr(11, 2) + ":" + log.tm.substr(14, 2) + ":" + log.tm.substr(17, 2)).toLocaleString() : "";
    inner = '<span class="timestamp">' + time + '</span>' +
      '<span class="message"><svg class="icon"><use xlink:href="#' + type + 'Icon" /></svg><button class="stackBtn" onclick="openCloseStack(this)">Stack</button>' + log.l + '</span>' +
      '<span class="stack hidden">' + log.s.replace(/\n/gi, "<br>") + '</span>';
    entry.innerHTML = inner;
    mainPanel.appendChild(entry);
  });
  console.timeEnd("populateLogs()");
}//populateLogs


function prependLog(log) {
  console.time("prependLog()");

  var type = log.t.toLowerCase();

  if(counts[type] == undefined) {
    counts[type] = 1;
  } else {
    counts[type] = ++counts[type];
  }

  // 로그의 갯수가 너무 많아진거같으면 옛날 로그를 지운다
  while(counts[type] > maxToShowByDefault) {
    var childcount = mainPanel.children.length;
    var found = null;
    for(var i = childcount-1 ; i >= 0 ; i--) {
      var el = mainPanel.children[i];
      if(el.classList.contains(type)) {
        found = el;
        break;
      }
    }
    if(found) {
      mainPanel.removeChild(found);
      counts[type]--;
    }
  }

  var entry = document.createElement('div');
  entry.classList.add('entry', type);
  entry.classList.add('hidden');
  var time = log.tm ? new Date(log.tm.substr(0, 10).replace(/\./g, '-') + "T" + log.tm.substr(11, 2) + ":" + log.tm.substr(14, 2) + ":" + log.tm.substr(17, 2)).toLocaleString() : "";

  var inner = '<span class="timestamp">' + time + '</span>' +
      '<span class="message"><svg class="icon"><use xlink:href="#' + type + 'Icon" /></svg><button class="stackBtn" onclick="openCloseStack(this)">Stack</button>' + log.l + '</span>' +
      '<span class="stack hidden">' + log.s.replace(/\n/gi, "<br>") + '</span>';
  entry.innerHTML = inner;
  mainPanel.insertBefore(entry, mainPanel.firstChild);

  console.timeEnd("prependLog()");
}

// Update counts, hide empty lists
function updateCounts() {
  console.time("updateCounts()");

  // Update counts
  countElements["assert"].textContent = " (" + counts.assert + ")";
  countElements["error"].textContent = " (" + counts.error + ")";
  countElements["exception"].textContent = " (" + counts.exception + ")";
  countElements["warning"].textContent = " (" + counts.warning + ")";
  countElements["system"].textContent = " (" + counts.system + ")";
  countElements["log"].textContent = " (" + counts.log + ")";
  countElements["ai"].textContent = " (" + counts.ai + ")";
  countElements["audio"].textContent = " (" + counts.audio + ")";
  countElements["content"].textContent = " (" + counts.content + ")";
  countElements["logic"].textContent = " (" + counts.logic + ")";
  countElements["gui"].textContent = " (" + counts.gui + ")";
  countElements["input"].textContent = " (" + counts.input + ")";
  countElements["network"].textContent = " (" + counts.network + ")";
  countElements["physics"].textContent = " (" + counts.physics + ")";

  // Hide no counts
  if (!counts["assert"]) { checklistElements["assert"].classList.add('hidden'); }
  else { checklistElements["assert"].classList.remove('hidden'); }

  if (!counts["error"]) { checklistElements["error"].classList.add('hidden'); }
  else { checklistElements["error"].classList.remove('hidden'); }

  if (!counts["exception"]) { checklistElements["exception"].classList.add('hidden'); }
  else { checklistElements["exception"].classList.remove('hidden'); }

  if (!counts["warning"]) { checklistElements["warning"].classList.add('hidden'); }
  else { checklistElements["warning"].classList.remove('hidden'); }

  if (!counts["system"]) { checklistElements["system"].classList.add('hidden'); }
  else { checklistElements["system"].classList.remove('hidden'); }

  if (!counts["ai"]) { checklistElements["ai"].classList.add('hidden'); }
  else { checklistElements["ai"].classList.remove('hidden'); }

  if (!counts["audio"]) { checklistElements["audio"].classList.add('hidden'); }
  else { checklistElements["audio"].classList.remove('hidden'); }

  if (!counts["content"]) { checklistElements["content"].classList.add('hidden'); }
  else { checklistElements["content"].classList.remove('hidden'); }

  if (!counts["logic"]) { checklistElements["logic"].classList.add('hidden'); }
  else { checklistElements["logic"].classList.remove('hidden'); }

  if (!counts["gui"]) { checklistElements["gui"].classList.add('hidden'); }
  else { checklistElements["gui"].classList.remove('hidden'); }

  if (!counts["input"]) { checklistElements["input"].classList.add('hidden'); }
  else { checklistElements["input"].classList.remove('hidden'); }

  if (!counts["network"]) { checklistElements["network"].classList.add('hidden'); }
  else { checklistElements["network"].classList.remove('hidden'); }

  if (!counts["physics"]) { checklistElements["physics"].classList.add('hidden'); }
  else { checklistElements["physics"].classList.remove('hidden'); }

  console.timeEnd("updateCounts()");
}//updateCounts

// Update list entries
function updateEntriesList() {
  console.time("updateEntriesList()");
  entryElements["assert"] = document.getElementsByClassName("entry assert");
  entryElements["error"] = document.getElementsByClassName("entry error");
  entryElements["exception"] = document.getElementsByClassName("entry exception");
  entryElements["warning"] = document.getElementsByClassName("entry warning");
  entryElements["system"] = document.getElementsByClassName("entry system");
  entryElements["log"] = document.getElementsByClassName("entry log");
  entryElements["ai"] = document.getElementsByClassName("entry ai");
  entryElements["audio"] = document.getElementsByClassName("entry audio");
  entryElements["content"] = document.getElementsByClassName("entry content");
  entryElements["logic"] = document.getElementsByClassName("entry logic");
  entryElements["gui"] = document.getElementsByClassName("entry gui");
  entryElements["input"] = document.getElementsByClassName("entry input");
  entryElements["network"] = document.getElementsByClassName("entry network");
  entryElements["physics"] = document.getElementsByClassName("entry physics");
  console.timeEnd("updateEntriesList()");
}//updateEntriesList

// turn off big lists
function setCheckboxesOn() {
  var any = false, log = false;
  Object.keys(counts).forEach(function (key) {
    if (counts[key] > maxToShowByDefault) {
      any = true;
      if (key === "log") {
        log = true;
      }
      checkboxElements[key].checked = false;
      if (!(log && (key === "ai" || key === "audio" || key === "content" || key === "logic" || key === "gui" || key === "input" || key === "network" || key === "physics"))) {
        checkboxElements[key].dispatchEvent(checkEvent);
      }
    }
    else {
      checkboxElements[key].checked = true;
      checkboxElements[key].dispatchEvent(checkEvent);
    }
  });
  if (any) {
    checkboxElements["all"].checked = false;
    largeNotice.classList.remove('hidden');
    // checkboxElements["all"].dispatchEvent(checkEvent);
  }
  else if (log) {
    checkboxElements["log"].dispatchEvent(checkEvent);
    largeNotice.classList.remove('hidden');
  }
  else {
    checkboxElements["all"].checked = true;
    checkboxElements["all"].dispatchEvent(checkEvent);
    largeNotice.classList.add('hidden');
  }
}//setCheckboxesOn

// Helper attached to buttons to open/close stack for current entry
function openCloseStack(element) {
  console.time("openCloseStack()");
  var stack = element.parentElement.parentElement.getElementsByClassName('stack');
  stack[0].classList.toggle('hidden');
  console.timeEnd("openCloseStack()");
}//openCloseStack

// Helper to show/hide the log entry from checkbox. If a search is present rerun it
function showOrHideLogEntry(type, checked) {
  console.time("showOrHideLogEntry(" + type + ")");
  for (var i = 0; i < entryElements[type].length; i++) {
    if (checked) {
      entryElements[type][i].classList.remove("hidden");
      theSearchableElements.push(entryElements[type][i]);
    }
    else {
      entryElements[type][i].classList.add("hidden");
      var index = theSearchableElements.indexOf(entryElements[type][i]);
      theSearchableElements.splice(index, 1);
    }
  }
  // If there is a search query rerun it
  if (searchBox.value) {
    var event = new CustomEvent('input', {});
    searchBox.dispatchEvent(event);
  }
  console.timeEnd("showOrHideLogEntry(" + type + ")");
}//showOrHideLogEntry

// Helper to handle search query
function search(evt) {
  console.time('search()');
  var searchQuery = this.value.toLowerCase();
  var messages, stack, type, time, inMessage, inStack, inType, inTime;
  theSearchableElements.forEach(function (element) {
    // Reset
    inType = false;
    inTime = false;
    type = element.classList.toString().replace("entry", "").replace("hidden", "").replace(" ", "");
    time = element.getElementsByClassName('timestamp')[0].textContent.toLowerCase();
    if (time.indexOf(searchQuery) > -1) { inTime = true; }
    if (type.indexOf(searchQuery) > -1) { inType = true; }
    // Loop if not already a match
    if (!inType && !inTime) {
      inMessage = false;
      inStack = false;
      messages = element.getElementsByClassName('message');
      stack = element.getElementsByClassName('stack');
      // Loop through messages
      for (var j = 0; j < messages.length; j++) {
        if (messages[j].nodeType == 1) {
          if (messages[j].textContent.toLowerCase().indexOf(searchQuery) > -1) { inMessage = true; }
        }
      }//for each message
      // Loop through stack
      for (var j = 0; j < stack.length; j++) {
        if (stack[j].nodeType == 1) {
          if (stack[j].textContent.toLowerCase().indexOf(searchQuery) > -1) { inStack = true; }
        }
      }//for each stack
    }
    // Add remove classes if found
    if (inMessage || inStack || inType || inTime) { element.classList.remove("hidden"); }
    else { element.classList.add("hidden"); }
  });// for each theSearchableElements
  console.timeEnd('search()');
}//search

// DOM Content Loaded
document.addEventListener('DOMContentLoaded', function () {
  console.time("DOMContentLoaded");
  var event = new CustomEvent('change', { checked: this.checked });
  allCheckbox.dispatchEvent(event);
  init();
  console.timeEnd("DOMContentLoaded");
}, false); //DOMContentLoaded

// Listeners
// File loading
document.getElementById('file').addEventListener('change', handleFileSelect, false);

// Search logic
searchBox.addEventListener('input', search, false);

// Listener for the select all checkbox
allCheckbox.addEventListener('change', function (e) {
  assertCheckbox.checked = (this.checked ? true : false);
  errorCheckbox.checked = (this.checked ? true : false);
  exceptionCheckbox.checked = (this.checked ? true : false);
  warningCheckbox.checked = (this.checked ? true : false);
  systemCheckbox.checked = (this.checked ? true : false);
  logCheckbox.checked = (this.checked ? true : false);

  var event = new CustomEvent('change', { checked: this.checked });
  assertCheckbox.dispatchEvent(event);
  errorCheckbox.dispatchEvent(event);
  exceptionCheckbox.dispatchEvent(event);
  warningCheckbox.dispatchEvent(event);
  systemCheckbox.dispatchEvent(event);
  logCheckbox.dispatchEvent(event);
});//allCheckbox change

// Listener for the log checkbox
logCheckbox.addEventListener('change', function (e) {

  showOrHideLogEntry("log", this.checked);

  aiCheckbox.checked = (this.checked ? true : false);
  audioCheckbox.checked = (this.checked ? true : false);
  contentCheckbox.checked = (this.checked ? true : false);
  logicCheckbox.checked = (this.checked ? true : false);
  guiCheckbox.checked = (this.checked ? true : false);
  inputCheckbox.checked = (this.checked ? true : false);
  networkCheckbox.checked = (this.checked ? true : false);
  physicsCheckbox.checked = (this.checked ? true : false);

  var event = new CustomEvent('change', { checked: this.checked });
  aiCheckbox.dispatchEvent(event);
  audioCheckbox.dispatchEvent(event);
  contentCheckbox.dispatchEvent(event);
  logicCheckbox.dispatchEvent(event);
  guiCheckbox.dispatchEvent(event);
  inputCheckbox.dispatchEvent(event);
  networkCheckbox.dispatchEvent(event);
  physicsCheckbox.dispatchEvent(event);
});//logCheckbox change

// Listeners for individual checkboxes
assertCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("assert", e.checked); }
  else { showOrHideLogEntry("assert", this.checked); }
});

errorCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("error", e.checked); }
  else { showOrHideLogEntry("error", this.checked); }
});

exceptionCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("exception", e.checked); }
  else { showOrHideLogEntry("exception", this.checked); }
});

warningCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("warning", e.checked); }
  else { showOrHideLogEntry("warning", this.checked); }
});

systemCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("system", e.checked); }
  else { showOrHideLogEntry("system", this.checked); }
});

aiCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("ai", e.checked); }
  else { showOrHideLogEntry("ai", this.checked); }
});

audioCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("audio", e.checked); }
  else { showOrHideLogEntry("audio", this.checked); }
});

contentCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("content", e.checked); }
  else { showOrHideLogEntry("content", this.checked); }
});

logicCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("logic", e.checked); }
  else { showOrHideLogEntry("logic", this.checked); }
});

guiCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("gui", e.checked); }
  else { showOrHideLogEntry("gui", this.checked); }
});

inputCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("input", e.checked); }
  else { showOrHideLogEntry("input", this.checked); }
});

networkCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("network", e.checked); }
  else { showOrHideLogEntry("network", this.checked); }
});

physicsCheckbox.addEventListener('change', function (e) {
  if (e.checked) { showOrHideLogEntry("physics", e.checked); }
  else { showOrHideLogEntry("physics", this.checked); }
});

// reset
document.querySelector('#reset-log-entries').onclick = function() {
  populateLogs([]);
  updateCounts();
  updateEntriesList();
  setCheckboxesOn();

  lastUniqueLogId = 0;
  lastLogID.innerHTML = lastUniqueLogId;
}

var lastUniqueLogId = 0;

function handleReceivedLogs(data) {
  console.time("handleReceivedLogs load()");

  // 로그를 역순으로 추가
  // 최신 로그가 위에 있는게 읽기 쉬울거같아서?
  for(var i = 0 ; i < data.length ; i++) {
    var log = data[i];
    if(log.id > lastUniqueLogId) {
      prependLog(log);
    }
  }

  title.textContent = 'sagiri';
  updateCounts();
  updateEntriesList();
  setCheckboxesOn();

  // 마지막으로 받은 log id 갱신
  for(var i = 0 ; i < data.length ; i++) {
    var log = data[i];
    if(log.id > lastUniqueLogId) {
      lastUniqueLogId = log.id;
    }
  }
  lastLogID.innerHTML = lastUniqueLogId;

  console.timeEnd("handleReceivedLogs load()");
}

function updateConsole(callback) {
  var path = '/console/fetch';
  var qs = 'last=' + lastUniqueLogId;
  var url = path + '?' + qs;

  fetch(url).then(function(res) {
    if(res.ok) {
      res.json().then(function(data) {
        if(data.length > 0) {
          handleReceivedLogs(data);
        }
      });
    } else {
      console.log("/console/out response error: ", res.status);
    }
  }).then(function() {
    window.setTimeout(function() { updateConsole(null); }, 500);
  }).catch(function(e) {
    //console.log("/console/out fetch failed:", e);
    window.setTimeout(function() { updateConsole(null); }, 500);
  })
}

window.setTimeout(function() { updateConsole(null); }, 500);
