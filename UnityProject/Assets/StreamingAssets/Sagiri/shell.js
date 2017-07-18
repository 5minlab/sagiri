// caching dom elements
var inputNode = document.querySelector('#input');
var outputNode = document.querySelector('#output');

var commandIndex = -1;
var hash = null;

function scrollBottom() {
  outputNode.scrollTop = output.scrollHeight;
}

function runCommand(command) {
  scrollBottom();

  var url = 'shell/run?command=' + encodeURI(encodeURIComponent(command));
  fetch(url).then(function(res) {
    if(res.ok) {
      updateConsole(function () {
        updateCommand(commandIndex - 1);
      });
    }
  });

  resetInput();
}

function updateConsole(callback) {
  // Check if we are scrolled to the bottom to force scrolling on update
  fetch('shell/out').then(function(res) {
    if(res.ok) {
      res.text().then(function(text) {
        var shouldScroll = (outputNode.scrollHeight - outputNode.scrollTop) == output.clientHeight
        output.value = text;
        if (callback) callback();
        if (shouldScroll) scrollBottom();
      })
    }
  });
}

function resetInput() {
  commandIndex = -1;
  inputNode.value = '';
}

function previousCommand() {
  updateCommand(commandIndex + 1);
}

function nextCommand() {
  updateCommand(commandIndex - 1);
}

function updateCommand(index) {
  // Check if we are at the defualt index and clear the input
  if (index < 0) {
    resetInput();
    return;
  }

  var url = 'shell/commandHistory?index=' + index;
  fetch(url).then(function(res) {
    if(res.ok) {
      res.text().then(function(text) {
        if(text) {
          commandIndex = index;
          inputNode.value = text;
        }
      });
    }
  });
}

function complete(command) {
  var url = 'shell/complete?command=' + command;
  fetch(url).then(function(res) {
    if(res.ok) {
      res.text().then(function(text) {
        if(text) {
          inputNode.value = text;
        }
      });
    }
  });
}

// 최초 1번 데이터 가져오기
updateConsole(null);

inputNode.onkeydown = function (e) {
  if (e.keyCode == 13) { // Enter
    // we don't want a line break in the console
    e.preventDefault();
    runCommand(inputNode.value);
  } else if (e.keyCode == 38) { // Up
    previousCommand();
  } else if (e.keyCode == 40) { // Down
    nextCommand();
  } else if (e.keyCode == 27) { // Escape
    resetInput();
  } else if (e.keyCode == 9) { // Tab
    e.preventDefault();
    complete(inputNode.value);
  }
}
