"use strict"

const connectionChat = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
     .configureLogging(signalR.LogLevel.Information)
    .build();

//Disable the send button until connection is established.
document.getElementById("sendMessage").disabled = true;

connectionChat.on("MessageReceived", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user} - ${message}`;
    
});

document.getElementById("sendMessage").addEventListener("click", function (event) {
    // retrieve values of dom elements
    var sender = document.getElementById("senderEmail").value;
    console.log(sender);
    var chatMessage = document.getElementById("chatMessage").value;
    console.log(chatMessage);
    var receiver = document.getElementById("receiverEmail").value;


    // Check if receiver set, if so only send to them
    if (receiver.length > 0) {
        connectionChat.send("SendMessageToReceiver", sender, receiver, chatMessage)
            .catch(function (err) {
                return console.error(err.toString());
            });
    } else {
        // send message to all users
        connectionChat.send("SendMessageToAll", sender, chatMessage)
            .catch(function (err) {
                return console.error(err.toString());
            });
    }
    event.preventDefault();
})

connectionChat.start().then(function () {
    document.getElementById("sendMessage").disabled = false; // enable send button
}).catch(function (err) {
    return console.error(err.toString());
});