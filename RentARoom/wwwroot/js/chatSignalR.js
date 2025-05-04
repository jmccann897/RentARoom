
import { appendMessage, scrollToBottom, getOrCreateChatWindow, setActiveChatWindow } from './chatMessages.js';

export let connectionChat = null;
let isMessageReceivedListenerAdded = false;
export function initialiseChatConnection() {
    return new Promise((resolve, reject) => {
        console.log("initialiseChatConnection called");
        // Only create a new connection if one does not already exist or if it's disconnected
        if (!connectionChat || connectionChat.state === signalR.HubConnectionState.Disconnected) {
            connectionChat = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .configureLogging(signalR.LogLevel.Information)
                .build();


            // Handle automatic reconnection
            connectionChat.onclose(async () => {
                console.warn("Chat connection lost. Attempting to reconnect...");
                setTimeout(() => initialiseChatConnection(), 3000);
            });

            // Start the SignalR connection
            connectionChat.start().then(() => {
                console.log("SignalR Connected for chat within chatsignalr.js");

                setTimeout(() => {
                    if (!isMessageReceivedListenerAdded) {
                        console.log("messagereceived listener being added");
                        connectionChat.on("MessageReceived", async function (messagePayload) {
                            try {
                                await handleReceivedMessage(messagePayload, connectionChat);
                            } catch (err) {
                                console.error("Error handling received message:", err);
                            }
                        });
                        isMessageReceivedListenerAdded = true;
                    }
                }, 100); // Add a 100ms delay
                
                // Resolve the promise after the connection is established
                resolve(connectionChat);
            }).catch(err => {
                console.error("Error starting SignalR connection:", err);
                reject(err); // Reject the promise if an error occurs
            });
        } else {
            console.log("SignalR connection is already active or in progress.");
            resolve(connectionChat); // If the connection is already established, resolve immediately
        }
    });
}
    
     
async function handleReceivedMessage(messagePayload, connectionChat) {

    console.log("MessageReceived event triggered:", messagePayload);
    // Prevent sender from processing their own message
    const senderEmailField = document.querySelector("#senderEmail");
    if (senderEmailField && messagePayload.senderEmail === senderEmailField.value) return;

    // Check if the message has already been appended
    const existingMessage = document.querySelector(`.messages-list[data-conversation-id="${messagePayload.conversationId}"] [data-message-id="${messagePayload.chatMessageId}"]`);
    if (existingMessage) return; // Skip if already appended

    // Add the receiver to the conversation group dynamically
    connectionChat.invoke("AddToConversationOnMessage", messagePayload.conversationId)
        .catch(err => console.error("Error adding receiver to conversation:", err));

    const chatWindow = await getOrCreateChatWindow(messagePayload.senderEmail, messagePayload.conversationId, messagePayload.propertyId);

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

    // Get current user Id (email in this case)
    //const currentUserEmail = senderEmailField?.value || "";
    //const currentUserId = messagePayload.senderId;
    const currentUserId = window.currentUserId || sessionStorage.getItem('userId') || localStorage.getItem('userId');

    // Append message with correct parameters
    appendMessage(messagesList, messagePayload, currentUserId);

    // Select conversationItem to set as active
    const conversationItem = document.querySelector(`.conversation-item[data-conversation-id="${messagePayload.conversationId}"][data-property-id="${messagePayload.propertyId}"]`);
    // Update UI to make current chatwindow active and scroll to bottom
    if (conversationItem) {
        setActiveChatWindow(chatWindow, conversationItem);
        scrollToBottom(chatWindow);
    }  
}

// Function to join a conversation group
export function joinConversation(connectionChat, conversationId) {
    console.log(`Joining conversation group: ${conversationId}`);
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
        console.log(`Joining conversation group: ${conversationId}`);
        connectionChat.invoke("JoinConversation", conversationId)
            .catch(err => console.error("Error joining conversation group: ", err));
    });
}

// Function to send a message
export async function sendMessage(connectionChat, senderEmail, receiverEmail, message, propertyId) {
    console.log("Within send message in chatsignalr.js and connectionchat state=", connectionChat.state);
    if (connectionChat.state === signalR.HubConnectionState.Connected) {
        try {
            await connectionChat.invoke("SendMessageToReceiver", senderEmail, receiverEmail, message, propertyId);
            console.log("AFTER calling send() - Message sent to SignalR hub successfully");
        } catch (err) {
            console.error("Error sending message: ", err);
            alert("Message failed to send. Please try again.");
        }
    } else {
        console.error("SignalR connection is not in the 'Connected' state.");
    }


    //try {
    //    await connectionChat.invoke("SendMessageToReceiver", senderEmail, receiverEmail, message, propertyId);
    //    console.log("AFTER calling send() - Message sent to SignalR hub successfully");
    //} catch (err) {
    //    console.error("Error sending message: ", err);
    //}

    //return connectionChat.send("SendMessageToReceiver", senderEmail, receiverEmail, message, propertyId)
    //    .catch(err => {
    //        console.error("Error sending message: ", err);
    //        throw err; // Rethrow the error so the caller can handle it
    //    });
}

// Function to listen for 'MessageAppended' event and append the message
export async function listenForNewMessage(connectionChat, currentUserId) {

    console.log("Setting up listener for 'MessageAppended'...");
    connectionChat.on("MessageAppended", async function (message) {
        console.log("within message appended listener setup");

        try {
            // Get or create chat window based on conversationId, sender and property Id
            const chatWindow = await getOrCreateChatWindow(message.senderEmail, message.conversationId, message.propertyId);

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
        } catch (err) {
            console.error("Error handling message appended:", err);
        }
    });
}



    