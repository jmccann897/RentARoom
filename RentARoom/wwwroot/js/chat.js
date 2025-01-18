
document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    // The list of conversation Ids passed from the view model
    const userConversationIds = window.conversationIds || [];  

    // CurrentUserId from sessionStorage, localStorage, or from the global window object
    let currentUserId = window.currentUserId || sessionStorage.getItem('userId') || localStorage.getItem('userId');

    // Ensure currentUserId is available
    if (!currentUserId) {
        console.error('Current user ID is not defined!');
        return;
    }

    // Ensure userConversationIds is loaded
    console.log("UserConversationIds loaded in JS: ", userConversationIds);

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


    // Join existing conversations based on conversationIds
    userConversationIds.forEach(conversationId => {
        // Logic to join existing conversation groups
        connectionChat.invoke("JoinConversation", conversationId)
            .catch(err => console.error("Error joining conversation group: ", err));
    });

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

        const timestampTime = new Date().toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
        });
        const timestampDate = new Date().toLocaleString([], {
            day: '2-digit',
            month: '2-digit', 
            year: '2-digit'    
        });

        timestampElement.textContent = `${timestampTime}, ${timestampDate}`;

        // Construct message
        li.appendChild(messageContent);
        li.appendChild(timestampElement);

        messagesList.appendChild(li);

        // Auto-scroll to the bottom of the messages box
        const messagesBox = chatWindow.querySelector(".messages-box");
        messagesBox.scrollTop = messagesBox.scrollHeight;
    }
    // Helper function to get previous chat messages in a conversation
    async function loadChatMessages(conversationId, chatWindow) {
        try {
            const response = await fetch(`/user/notification/GetChatMessages?conversationId=${conversationId}`);

            if (response.ok) {
                const messages = await response.json();

                // Render messages in the chat window
                const messagesList = chatWindow.querySelector(".messages-list");
                // Check if messagesList exists
                if (!messagesList) {
                    console.error("Messages list not found in chat window.");
                    return;
                }

                // Render messages in the chat window
                messages.forEach(message => { 
                    const li = document.createElement("li");
                    // Apply styling based on whether the message is sent or received
                    if (message.senderId === currentUserId) {
                        li.classList.add("sent-message");
                    } else {
                        li.classList.add("received-message");
                    }
                    // Add message content
                    const messageContent = document.createElement("div");
                    messageContent.classList.add("message-text");
                    messageContent.textContent = message.content;

                    // Add timestamp as HH:MM, DD:MM:YY
                    const timestampElement = document.createElement("div");
                    timestampElement.classList.add("timestamp");

                    const timestampTime = new Date(message.timestamp).toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                    });
                    const timestampDate = new Date(message.timestamp).toLocaleString([], {
                        day: '2-digit',
                        month: '2-digit',
                        year: '2-digit'
                    });

                    timestampElement.textContent = `${timestampTime}, ${timestampDate}`;

                    // Construct message
                    li.appendChild(messageContent);
                    li.appendChild(timestampElement);

                    messagesList.appendChild(li);
                });

                // Scroll to the bottom of the messages box
                const messagesBox = chatWindow.querySelector(".messages-box");
                if (messagesBox) {
                    messagesBox.scrollTop = messagesBox.scrollHeight;
                } else {
                    console.error("Messages box not found in chat window.");
                }
            } else {
                console.error("Failed to fetch messages:", await response.text());
            }
        } catch (error) {
            console.error("Error loading messages:", error);
        }
    }

    // Helper function to find or create a chat window
    function getOrCreateChatWindow(receiverEmail, conversationId) {
        let chatWindow = document.querySelector(`.chat-window[data-receiver="${receiverEmail}"]`);

        if (!chatWindow) {
            // Dynamically create a new chat window
            chatWindow = document.createElement("div");
            chatWindow.classList.add("chat-window", "border", "rounded", "p-3", "mb-3");
            chatWindow.setAttribute("data-receiver", receiverEmail);
            chatWindow.setAttribute("data-conversation-id", conversationId);
            chatWindow.innerHTML = `
                <div class="messages-box border rounded p-3 mb-3">
                    <!-- messages-list will be added dynamically -->
                </div>
                <textarea class="form-control mb-2 message-input" rows="2" placeholder="Type your message..." ></textarea>
                <button class="btn btn-primary send-private-message">Send</button>
            `;
            document.getElementById("chatWindows").appendChild(chatWindow);

            // Create the messages list
            const messagesList = document.createElement("ul");
            messagesList.classList.add("messages-list", "list-unstyled", "mb-0");
            chatWindow.querySelector(".messages-box").appendChild(messagesList);

            // Load previous messages for the conversation
            loadChatMessages(conversationId, chatWindow);


            // Attach event listener for sending private messages
            chatWindow.querySelector(".send-private-message").addEventListener("click", function () {
                const messageInput = chatWindow.querySelector(".message-input");
                const privateMessage = messageInput.value;
                //const receiverEmail = chatWindow.querySelector(".receiver-email").value;

                console.log("Message:", privateMessage);
                console.log("ReceiverEmail:", receiverEmail);

                if (privateMessage.trim() === "") return;
                if (!privateMessage.trim()) {
                    console.error("Message is empty. Aborting send.");
                    return;
                }

                if (!receiverEmail) {
                    console.error("Receiver email is undefined!");
                    return;
                }

                // Send private message
                connectionChat.send("SendMessageToReceiver", senderEmailField.value, receiverEmail, privateMessage)
                    .then(() => console.log("Message sent successfully!"))
                    .catch(err => console.error("Error sending message: ", err));

                // Append message to the chat window
                appendMessage(chatWindow, senderEmailField.value, privateMessage, "sent");
                messageInput.value = "";
            });
        } else {
            // If the chat window already exists, just load messages
        loadChatMessages(conversationId, chatWindow);
        }
        return chatWindow;
    }

    // SignalR event for receiving messages
    connectionChat.on("MessageReceived", function (messagePayload) {
        console.log("Message Payload:", messagePayload);
        console.log(`MessageReceived triggered. From: ${messagePayload.senderEmail}, Message: ${messagePayload.content}, To: ${messagePayload.receiverEmail}`);

        // Add the receiver to the conversation group dynamically
        connectionChat.invoke("AddToConversationOnMessage", messagePayload.conversationId)
            .catch(err => console.error("Error adding receiver to conversation:", err));

        // Prevent sender from processing their own message
        if (messagePayload.senderEmail === senderEmailField.value) return;

        // Get or create chat window for receiver where the sender is the data-receiver
        const chatWindow = getOrCreateChatWindow(messagePayload.senderEmail, messagePayload.conversationId);

        // Automatically populate the receiver's email field for easy response
        const receiverEmailField = chatWindow.querySelector(".receiver-email");
        if (receiverEmailField) {
            receiverEmailField.value = messagePayload.SenderEmail; // Populate receiver's email
        }

        // Append the received message to the chat window
        appendMessage(chatWindow, messagePayload.senderEmail, messagePayload.content, "received");

    });

    // Event listener for "Start Chat" button
    document.getElementById("startChat").addEventListener("click", async function () {

        // Validate receiver email
        if (!receiverEmailField.value || receiverEmailField.value.trim() === "") {
            alert("Please enter a recipient's email to start a chat.");
            return;
        }

        const receiverEmail = receiverEmailField.value;

        // Check if the conversation already exists in the list of user's conversations
        const existingConversation = userConversationIds.find(convoId => convoId.receiverEmail === receiverEmail);

        let conversationId;

        if (existingConversation) {
            conversationId = existingConversation.conversationId;
        } else {
            console.log("No existing conversation found, creating a new one.");

            // If no conversation exists, create one on the backend
            try {
                const response = await fetch('/user/notification/createconversation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ receiverEmail: receiverEmail })
                });

                const result = await response.json();
                if (response.ok) {
                    conversationId = result.conversationId;
                    console.log("Conversation Id:", conversationId);

                    // Optionally, update the conversation list locally if needed
                    userConversationIds.push({ conversationId, receiverEmail });

                    // You could also update the view to reflect the new conversation in the list
                } else {
                    console.error("Error fetching or creating conversation:", result.error);
                    return;
                }
            } catch (error) {
                console.error("Error connecting to backend to create conversation:", error);
                return;
            }
        }


        // Create or open a chat window for the receiver
        const chatWindow = getOrCreateChatWindow(receiverEmail, conversationId);

        // Join the SignalR group (conversation)
        connectionChat.invoke("JoinConversation", conversationId)
            .catch(err => console.error("Error joining conversation group: ", err));
        
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