import { sendMessageHandler } from './chat.js';
import { joinConversation, connectionChat } from './chatSignalR.js';

export async function loadChatMessages(conversationId, chatWindow, conversationPropertyId) {
    // Check if the messages are already loaded in the chat window
    if (chatWindow.querySelector('.messages-list').children.length > 0) {
        console.log("Messages already loaded, skipping fetch.");
        return;
    }

    try {
        // Fetch historical chat messages
        const response = await fetch(`/user/notification/GetChatMessages?conversationId=${conversationId}`);
        if (response.ok) {
            const messages = await response.json();

            // Associate UI elem to variable
            const messagesList = chatWindow.querySelector(".messages-list");

            // Check if messagesList exists
            if (!messagesList) {
                console.error("Messages list not found in chat window.");
                return;
            }

            // Handle empty messages
            if (!messages.length) {
                console.log("No messages found for this conversation.");
                return;
            }

            // Construct chat messages and add them to the window
            messages.forEach(message => {
                console.log("Message Data: ", message); // Log message before appending
                appendMessage(messagesList, message, currentUserId);
            });

            scrollToBottom(chatWindow);
        } else {
            console.error("Failed to fetch messages: ", await response.text());
        }
    } catch (error) {
        console.error("Error loading messages:", error);
    }
}

// Helper function to append a message to the chat window
export function appendMessage(messagesList, message, currentUserId) {

    const existingMessage = messagesList.querySelector(`[data-message-id="${message.chatMessageId}"]`);
    if (existingMessage) return;  // Avoid adding the duplicate message

    const li = document.createElement("li");
    li.setAttribute("data-message-id", message.chatMessageId); // Add message Id to avoid duplicates

    const isSent = message.senderId === currentUserId;

    // Add appropriate styling
    li.classList.add(isSent ? "sent-message" : "received-message");

    // Add message content
    const messageContent = createMessageContent(message.content);
    li.appendChild(messageContent);

    // Handle timestamp as a string first
    let timestampStr = message.timestamp;

    // If it's a string and includes milliseconds, strip them
    if (typeof timestampStr === 'string' && timestampStr.includes('.')) {
        timestampStr = timestampStr.split('.')[0];
    }

    // Ensure 'Z' is appended for UTC if not already
    if (typeof timestampStr === 'string' && !timestampStr.endsWith('Z')) {
        timestampStr += 'Z';
    }

    const localTimestamp = new Date(timestampStr);

    if (isNaN(localTimestamp)) {
        console.error("Invalid timestamp:", message.timestamp);
    }

    const timestampElement = createTimeStamp(localTimestamp);
    li.appendChild(timestampElement);

    messagesList.appendChild(li);
}

// Helper function to create the message content
function createMessageContent(content) {
    const messageContent = document.createElement("div");
    messageContent.classList.add("message-text");
    messageContent.textContent = content;
    return messageContent;
}

// Helper function to create the timestamp element
function createTimeStamp(timestamp) {
    const timestampElement = document.createElement("div");
    timestampElement.classList.add("timestamp");

    // Ensure the timestamp is valid before formatting
    if (timestamp instanceof Date && !isNaN(timestamp.getTime())) {
        const formattedTime = timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        const formattedDate = timestamp.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: '2-digit' });

        timestampElement.textContent = `${formattedTime}, ${formattedDate}`;
    } else {
        console.error("Invalid timestamp:", timestamp);
        timestampElement.textContent = "Invalid Date";
    }

    return timestampElement;
}

 // Helper function to scroll the chat window to the bottom
export function scrollToBottom(chatWindow) {
    const messagesBox = chatWindow.querySelector(".messages-box");
    if (messagesBox) {
        const isScrolledToBottom = messagesBox.scrollHeight === messagesBox.scrollTop + messagesBox.clientHeight;
        if (isScrolledToBottom) {
            messagesBox.scrollTop = messagesBox.scrollHeight;
        }
    } else {
        console.log("Messages box not found in chat window - chatMessages.js")
    }
}


