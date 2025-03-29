
import { initialiseChatConnection, joinConversation, sendMessage, listenForNewMessage, connectionChat } from './chatSignalR.js';
import { loadChatMessages, appendMessage, getOrCreateChatWindow } from './chatMessages.js';
import { debounce } from './site.js';
import { checkUserEmail, filterPropertiesByEmail } from './chatInputhelpers.js';

// Global variables
let conversationId;
let conversationPropertyId;
let senderEmail;
let receiverEmail;
let chatWindow;
let messagePayload;

// Function to set up chatListeners
function setupChatListeners(conncectionChat, currentUserId) {
    // Ensure currentUserId is available
    if (!currentUserId) {
        console.error('Current user ID is not defined!');
        return;
    }

    // The list of conversations passed from the view model
    const userConversationList = window.conversations || [];

    // Associate UI to vars
    const startChatButton = document.getElementById("startChat");
    const receiverEmailField = document.getElementById("receiverEmail");
    const senderEmailField = document.getElementById("senderEmail");
    const chatInfo = document.getElementById("chatInfo");

    // Get propertyId if present from query string
    const urlParams = new URLSearchParams(window.location.search);
    const initialPropertyId = parseInt(urlParams.get('propertyId'));

    // Retrieve stored conversation Ids with new messages
    let storedConvosWithNewMessages = JSON.parse(sessionStorage.getItem("conversationsWithNewMessages")) || [];

    // Highlight conversations that have new messages
    storedConvosWithNewMessages.forEach(convoId => {
        const conversationItem = document.querySelector(`.conversation-item[data-conversation-id="${convoId}"]`);
        if (conversationItem) {
            conversationItem.classList.add('highlight');
        }
    });

    // Event listener for conversation click to load messages
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.addEventListener('click', async function () {
            const selectedConversationId = this.dataset.conversationId;
            const selectedReceiverEmail = this.dataset.recipientEmail;
            const selectedConversationPropertyId = this.dataset.propertyId;

            // Remove the highlight from the clicked conversation
            document.querySelectorAll('.conversation-item.highlight').forEach(item => {
                if (item.dataset.conversationId === selectedConversationId) {
                    item.classList.remove('highlight');
                }
            });

            // Update session storage to remove this conversation from the new message list
            storedConvosWithNewMessages = storedConvosWithNewMessages.filter(id => id !== selectedConversationId);
            sessionStorage.setItem("conversationsWithNewMessages", JSON.stringify(storedConvosWithNewMessages));

            // Set conversationId, receiver email and propertyId
            conversationId = selectedConversationId;
            receiverEmail = selectedReceiverEmail;
            conversationPropertyId = selectedConversationPropertyId

            // Get or create a chat window
            chatWindow = getOrCreateChatWindow(receiverEmail, conversationId, conversationPropertyId);

        });
    });

    // Event listener for email field input

    // Create debounced version of Check User Email
    const debouncedCheckUserEmail = debounce(function (email) {
        checkUserEmail(email);
        // Get propertyId from select element
        const propertySelect = document.getElementById("propertySelect");
        const selectedPropertyId = propertySelect.value ? parseInt(propertySelect.value, 10) : null;

        filterPropertiesByEmail(email, selectedPropertyId);
    }, 500);
    receiverEmailField.addEventListener("input", function () {
        debouncedCheckUserEmail(receiverEmailField.value);
    });

    // Check if receiverEmail is pre-populated on page load
    if (receiverEmailField.value) {
        filterPropertiesByEmail(receiverEmailField.value, initialPropertyId);
    }

    // Event listener for "Start Chat" button
    startChatButton.addEventListener("click", async function () {

        startChatButton.disabled = true; // Disable while processing

        // Validate receiver email
        if (!receiverEmailField.value || receiverEmailField.value.trim() === "") {
            alert("Please enter a recipient's email to start a chat.");
            startChatButton.disabled = false; // Re-enable if validation fails
            return;
        }

        const receiverEmail = receiverEmailField.value;
        const propertySelect = document.getElementById("propertySelect");
        const propertyId = propertySelect.value ? parseInt(propertySelect.value, 10) : null;
        console.log("Start chat clicked, propertyId:", propertyId);

        // Check if the conversation already exists in the list of user's conversations
        const existingConversation = userConversationList.find(convo =>
            convo.receiverEmail === receiverEmail && convo.propertyId === propertyId
        );

        let conversationId;

        if (existingConversation) {
            conversationId = existingConversation.chatConversationId;
            chatWindow = getOrCreateChatWindow(receiverEmail, conversationId, propertyId);
        } else {
            console.log("No existing conversation found, creating a new one.");

            // If no conversation exists, create one on the backend
            try {

                console.log("Sending payload:", { receiverEmail, propertyId });

                const response = await fetch('/user/notification/createconversation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ receiverEmail: receiverEmail, propertyId: propertyId })
                });

                const result = await response.json();
                if (response.ok) {
                    conversationId = result.conversationId;
                    console.log("Conversation Id:", conversationId);

                    // Update the conversation list locally if needed
                    userConversationList.push({ chatConversationId: conversationId, receiverEmail, propertyId });

                    // Create or open a chat window for the receiver
                    chatWindow = getOrCreateChatWindow(receiverEmail, conversationId, propertyId);

                } else {
                    console.error("Error fetching or creating conversation:", result.error);
                    startChatButton.disabled = false; // Re-enable on failure
                    return;
                }
            } catch (error) {
                console.error("Error connecting to backend to create conversation:", error);
                startChatButton.disabled = false; // Re-enable on failure
                return;
            }
        }
    });
}

