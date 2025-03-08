
import { initialiseChatConnection, joinConversation, sendMessage, listenForNewMessage } from './chatSignalR.js';
import { loadChatMessages, appendMessage } from './chatMessages.js';
import { debounce } from './site.js';

// Global variables
let connectionChat;
let conversationId;
let senderEmail;
let receiverEmail;
let chatWindow;

// #region Handler for sending messages
function sendMessageHandler(chatWindow, receiverEmail) {
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

    // Send the message through SignalR
    sendMessage(connectionChat, senderEmail, receiverEmail, privateMessage)
        .then(() => {
            console.log("Message sent successfully");

            // Create message object for appending - leave timestamp null so it gets created
            const message = {
                content: privateMessage,
                senderId: senderEmail,
                receiverEmail: receiverEmail,
            };

            // Get the chat window and message list
            const messagesList = chatWindow.querySelector(".messages-list");

            
            // Clear input
            messageInput.value = "";
        }).catch(err => {
            console.error("Error sending the message: ", err);
        });
}
// #endregion

// #region Listeners on page load
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

    // Retrieve stored conversation Ids with new messages
    let storedConvosWithNewMessages = JSON.parse(sessionStorage.getItem("conversationsWithNewMessages")) || [];

    // Highlight conversations that have new messages
    storedConvosWithNewMessages.forEach(convoId => {
        const conversationItem = document.querySelector(`.conversation-item[data-conversation-id="${convoId}"]`);
        if (conversationItem) {
            conversationItem.classList.add('highlight');
        }
    });

    // Associate UI to vars
    const startChatButton = document.getElementById("startChat");
    const receiverEmailField = document.getElementById("receiverEmail");
    const senderEmailField = document.getElementById("senderEmail");
    const chatInfo = document.getElementById("chatInfo");


    // Modify the UI based on whether the data is present
    addPropertyDetailsToChatWindow(conversationId);
    

    // Event listener for conversation click to load messages
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.addEventListener('click', async function () {
            const selectedConversationId = this.dataset.conversationId;
            const selectedReceiverEmail = item.getAttribute('data-recipient-email');

            // Remove the highlight from the clicked conversation
            document.querySelectorAll('.conversation-item.highlight').forEach(item => {
                if (item.dataset.conversationId === selectedConversationId) {
                    item.classList.remove('highlight');
                }
            });
            
            // Update session storage to remove this conversation from the new message list
            storedConvosWithNewMessages = storedConvosWithNewMessages.filter(id => id !== selectedConversationId);
            sessionStorage.setItem("conversationsWithNewMessages", JSON.stringify(storedConvosWithNewMessages));

            // Set conversationId and receiver email
            conversationId = selectedConversationId;
            receiverEmail = selectedReceiverEmail;

            // console.log("Receiver Email from conversation click:", receiverEmail);

            // Initialize the SignalR connection
            connectionChat = initialiseChatConnection();

            // Start the SignalR connection
            connectionChat.start().then(() => {
                console.log("SignalR Connected for chat");

                // Now, start listening for new messages
                listenForNewMessage(connectionChat, currentUserId); // Listen for incoming messages

                // Join the conversation (use the selected conversationId)
                joinConversation(connectionChat, conversationId);

                // Get or create a chat window
                chatWindow = getOrCreateChatWindow(receiverEmail, conversationId);

                // Set the corresponding conversation item as active
                setActiveChatWindow(chatWindow, item);  // Pass the clicked conversation item here

            }).catch(err => {
                console.error("Error starting SignalR chat connection:", err);
            });
        });
    });

    // Event listener for email field input

    // Create debounced version of Check User Email
    const debouncedCheckUserEmail = debounce(checkUserEmail, 500);
    receiverEmailField.addEventListener("input", function () {
        debouncedCheckUserEmail(receiverEmailField.value);
    });
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

                    // Update the conversation list locally if needed
                    userConversationIds.push({ conversationId, receiverEmail });

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

        // Initialize the SignalR connection
        connectionChat = initialiseChatConnection();

        // Start the SignalR connection
        connectionChat.start().then(() => {
            console.log("SignalR Chat Connected");

            // Now, start listening for new messages
            listenForNewMessage(connectionChat, currentUserId);

            // Join the conversation (use the conversationId)
            joinConversation(connectionChat, conversationId);

            // Create or open a chat window for the receiver
            chatWindow = getOrCreateChatWindow(receiverEmail, conversationId);

            // If new chat started from property details then add property details div
            addPropertyDetailsToChatWindow(conversationId);

        }).catch(err => {
            console.error("Error starting SignalR connection:", err);
            startChatButton.disabled = false; // Re-enable on failure
        });
    });
});
// #endregion

