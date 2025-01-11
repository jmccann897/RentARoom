
document.addEventListener("DOMContentLoaded", function () {
    "use strict";
    // SignalR connection setup
    const connectionChat = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
         .configureLogging(signalR.LogLevel.Information)
        .build();


    const startChatButton = document.getElementById("startChat");

    if (!startChatButton) {
        console.error("Start Chat button not found!");
    } else {
        console.log("Start Chat button found!");
    }

    // Helper function to append messages to chat windows
    function appendMessage(chatWindow, user, message, type) {
        const messagesList = chatWindow.querySelector(".messages-list");
        const li = document.createElement("li");

        // Apply styling based on if sent or received
        if (type === 'sent') {
            li.classList.add("sent-message");
        } else {
            li.classList.add("received-message");
        }

        // Create message content
        const messageContent = document.createElement("div");
        messageContent.classList.add("message-text");
        messageContent.textContent = `${message}`;

        // Create timestamp
        const timestampElement = document.createElement("div");
        timestampElement.classList.add("timestamp");
        timestampElement.textContent = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }); // Formats the time as HH:MM

        // Construct message
        li.appendChild(messageContent);
        li.appendChild(timestampElement);

        messagesList.appendChild(li);

        // Auto-scroll to the bottom of the messages box
        const messagesBox = chatWindow.querySelector(".messages-box");
        messagesBox.scrollTop = messagesBox.scrollHeight;
    }

    // Helper function to find or create a chat window
    function getOrCreateChatWindow(user) {
        let chatWindow = document.querySelector(`.chat-window[data-receiver="${user}"]`);

        if (!chatWindow) {
            // Dynamically create a new chat window
            chatWindow = document.createElement("div");
            chatWindow.classList.add("chat-window", "border", "rounded", "p-3", "mb-3");
            chatWindow.setAttribute("data-receiver", user);
            chatWindow.innerHTML = `
                <h5>Private Chat with ${user}</h5>
                <div class="messages-box border rounded p-3 mb-3">
                    <ul class="messages-list list-unstyled mb-0"></ul>
                </div>
                <textarea class="form-control mb-2 message-input" rows="2" placeholder="Type your message..." ></textarea>
                <button class="btn btn-primary send-private-message">Send</button>
            `;
            document.getElementById("chatWindows").appendChild(chatWindow);


            // Attach event listener for sending private messages
            chatWindow.querySelector(".send-private-message").addEventListener("click", function () {
                const senderEmail = document.getElementById("senderEmail").value;
                const messageInput = chatWindow.querySelector(".message-input");
                const privateMessage = messageInput.value;
                if (privateMessage.trim() === "") return;

                // Send private message
                connectionChat.send("SendMessageToReceiver", senderEmail, user, privateMessage)
                    .catch(err => console.error(err));

                // Append message to the chat window
                appendMessage(chatWindow, senderEmail, privateMessage, "sent");
                messageInput.value = "";
            });
        }
        return chatWindow;
    }

    // SignalR event for receiving messages
    connectionChat.on("MessageReceived", function (user, message, receiver) {
        console.log(`MessageReceived triggered. From: ${user}, Message: ${message}, To: ${receiver}`);
        const senderEmail = document.getElementById("senderEmail").value;


        // Prevent sender from processing their own message
        if (user === senderEmail) return;


        const chatWindow = getOrCreateChatWindow(user);

        // Append the received message to the chat window
        appendMessage(chatWindow, user, message, "received");

        //const li = document.createElement("li");
        //li.classList.add(user === document.getElementById("senderEmail").value ? "sent" : "received");
        //li.textContent = `${user}: ${message}`;
        //document.getElementById("messagesList").appendChild(li);

        //// Scroll to the bottom of the chat box
        //const chatBox = document.querySelector(".chat-box");
        //chatBox.scrollTop = chatBox.scrollHeight;
    });

    // Event listener for "Start Chat" button
    document.getElementById("startChat").addEventListener("click", function () {
        console.log("Start chat triggered");
        const receiver = document.getElementById("receiverEmail").value;

        // Validate receiver email
        if (!receiver || receiver.trim() === "") {
            alert("Please enter a recipient's email to start a chat.");
            return;
        }

        // Open or create a new chat window for the specified receiver
        getOrCreateChatWindow(receiver);
    });

    connectionChat.start().then(function () {
        console.log("SignalR Connected");

        const startChatButton = document.getElementById("startChat");
        if (startChatButton) {
            startChatButton.disabled = false; // Enable the button only if it exists
        } else {
            console.error("Start Chat button not found when enabling.");
        }
    }).catch(function (err) {
        return console.error(err.toString());
    });

});