// Helper function to get or create a chat window
export async function getOrCreateChatWindow(receiverEmail, conversationId, conversationPropertyId) {
    // Find the existing chat window for the current conversationId
    let chatWindow = document.querySelector(`.chat-window[data-conversation-id="${conversationId}"][data-property-id="${conversationPropertyId}"]`);

    // If the chat window exists and has messages, do not refetch
    if (chatWindow && chatWindow.querySelector('.messages-list').children.length > 0) {
        console.log("Chat window already contains messages, not fetching again.");
    } else {
        if (!chatWindow) {
            // If chat window does not exist, create new one
            chatWindow = createChatWindow(receiverEmail, conversationId, conversationPropertyId);
            // Ensure the user joins the correct conversation group --> connectionChat defined globally in chat.js
            joinConversation(connectionChat, conversationId);

            // Ensure receiverEmail is correctly set in the data-receiver attribute
            chatWindow.setAttribute('data-receiver', receiverEmail); // Set the receiver email
            // Ensure propertyId is correctly set in the data-receiver attribute
            chatWindow.setAttribute('data-property-id', conversationPropertyId); // Set the propertyId

            document.getElementById("chatWindows").appendChild(chatWindow);
        }
        // Clear previous messages and load new ones
        const messagesList = chatWindow.querySelector('.messages-list');
        if (messagesList) {
            messagesList.innerHTML = ''; // Clear previous messages
            // Load previous messages for the conversation
            await loadChatMessages(conversationId, chatWindow, conversationPropertyId);
        }
    }

    // Get the send button (ensure it's set up once)
    const sendButton = chatWindow.querySelector(".send-private-message");

    // Ensure the send button's event listener is only set up once
    if (!sendButton.hasAttribute('data-listener-bound')) {
        sendButton.addEventListener("click", async () => {
            try {
                // Disable the send button to prevent further clicks while processing
                sendButton.disabled = true;

                console.log("Send message clicked, receiverEmail:", receiverEmail);

                // Call the sendMessageHandler (passing necessary parameters)
                await sendMessageHandler(connectionChat, chatWindow, receiverEmail, conversationPropertyId);

                // Re-enable the send button after a short delay
                setTimeout(() => {
                    sendButton.disabled = false;
                }, 500);  // Adjust delay as necessary (can remove the timeout if not needed)
            } catch (error) {
                console.error("Error sending message:", error);

                // Ensure the send button is re-enabled in case of an error
                sendButton.disabled = false;
            }
        });

        // Mark the listener as bound to prevent it from being added multiple times
        sendButton.setAttribute('data-listener-bound', 'true');
    }

    // Add more chat UI
    addChatHeaderDetails(receiverEmail, chatWindow);
    addPropertyDetailsToChatWindow(conversationId, conversationPropertyId, receiverEmail);

    // Mark this window as the active one and hide others
    setActiveChatWindow(chatWindow, document.querySelector(`.conversation-item[data-conversation-id="${conversationId}"][data-property-id="${conversationPropertyId}"]`));
    hideOtherChatWindows(chatWindow);
    return chatWindow;
}

// Helper function to create a new chat window
 function createChatWindow(receiverEmail, conversationId, conversationPropertyId) {
    const chatWindow = document.createElement("div");
    chatWindow.classList.add("chat-window", "border", "rounded", "p-3", "mb-3");
    chatWindow.setAttribute("data-receiver", receiverEmail);
    chatWindow.setAttribute("data-conversation-id", conversationId);
    chatWindow.setAttribute("data-property-id", conversationPropertyId);

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
 function addPropertyDetailsToChatWindow(conversationId, conversationPropertyId, recipientEmail) {

    // First, check if a chat window is open for the current conversation
    let chatWindow = document.querySelector(`.chat-window[data-conversation-id="${conversationId}"][data-property-id="${conversationPropertyId}`);

    if (!chatWindow) {
        console.log("No chat window for this conversation, skipping property details.");
        return;
    }

    // Ensure that the chat window is the correct conversation
    const currentConversationRecipientEmail = chatWindow.getAttribute("data-receiver");
    if (currentConversationRecipientEmail !== recipientEmail) {
        console.log("The conversation does not match the recipient, skipping property details.");
        return;
    }

    // Check if property details are already appended
    if (chatWindow.querySelector('.property-info')) {
        console.log("Property details already added to the chat window.");
        return;
    }
    // Check if conversationPropertyId is null
    if (!conversationPropertyId) {
        console.log("conversationPropertyId is null, skipping fetch.");
        // Add a general message to the chat window, or handle as needed
        const propertyInfoDiv = document.createElement("div");
        propertyInfoDiv.classList.add("property-info");
        propertyInfoDiv.innerHTML = `<div><p>General query</p></div>`;
        chatWindow.prepend(propertyInfoDiv);
        return;
    }

    // Fetch property details based on propertyId
    fetch(`/${conversationPropertyId}`)
        .then(response => {
            if (!response.ok) {
                console.log("Within GetPropertyDetailsForChatEndpoint Call within chat.js");
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(conversationPropertyId => {
            // Dynamically update the chat window with property info
            const propertyInfoDiv = document.createElement("div");
            propertyInfoDiv.classList.add("property-info");

            // Construct the header
            let imageHtml = '';
            if (conversationPropertyId.imageUrl) {
                imageHtml = `<img src="${conversationPropertyId.imageUrl}" alt="Property Image" style="max-width: 100px; max-height: 100px;">`;
            }

            propertyInfoDiv.innerHTML = `
                <p>Address: ${conversationPropertyId.address} | City: ${conversationPropertyId.city} | Price: ${conversationPropertyId.price}</p>
                ${imageHtml}
            `;

            // Find the chat header element
            const chatHeader = chatWindow.querySelector('.chat-header');

            if (chatHeader) {
                // Insert propertyInfoDiv after chatHeader
                chatWindow.insertBefore(propertyInfoDiv, chatHeader.nextSibling);
            } else {
                // If chatHeader doesn't exist, prepend propertyInfoDiv
                chatWindow.prepend(propertyInfoDiv);
            }
        })
        .catch(error => {
            console.error("Failed to fetch property details within chat.js: ", error);
        });
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
            //console.log(response.status); // Check the status code
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
export function formatConversationTimestamps() {
    document.querySelectorAll('.timestamp[data-utc-timestamp]').forEach(elem => {
        const raw = elem.getAttribute('data-utc-timestamp');
        if (!raw) return;

        const timestamp = new Date(raw);
        if (isNaN(timestamp)) {
            elem.textContent = "Invalid Date";
            return;
        }

        const time = timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
        const date = timestamp.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: '2-digit' });

        elem.textContent = `${time}, ${date}`;
    });
}