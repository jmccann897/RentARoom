
// #region Helper functions

// Helper function to check if the email exists
export async function checkUserEmail(email) {
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

// Helper function to filter property dropdown
export function filterPropertiesByEmail(email, propertyId) {
    const propertySelect = $('#propertySelect'); // Don't empty here!
    propertySelect.empty().append('<option value="">Select Property (Optional)</option>'); // Add default option

    if (email) {
        $.ajax({
            url: '/User/Notification/GetUserPropertiesByEmail',
            type: 'GET',
            data: { recipientEmail: email },
            success: function (response) {
                if (response && response.properties && Array.isArray(response.properties)) {
                    response.properties.forEach(property => {
                        const option = $(`<option value="${property.id}">${property.address}</option>`);
                        propertySelect.append(option);
                        if (propertyId && property.id === propertyId) {
                            option.prop('selected', true);
                        }
                    });
                } else {
                    console.error('Invalid properties response:', response);
                }
            },
            error: function (error) {
                console.error('Error fetching properties:', error);
            }
        });
    } else {
        // If recipientEmail is empty, show all properties
        const propertiesJson = '@Html.Raw(Json.Serialize(Model.UserProperties))'; // Get the JSON string
        const properties = JSON.parse(propertiesJson); // Parse the JSON string
        properties.forEach(property => {
            const option = $(`<option value="${property.id}">${property.address}</option>`);
            propertySelect.append(option);
            if (propertyId && property.id === propertyId) {
                option.prop('selected', true);
            }
        });
    }
}
