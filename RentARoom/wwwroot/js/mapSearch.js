// Initialise  map
var map = L.map('map').setView([54.607868, -5.926437], 13);

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

// Create marker cluster groups for properties and locations
const propertyClusterGroup = L.markerClusterGroup();
const locationClusterGroup = L.markerClusterGroup();


// Property icon
const propertyIcon = L.divIcon({
    html: '<i class="bi bi-house-fill" style="font-size: 24px; color: darkblue;"></i>',
    iconSize: [24, 24],
    className: '' // Remove default class styling for DivIcon
});


// Initialize the map and fetch data
fetchMapData();

// #region Functions

function fetchMapData() {
    fetchPropertyData();
    fetchLocationData();
}

// Fetch property data and handle the response
function fetchPropertyData() {
    $.ajax({
        type: "GET",
        url: '/User/Map/GetMapProperties',
        dataType: "json",
        success: handleProperty,
        error: handleError
    });
}
// Fetch Location data and handle the response
function fetchLocationData() {
    console.log("reached");
    $.ajax({
        type: "GET",
        url: '/User/Map/GetMapLocations',
        dataType: "json",
        success: handleLocation,
        error: handleError
    });
}

// Property response if GET request is successful 
function handleProperty(response) {
    console.log("Property Response data:", response);

    if (Array.isArray(response.data)) {
        response.data.forEach(createPropertyMarker);
    } else {
        console.error("Property data is not an array. Please check the response format.");
    }
}
// Location response if GET request is successful 
function handleLocation(response) {
    console.log("Location Response data:", response);

    if (Array.isArray(response.data)) {
        response.data.forEach(createLocationMarker);
    } else {
        console.error("Location data is not an array. Please check the response format.");
    }
}

// Handle any errors that occur during the request
function handleError(response) {
    console.error("Error fetching properties:", response);
}

// Create a marker for each property and add to cluster group
function createPropertyMarker(property) {
    if (property.latitude && property.longitude) {
        const marker = L.marker([property.latitude, property.longitude], { icon: propertyIcon });
        // Add a popup with property details
        marker.bindPopup(`
            <strong>${property.address}</strong>, <strong>${property.city}</strong>, <strong>${property.postcode}</strong><br>
            Price: £${property.price || "Unknown"}<br>
            Type: ${property.propertyType?.name || "Unknown"}
        `);
        propertyClusterGroup.addLayer(marker); // Add marker to the cluster group
    } else {
        console.warn(`Property with ID ${property.id} is missing latitude/longitude.`);
    }
}

// Create a marker for each location and add to cluster group
function createLocationMarker(location) {
    if (location.latitude && location.longitude) {
        console.log("reached");
        const marker = L.marker([location.latitude, location.longitude]);
        // Add a popup with property details
        marker.bindPopup(`
            <strong>${location.locationName || "Unknown"}</strong><br>
            ${location.address || "No description available."}
        `);
        locationClusterGroup.addLayer(marker); // Add marker to the cluster group
    } else {
        console.warn(`Location with ID ${location.id} is missing latitude/longitude.`);
    }
}

// Layer control setup with MarkerCluster groups
L.control.layers(null,{
    'Properties': propertyClusterGroup,
    'Locations': locationClusterGroup,
}).addTo(map);

// Add cluster groups to the map (this will enable clustering and visibility)
propertyClusterGroup.addTo(map);
locationClusterGroup.addTo(map);


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