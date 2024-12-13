// Initialise  map
var map = L.map('map').setView([54.607868, -5.926437], 13);

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);


// Create an array of markers to add to the map
const propertyMarkers = [];

// Initialize the map and fetch data
fetchPropertyData();

// #region Functions

// Fetch property data and handle the response
function fetchPropertyData() {
    $.ajax({
        type: "GET",
        url: '/User/Map/GetMapProperties',
        dataType: "json",
        success: handleResponse,
        error: handleError
    });
}

function handleResponse(response) {
    console.log("Response data:", response); // Log the entire response for inspection

    if (Array.isArray(response.data)) {
        response.data.forEach(createMarker);
    } else {
        console.error("Data is not an array. Please check the response format.");
    }
}

// Handle any errors that occur during the request
function handleError(response) {
    console.error("Error fetching properties:", response);
}

// Create a marker for each property and add it to the map
function createMarker(property) {
    if (property.latitude && property.longitude) {
        const marker = L.marker([property.latitude, property.longitude]).addTo(map);
        propertyMarkers.push(marker);

        // Add a popup with property details
        marker.bindPopup(`
            <strong>${property.address}</strong>, <strong>${property.city}</strong>, <strong>${property.postcode}</strong><br>
            Price: £${property.price}<br>
            Type: ${property.propertyType?.name || "Unknown"}
        `);
    } else {
        console.warn(`Property with ID ${property.id} is missing latitude/longitude.`);
    }
}

// #endregion
       

// #region MapProperties

// Circle
var circle = L.circle([54.5945, -5.9542], {
    color: 'red',
    fillColor: '#f03',
    fillOpacity: 0.5,
    radius: 50
}).addTo(map);

// Generic popups based on object mapped
// Can dynamic popups be used i.e specific to 
//marker.bindPopup("<b>Hello world!</b><br>I am a popup.").openPopup();
circle.bindPopup("I am a circle.");
//polygon.bindPopup("I am a polygon.");

// Polygon
var polygon = L.polygon([
    [51.509, -0.08],
    [51.503, -0.06],
    [51.51, -0.047]
])
    .addTo(map);


// Stand alone popup
var popup = L.popup()
    .setLatLng([54.595229, -5.93429])
    .setContent("Tesco Express")
    .openOn(map);

// Dealing with dynamically setting popup when user clicks 
var popupClick = L.popup();

function onMapClick(e) {
    popupClick
        .setLatLng(e.latlng)
        .setContent("You clicked the map at " + e.latlng.toString())
        .openOn(map);
}

map.on('click', onMapClick);

// #endregion