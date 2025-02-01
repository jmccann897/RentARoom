document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM is ready!");

    // Handle "Get Travel Time" button click
    document.getElementById("calculateTravelTime").addEventListener("click", function (event) {
        event.preventDefault(); // Prevent form submission, if inside a form.

        let profile = document.getElementById("travelProfile").value;

        // Get the propertyId, latitude and longitude values from the hidden input fields
        const propertyId = document.getElementById("propertyId").value;
        const propertyLatitude = parseFloat(document.getElementById('propertyLatitude').value);
        const propertyLongitude = parseFloat(document.getElementById('propertyLongitude').value);

        let travelTimesList = document.getElementById("travelTimesList");
        travelTimesList.innerHTML = ""; // Clear previous results

        const userLocations = JSON.parse(document.getElementById('userLocations').value || '[]');
        if (userLocations.length === 0) {
            alert('No user locations available.');
            travelTimesList.innerHTML = `<li class='list-group-item text-warning'>⚠ No user locations available.</li>`;
            return;
        }

        let userId = document.getElementById("userId")?.value; // Handle if not logged in

        if (!userId) {
            travelTimesList.innerHTML = `<li class='list-group-item text-danger'>⚠ Please log in to calculate travel time.</li>`;
            return;
        }

        console.log("Fetching travel time for propertyId:", propertyId, "with profile:", profile);  // Log the request params

        fetch(`/User/Home/GetTravelTime?propertyId=${propertyId}&profile=${profile}`)
            .then(response => {
                console.log("Received response:", response);
                return response.json();
            })
            .then(data => {
                console.log("Received data: ", data);

                if (data.travelTimes && data.travelTimes.length > 0 && data.distances && data.distances.length > 0) {
                    let travelTimes = data.travelTimes.slice(1);
                    let distances = data.distances.slice(1);

                    // Loop through results to populate UI
                    travelTimes.forEach((time, index) => { 

                        // Get Location related to time
                        let userLocation = userLocations[index];

                        // Get distance related to time
                        let distance = distances[index]; 

                        if (userLocation) {
                            // Check if location name exists in user location and display it
                            let locationName = userLocation.locationName || "Unknown Location"; 

                            let li = document.createElement("li");
                            li.classList.add("list-group-item", "d-flex", "justify-content-between", "align-items-center");
                            
                            li.innerHTML = `
                                <div>
                                    <strong class="text-dark">${locationName}</strong><br>
                                    <small><i class="bi bi-clock-history"></i> ${time} min &nbsp;
                                    <i class="bi bi-geo-alt"></i> ${(distance / 1000).toFixed(2)} km</small>
                                </div>
                                `;
                            travelTimesList.appendChild(li);
                        }
                    });
                } else {
                    travelTimesList.innerHTML = "<li class='list-group-item text-muted'>No travel times available.</li>";
                }
            })
            .catch(error => {
                console.error("Error fetching travel time:", error);
                travelTimesList.innerHTML = `<li class='list-group-item text-danger'>❌ Error fetching data.</li>`;
            });
    });
});