// #region Handler for sending messages
function sendMessageLogic(connectionChat, chatWindow, receiverEmail, conversationPropertyId) {
    const messageInput = chatWindow.querySelector(".message-input"); // Adjust selector for the message input
    const privateMessage = messageInput.value.trim();

    if (privateMessage === "") {
        console.error("Message is empty. Aborting send - Chat.js sendMessageHandler");
        return;
    }

    if (!receiverEmail) {
        console.error("Receiver email is undefined - Chat.js sendMessageHandler");
        return;
    }

    const senderEmailField = document.getElementById("senderEmail");
    const senderEmail = senderEmailField ? senderEmailField.value : "";
    if (!senderEmail) {
        console.error("Sender email is undefined - Chat.js sendMessageHandler");
        return;
    }

    // Disable the input or send button to prevent multiple sends in quick succession
    const sendButton = chatWindow.querySelector(".send-private-message"); // Adjust selector for the send button
    if (sendButton) {
        sendButton.disabled = true;
    }

    const propertyIdString = conversationPropertyId !== null ? conversationPropertyId.toString() : null;

    // Send the message through SignalR
    sendMessage(connectionChat, senderEmail, receiverEmail, privateMessage, propertyIdString)
        .then(() => {
            console.log("Message sent successfully");

            // Create message object for appending - leave timestamp null so it gets created
            const message = {
                content: privateMessage,
                senderId: senderEmail,
                receiverEmail: receiverEmail
            };

            // Get the chat window and message list
            const messagesList = chatWindow.querySelector(".messages-list");

            // Clear input
            messageInput.value = "";

            // Re-enable the send button after a short delay to prevent immediate re-click
            setTimeout(() => {
                if (sendButton) {
                    sendButton.disabled = false;
                }
            }, 500);
        }).catch(err => {
            console.error("Error sending the message: ", err);
            // Re-enable the send button in case of error
            if (sendButton) {
                sendButton.disabled = false;
            }
        });
}

// Wrap the sendMessageLogic in a debounced function to limit impact of multi-click on send message
//const debouncedSendMessage = debounce((connectionChat, chatWindow, receiverEmail, conversationPropertyId) => {
//    sendMessageLogic(connectionChat, chatWindow, receiverEmail, conversationPropertyId);
//}, 1000);

// Wrapper function that calls the debounced sendMessageLogic function -- need to export for chatmessages.js>getOrCreateChatWindow method
export async function sendMessageHandler(connectionChat, chatWindow, receiverEmail, conversationPropertyId) {
    sendMessageLogic(connectionChat, chatWindow, receiverEmail, conversationPropertyId);
}
// #endregion

// #region Page load flow
document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    // CurrentUserId from sessionStorage, localStorage, or from the global window object
    let currentUserId = window.currentUserId || sessionStorage.getItem('userId') || localStorage.getItem('userId');

    // Initialize the SignalR connection only once
    initialiseChatConnection().then(conn => {
        console.log("SignalR Connected for chat within chat.js");
        //console.log("recieverEmail", receiverEmail);
        //console.log("conversationId", conversationId);
        //console.log("conversationPropertyId", conversationPropertyId);
        // Check if receiverEmail is available, conversationId might be null - create conv, conversationpropertyId might be null - general query.
        setupChatListeners(conn, currentUserId);
        listenForNewMessage(conn, currentUserId); // global
        if (receiverEmail) {
            chatWindow = getOrCreateChatWindow(receiverEmail, conversationId, conversationPropertyId);
        }
        
    }).catch(err => {
        console.error("Error starting SignalR chat connection:", err);
    });
});
// #endregion