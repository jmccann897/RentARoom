const connection = new signalR.HubConnectionBuilder()
    .withUrl("/propertyViewHub")
    .build();

connection.start().catch(err => console.error(err.toString()));

connection.on("UpdateViewCount", (propertyId, newViewCount) => {
    if (document.getElementById("viewCount").dataset.propertyId == propertyId) {
        const viewCountElement = document.getElementById("viewCountNumber");
        const spinner = document.getElementById("loadingSpinner");

        spinner.classList.remove("d-none");  // Show spinner

        setTimeout(() => {
            viewCountElement.innerText = newViewCount;
            spinner.classList.add("d-none");  // Hide spinner after update
        }, 300);  // Slight delay for smooth transition

        
    }
});