// #region Helper functions

// Helper function to check if the email exists
async function checkUserEmail(email) {
    let errorMessageElement = document.getElementById("emailError");
    let startChatButton = document.getElementById("startChat");

    if (!email.trim()) {
        errorMessageElement.textContent = "";
        errorMessageElement.style.visibility = "hidden";
        startChatButton.disabled = true;
        return;
    }

    try {
        const response = await fetch(`/check-user-email?email=${encodeURIComponent(email)}`);

        if (!response.ok) throw new Error("Failed to validate email");

        const result = await response.json();

        if (result.exists) { // Check if the user exists
            errorMessageElement.textContent = ""; // Clear error if email exists
            errorMessageElement.style.visibility = "hidden";
            startChatButton.disabled = false;
        } else {
            errorMessageElement.textContent = "User not found. Please enter a valid email.";
            errorMessageElement.style.visibility = "visible";
            startChatButton.disabled = true;
        }        
    } catch (error) {
        console.error("Error checking user email:", error);
        errorMessageElement.textContent = "An error occurred. Please try again later.";
        errorMessageElement.style.visibility = "visible"; 
        startChatButton.disabled = true;
    }
}


// Helper function to get or create a chat window
export function getOrCreateChatWindow(receiverEmail, conversationId) {
    // Find the existing chat window for the current conversationId
    let chatWindow = document.querySelector(`.chat-window[data-conversation-id="${conversationId}"]`);

    // If the chat window exists and has messages, do not refetch
    if (chatWindow && chatWindow.querySelector('.messages-list').children.length > 0) {
        console.log("Chat window already contains messages, not fetching again.");
    } else {
        if (!chatWindow) {
            // If chat window does not exist, create new one
            chatWindow = createChatWindow(receiverEmail, conversationId);

            // Ensure receiverEmail is correctly set in the data-receiver attribute
            chatWindow.setAttribute('data-receiver', receiverEmail); // Set the receiver email

            document.getElementById("chatWindows").appendChild(chatWindow);
        }
        // Load previous messages for the conversation
        loadChatMessages(conversationId, chatWindow);
    }

    // Attach event listener for sending private messages
    const sendButton = chatWindow.querySelector(".send-private-message");

    sendButton.addEventListener("click", () => sendMessageHandler(chatWindow, receiverEmail));

    addChatHeaderDetails(receiverEmail, chatWindow)
    addPropertyDetailsToChatWindow(conversationId)
    
    // Mark this window as the active one and hide others
    setActiveChatWindow(chatWindow, document.querySelector(`.conversation-item[data-conversation-id="${conversationId}"]`));
    hideOtherChatWindows(chatWindow);

    return chatWindow;
}

// Helper function to create a new chat window
function createChatWindow(receiverEmail, conversationId) {
    const chatWindow = document.createElement("div");
    chatWindow.classList.add("chat-window", "border", "rounded", "p-3", "mb-3");
    chatWindow.setAttribute("data-receiver", receiverEmail);
    chatWindow.setAttribute("data-conversation-id", conversationId);

    // HTML inner structure for chat window
    chatWindow.innerHTML += `
    <div class="messages-box border rounded p-3 mb-3">
        <!-- messages-list will be added dynamically -->
    </div>
    <textarea class="form-control mb-2 message-input" rows="2" placeholder="Type your message..." ></textarea>
    <button class="btn btn-primary send-private-message">Send</button>
    `;

    // Create the message list container
    const messagesList = document.createElement("ul");
    messagesList.classList.add("messages-list", "list-unstyled", "mb-0");
    chatWindow.querySelector(".messages-box").appendChild(messagesList);

    return chatWindow;

}

