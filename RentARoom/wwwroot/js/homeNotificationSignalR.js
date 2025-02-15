// Wait for the DOM to be fully loaded before executing the script
document.addEventListener("DOMContentLoaded", function () {
    const userId = document.body.getAttribute("data-user-id");

    // Ensure elements exist before using them
    window.notificationIcon = document.getElementById("notificationIcon");
    window.notificationBadge = document.querySelector(".notification-badge");


    //let notificationIcon = document.getElementById("notificationIcon");
    //let notificationBadge = document.querySelector(".notification-badge");

    // Hide the notification icon initially
    if (notificationIcon) {
        notificationIcon.classList.add("d-none");
    }

    // If the user is logged in (i.e., userId is not null), establish a SignalR connection
    if (userId) {
        startNotificationConnection(userId);
    } else {
        console.warn("User is not authenticated, skipping SignalR connection.");
    }

    // When the user visits the chat page, hide the badge and remove the highlight
    if (window.location.pathname.includes("/Chat")) {
        if (notificationIcon) {
            notificationIcon.classList.add("d-none");
            let iconElement = notificationIcon.querySelector("i");
            if (iconElement) {
                iconElement.classList.remove("text-warning");
            }
        }
    }
});

// Function that establishes a SignalR connection to the NotificationHub
function startNotificationConnection(userId) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();


    // Listen for incoming notifications from the server
    connection.on("ReceiveChatMessageNotification", function (message) {
        console.log("New chat message received!");

        if (notificationIcon) {
            notificationIcon.classList.remove("d-none"); // Show icon
            let iconElement = notificationIcon.querySelector("i");
            if (iconElement) {
                iconElement.classList.add("text-warning"); // Highlight icon
            }
        }

        showNotification();
    });

    connection.start()
        .then(() => console.log("Connected to NotificationHub"))
        .catch(err => console.error("Error connecting to NotificationHub:", err));
}

// Show notification icon and increment badge count
function showNotification() {
    if (notificationIcon) {
        notificationIcon.classList.add("show"); // Make icon visible
    }
    if (notificationBadge) {
        let currentCount = parseInt(notificationBadge.textContent || "0");
        notificationBadge.textContent = currentCount + 1;
        notificationBadge.classList.remove("d-none"); // Ensure badge is visible
    }
}

function clearNotification() {
    if (notificationIcon) {
        notificationIcon.classList.remove("show"); // Hide icon
    }
    if (notificationBadge) {
        notificationBadge.textContent = "";
        notificationBadge.classList.add("d-none"); // Hide badge when cleared
    }
}