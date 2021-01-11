"use strict";

var clientAppInstance = "pa_" + (Math.random() * 500 + 1000);

var connection = new signalR.HubConnectionBuilder().withUrl("/centralHub").build();

//Disable send button until connection is established
document.getElementById("SendButton").disabled = true;

connection.on("ReceiveMessage", function (source, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = source + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("MessagesList").appendChild(li);
});

connection.on("ManagerMessage", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var li = document.createElement("li");
    li.textContent = msg;
    document.getElementById("ManagerList").appendChild(li);
});

connection.start().then(function () {
    document.getElementById("Status").innerText = "Connected";
    document.getElementById("SendButton").disabled = false;

    connection.invoke("AddToGroup", "Manager").catch(function (err) {
        return console.error("Error adding to Manager group: " + err.toString());
    });
}).catch(function (err) {
    return console.error("Error starting SignalR connection: " + err.toString());
});

document.getElementById("SendButton").addEventListener("click", function (event) {
    var deviceId = document.getElementById("DeviceId").value;
    var message = document.getElementById("Message").value;
    connection.invoke("SendMessageToDevice", deviceId, message).catch(function (err) {
        return console.error("Error sending message: " + err.toString());
    });
    event.preventDefault();
});

document.getElementById("HeartbeatButton").addEventListener("click", function (event) {
    connection.invoke("Heartbeat", clientAppInstance).catch(function (err) {
        return console.error("Error sending heartbeat: " + err.toString());
    });
    event.preventDefault();
});