// Helper function to close all other open chat windows (except the current one)
function hideOtherChatWindows(currentChatWindow) {
    document.querySelectorAll('.chat-window').forEach(otherWindow => {
        if (otherWindow !== currentChatWindow) {
            otherWindow.style.display = 'none'; // Hide or close other windows
        }
    });

    // Ensure the current chat window is displayed
    currentChatWindow.style.display = 'block';
}

export function setActiveChatWindow(currentChatWindow, conversationItem) {
    // Remove the 'active' class from all other windows
    document.querySelectorAll('.chat-window').forEach(window => {
        window.classList.remove('active'); // Remove active class from all windows
    });

    // Add the 'active' class to the selected chat window
    currentChatWindow.classList.add('active');

    // Remove the 'active' class from all conversation list items
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.classList.remove('active');
        if (item !== conversationItem && item.classList.contains('highlight')) {
            item.classList.add('highlight'); // Keep highlight for other items with new messages
        }
    });

    // Add the 'active' class to the corresponding conversation list item
    if (conversationItem) {
        conversationItem.classList.add('active');
        conversationItem.classList.remove('highlight'); // Remove highlight from the active conversation item
    }
}

// Helper function to add property details if chat accessed via details page
function addPropertyDetailsToChatWindow(conversationId) {
    // Access data attributes
    const recipientEmail = chatInfo.getAttribute("data-recipient-email");
    const propertyAddress = chatInfo.getAttribute("data-property-address");
    const propertyCity = chatInfo.getAttribute("data-property-city");
    const propertyPrice = chatInfo.getAttribute("data-property-price");

    if (!recipientEmail || !propertyAddress || !propertyPrice || !propertyCity) {
        console.log("recipientEmail,propertyAddress, propertyPrice , propertyCity not populated, skipping property details.");
        return;
    }
    // First, check if a chat window is open for the current conversation
    let chatWindow = document.querySelector(`.chat-window[data-conversation-id="${conversationId}"]`);

    if (!chatWindow) {
        console.log("No chat window for this conversation, skipping property details.");
        return;  // If no chat window exists, skip adding the property details
    }

    // Ensure that the chat window is the correct conversation
    const currentConversationRecipientEmail = chatWindow.getAttribute("data-receiver");
    if (currentConversationRecipientEmail !== recipientEmail) {
        console.log("The conversation does not match the recipient, skipping property details.");
        return;  // If the conversation recipient doesn't match, skip
    }

    // Check if property details are already appended
    if (chatWindow.querySelector('.property-info')) {
        console.log("Property details already added to the chat window.");
        return;  // If the property details are already added, do nothing
    }

    // Dynamically update the chat window with property info
    const propertyInfoDiv = document.createElement("div");
    propertyInfoDiv.classList.add("property-info");

    // Construct the header
    propertyInfoDiv.innerHTML = `
        <div>
            <p>Address: ${propertyAddress} | City: ${propertyCity} | Price: ${propertyPrice}</p>
        </div>
    `;

    // Prepend the header div to the chat window to ensure it goes above the messages
    chatWindow.insertBefore(propertyInfoDiv, chatWindow.firstChild);
}

function addChatHeaderDetails(receiverEmail, chatWindow) {
    // Check if property details are already appended
    if (chatWindow.querySelector('.chat-header')) {
        console.log("Chat header already added to the chat window.");
        return;  // If the chat header already added, do nothing
    }
    // Fetch the receiver's name based on the email
    fetch(`/User/Notification/GetReceiverName?email=${receiverEmail}`)
        .then(response => {
            console.log(response.status); // Check the status code
            if (!response.ok) {
                throw new Error(`Server responded with status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data && data.name) {
                // Add the receiver's name header at the top of the chat window
                const chatHeader = document.createElement("div");
                chatHeader.classList.add("chat-header", "mb-2");
                chatHeader.innerHTML = `
                    <span class="receiver-name">You are chatting with: ${data.name}</span>
                `;
                // Insert the chat header at the top of the chat window
                chatWindow.prepend(chatHeader);  // Insert as the first child
            }
        })
        .catch(error => console.error('Error fetching receiver info:', error));
}

// #endregion