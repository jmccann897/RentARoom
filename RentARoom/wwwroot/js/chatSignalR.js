
import { appendMessage, scrollToBottom } from './chatMessages.js';
import { getOrCreateChatWindow } from './chat.js';


export function initialiseChatConnection() {
    const connectionChat = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // SignalR MessageReceived function  - event handler for receiving messages
    connectionChat.on("MessageReceived", function (messagePayload) {
        // Prevent sender from processing their own message
        const senderEmailField = document.querySelector("#senderEmail");
        if (senderEmailField && messagePayload.senderEmail === senderEmailField.value) return;

        // Add the receiver to the conversation group dynamically
        connectionChat.invoke("AddToConversationOnMessage", messagePayload.conversationId)
            .catch(err => console.error("Error adding receiver to conversation:", err));


        const chatWindow = getOrCreateChatWindow(messagePayload.senderEmail, messagePayload.conversationId);

        // Get or create chat window for receiver where the sender is the data-receiver
        if (!chatWindow) {
            console.error("Chat window not found or created.");
            return;
        }

        // Ensure messages-box exists
        const messagesBox = chatWindow.querySelector(".messages-box");
        if (!messagesBox) {
            console.error("Messages box not found in chat window.");
            return;
        }

        // Ensure messagesList exists
        const messagesList = chatWindow.querySelector(".messages-list");
        if (!messagesList) {
            console.error("Messages list not found in chat window.");
            return;
        }

        // Automatically populate the receiver's email field for easy response
        const receiverEmailField = chatWindow.querySelector(".receiver-email");
        if (receiverEmailField) {
            receiverEmailField.value = messagePayload.senderEmail; // Populate receiver's email
        }

        // Get current user ID (email in this case)
        const currentUserEmail = senderEmailField?.value || "";

        // Append message with correct parameters
        appendMessage(messagesList, messagePayload, currentUserEmail);      
    });

    return connectionChat;
}

// Function to join a conversation group
export function joinConversation(connectionChat, conversationId) {
    connectionChat.invoke("JoinConversation", conversationId)
        .catch(err => console.error("Error joining conversation group: ", err));
}

// Function to join multiple conversations
export function joinExistingConversations(connectionChat, conversationIds) {
    if (!Array.isArray(conversationIds)) {
        console.error("Conversation Ids must be an array - chatSignalR.js");
        return;
    }

    conversationIds.forEach(conversationId => {
        connectionChat.invoke("JoinConversation", conversationId)
            .catch(err => console.error("Error joining conversation group: ", err));
    });
}

// Function to send a message
export function sendMessage(connectionChat, senderEmail, receiverEmail, message) {
    return connectionChat.send("SendMessageToReceiver", senderEmail, receiverEmail, message)
        .catch(err => {
            console.error("Error sending message: ", err);
            throw err; // Rethrow the error so the caller can handle it
        });
}

// Function to listen for 'MessageAppended' event and append the message
export function listenForNewMessage(connectionChat, currentUserId) {

    console.log("Setting up listener for 'MessageAppended'...");
    connectionChat.on("MessageAppended", function (message) {
        // Get or create chat window based on conversationId
        const chatWindow = getOrCreateChatWindow(message.senderEmail, message.conversationId);

        if (!chatWindow) {
            console.error("Chat window not found or created.");
            return;
        }

        const messagesList = chatWindow.querySelector(".messages-list");
        if (!messagesList) {
            console.error("Messages list not found in chat window.");
            return;
        }

        // Append the message, passing in the currentUserId to check if it's sent or received
        appendMessage(messagesList, message, currentUserId);

        // Ensure the chat window scrolls to the bottom
        scrollToBottom(chatWindow);
    });
}

    