export async function loadChatMessages(conversationId, chatWindow) {
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
            messages.forEach(message => appendMessage(messagesList, message, currentUserId));
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
    
    const li = document.createElement("li");

    const isSent = message.senderId === currentUserId;

    // Add appropriate styling
    li.classList.add(isSent ? "sent-message" : "received-message");

    // Add message content
    const messageContent = createMessageContent(message.content);
    li.appendChild(messageContent);

    // Use provided timestamp or generate a new one
    const timestamp = message.timestamp ? new Date(message.timestamp) : new Date();
    const timestampElement = createTimeStamp(timestamp);
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
        const formattedTime = timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const formattedDate = timestamp.toLocaleDateString([], { day: '2-digit', month: '2-digit', year: '2-digit' });

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
        messagesBox.scrollTop = messagesBox.scrollHeight;
    } else {
        console.log("Messages box not found in chat window - chatMessages.js")
    }
}
