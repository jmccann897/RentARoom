// Wait for the DOM to be fully loaded before executing the script
document.addEventListener("DOMContentLoaded", function () {
    const userId = document.body.getAttribute("data-user-id");

    // Ensure elements exist before using them
    window.notificationIcon = document.getElementById("notificationIcon");
    window.notificationBadge = document.querySelector(".notification-badge");

    // Check if there are new messages before clearing notifications
    let storedConversations = JSON.parse(sessionStorage.getItem("conversationsWithNewMessages")) || [];
    if (storedConversations.length === 0) {
        clearNotification(); // Only clear if there are no new messages
    }

    // If the user is logged in (i.e., userId is not null), establish a SignalR connection
    if (userId) {
        startNotificationConnection(userId);
    } else {
        console.warn("User is not authenticated, skipping SignalR connection.");
    }

    // When the user visits the chat page, hide the notification icon
    if (window.location.pathname.includes("/Chat")) {
        clearNotification();
    }

    // Attach event listener for the notification icon click
    if (notificationIcon) {
        notificationIcon.addEventListener("click", function () {
            if (storedConversations.length > 0) {
                onNotificationClick(storedConversations[0]);
            } else {
                console.error("No new conversation IDs found in sessionStorage.");
            }
        });
    }
});

// Function that establishes a SignalR connection to the NotificationHub
function startNotificationConnection(userId) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();


    // Listen for incoming notifications from the server
    connection.on("ReceiveChatMessageNotification", function (message, conversationId) {
        console.log("New chat message received!");

        const currentPath = window.location.pathname;

        if (currentPath.includes("/Chat")) {
            // User is on chat page, do NOT show notification icon
            clearNotification();
            return;
        }

        // User is not on chat page
        // Retrieve existing conversations from sessionStorage
        let storedConversations = JSON.parse(sessionStorage.getItem("conversationsWithNewMessages")) || [];

        // Add the new conversation ID only if it's not already in the array
        if (!storedConversations.includes(conversationId)) {
            storedConversations.push(conversationId);
            sessionStorage.setItem("conversationsWithNewMessages", JSON.stringify(storedConversations));
        }

        showNotification();
    });

    connection.start()
        .then(() => console.log("Connected to NotificationHub"))
        .catch(err => console.error("Error connecting to NotificationHub:", err));
}

// Handler for the click on the notification icon
function onNotificationClick(conversationId) {

    // Redirect the user to the chat page
    window.location.href = '/User/Notification/Chat';  // Assuming '/chat' is where your chat page is located
}

// #region Helper Functions

// Show notification icon and increment badge count
function showNotification() {

    if (notificationIcon) {
        notificationIcon.classList.remove("d-none");
        notificationIcon.classList.add("show");
        let iconElement = notificationIcon.querySelector("i");
        if (iconElement) {
            iconElement.classList.add("text-primary");
        }
    }
    if (notificationBadge) {
        let currentCount = parseInt(notificationBadge.textContent || "0");
        notificationBadge.textContent = currentCount + 1;
        notificationBadge.classList.remove("d-none");
    }
}

function clearNotification() {
    if (notificationIcon) {
        notificationIcon.classList.remove("show"); 
        notificationIcon.classList.add("d-none");
    }
    if (notificationBadge) {
        notificationBadge.textContent = "";
        notificationBadge.classList.add("d-none");
    }
}
// #endregion



