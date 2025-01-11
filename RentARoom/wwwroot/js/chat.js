
document.addEventListener("DOMContentLoaded", function () {
    "use strict";
    // SignalR connection setup
    const connectionChat = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
         .configureLogging(signalR.LogLevel.Information)
        .build();


    const startChatButton = document.getElementById("startChat");
    const receiverEmailField = document.getElementById("receiverEmail");
    const senderEmailField = document.getElementById("senderEmail");
    const chatInfo = document.getElementById("chatInfo");

    // Access data attributes
    const recipientEmail = chatInfo.getAttribute("data-recipient-email");
    const propertyAddress = chatInfo.getAttribute("data-property-address");
    const propertyCity = chatInfo.getAttribute("data-property-city");
    const propertyPrice = chatInfo.getAttribute("data-property-price");

    // Modify the UI based on whether the data is present
    if (recipientEmail && propertyAddress && propertyPrice && propertyCity) {
        // Dynamically update the chat window with property info
        const propertyInfoDiv = document.createElement("div");
        propertyInfoDiv.classList.add("chat-header");

        // Construct the header
        propertyInfoDiv.innerHTML = `
        <div class="property-info-left">
            <p><strong>Address:</strong> ${propertyAddress}</p>
            <p><strong>City:</strong> ${propertyCity}</p>
            <p><strong>Price:</strong> ${propertyPrice}</p>
        </div>
        <div class="receiver-info-right">
            <p><strong>Receiver:</strong> ${recipientEmail}</p>
        </div>
    `;

        // Append the header div to the chat window
        document.getElementById("chatWindows").appendChild(propertyInfoDiv);
    }

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
                <div class="messages-box border rounded p-3 mb-3">
                    <ul class="messages-list list-unstyled mb-0"></ul>
                </div>
                <textarea class="form-control mb-2 message-input" rows="2" placeholder="Type your message..." ></textarea>
                <button class="btn btn-primary send-private-message">Send</button>
            `;
            document.getElementById("chatWindows").appendChild(chatWindow);


            // Attach event listener for sending private messages
            chatWindow.querySelector(".send-private-message").addEventListener("click", function () {
                const messageInput = chatWindow.querySelector(".message-input");
                const privateMessage = messageInput.value;
                if (privateMessage.trim() === "") return;

                // Send private message
                connectionChat.send("SendMessageToReceiver", senderEmailField.value, user, privateMessage)
                    .catch(err => console.error(err));

                // Append message to the chat window
                appendMessage(chatWindow, senderEmailField.value, privateMessage, "sent");
                messageInput.value = "";
            });
        }
        return chatWindow;
    }

    // SignalR event for receiving messages
    connectionChat.on("MessageReceived", function (user, message, receiver) {
        console.log(`MessageReceived triggered. From: ${user}, Message: ${message}, To: ${receiver}`);


        // Prevent sender from processing their own message
        if (user === senderEmailField.value) return;


        const chatWindow = getOrCreateChatWindow(user);

        // Append the received message to the chat window
        appendMessage(chatWindow, user, message, "received");

    });

    // Event listener for "Start Chat" button
    document.getElementById("startChat").addEventListener("click", function () {

        // Validate receiver email
        if (!receiverEmailField.value || receiverEmailField.value.trim() === "") {
            alert("Please enter a recipient's email to start a chat.");
            return;
        }

        // Open or create a new chat window for the specified receiver
        getOrCreateChatWindow(receiverEmailField.value);
    });

    // Check if the receiver email is pre-populated
    if (receiverEmailField.value) {
        console.log("Receiver email is pre-populated: ", receiverEmailField.value);
        startChatButton.disabled = false;
        startChatButton.click();
    }

    connectionChat.start().then(function () {
        console.log("SignalR Connected");

        if (startChatButton) {
            startChatButton.disabled = false; // Enable the button only if it exists
        } else {
            console.error("Start Chat button not found when enabling.");
        }
    }).catch(function (err) {
        return console.error(err.toString());
    });

});