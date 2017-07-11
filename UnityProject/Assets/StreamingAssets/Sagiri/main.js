var commandIndex = -1;
var hash = null;

function scrollBottom() {
  $('#output').scrollTop($('#output')[0].scrollHeight);
}

function runCommand(command) {
  scrollBottom();
  $.get("console/run?command=" + encodeURI(encodeURIComponent(command)), function (data, status) {
    updateConsole(function () {
      updateCommand(commandIndex - 1);
    });
  });
  resetInput();
}

function updateConsole(callback) {
  $.get("console/out", function (data, status) {
    // Check if we are scrolled to the bottom to force scrolling on update
    var output = $('#output');
    shouldScroll = (output[0].scrollHeight - output.scrollTop()) == output.innerHeight();
    var fullLog = "";
    for (var i = 0; i < data.length; i++) {
      var row = data[i];
      var time = row.time;
      var lv = row.lv;
      var log = row.log;
      fullLog += log;
      fullLog += "\n";
    }
    output.val(fullLog);
    if (callback) callback();
    if (shouldScroll) scrollBottom();
  });
}

function resetInput() {
  commandIndex = -1;
  $("#input").val("");
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

  $.get("console/commandHistory?index=" + index, function (data, status) {
    if (data) {
      commandIndex = index;
      $("#input").val(String(data));
    }
  });
}

function complete(command) {
  $.get("console/complete?command=" + command, function (data, status) {
    if (data) {
      $("#input").val(String(data));
    }
  });
}

// Poll to update the console output
window.setInterval(function () { updateConsole(null) }, 500);

////////////////////////////////////////
$("#input").keydown(function (e) {
  if (e.keyCode == 13) { // Enter
    // we don't want a line break in the console
    e.preventDefault();
    runCommand($("#input").val());
  } else if (e.keyCode == 38) { // Up
    previousCommand();
  } else if (e.keyCode == 40) { // Down
    nextCommand();
  } else if (e.keyCode == 27) { // Escape
    resetInput();
  } else if (e.keyCode == 9) { // Tab
    e.preventDefault();
    complete($("#input").val());
  }
});
