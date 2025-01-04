// #region Initialise  map
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
    className: '' // Remove default class styling for DivIcon otherwise looks bad
});
// #endregion

// Fetch data
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
    if (Array.isArray(response.data)) {
        response.data.forEach(createPropertyMarker);
    } else {
        console.error("Property data is not an array. Please check the response format.");
    }
}
// Location response if GET request is successful 
function handleLocation(response) {
    if (Array.isArray(response.data)) {
        response.data.forEach(createLocationMarker);
    } else {
        console.error("Location data is not an array. Please check the response format.");
    }
}

// Handle any errors that occur during the request
function handleError(response) {
    console.error("Error fetching data:", response);
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

// #region Search Functionality

// Function to filter markers based on the search term
function filterMarkers(searchTerm) {
    // Clear all current markers from the property cluster group
    propertyClusterGroup.clearLayers();

    // Filter the property data based on the search term
    fetch('/User/Map/GetMapProperties', {
        method: 'GET'
    })
        .then(response => response.json())
        .then(data => {
            if (Array.isArray(data.data)) {
                data.data
                    .filter(property => {
                        const content = `
                            ${property.address || ''} 
                            ${property.city || ''} 
                            ${property.postcode || ''}
                            ${property.propertyType?.name || ''}
                        `.toLowerCase();
                        return content.includes(searchTerm.toLowerCase());
                    })
                    .forEach(createPropertyMarker); // Recreate filtered markers
            } else {
                console.error("Property data is not an array. Please check the response format.");
            }
        })
        .catch(error => console.error("Error fetching property data:", error));
}

// Event listener for the search box input

document.addEventListener('DOMContentLoaded', function () {
    console.log("Script loaded and DOM is ready");

    // Get the search input element and add the event listener for 'input' event
    const searchInput = document.getElementById('searchField');

    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            console.log('Input detected:', e.target.value); // Ensure this logs when you type
            const searchTerm = e.target.value.trim();
            filterMarkers(searchTerm);
        });
    } else {
        console.error("Search input element not found!");
    }
});

// #endregion

// Layer control setup with MarkerCluster groups
L.control.layers(null,{
    'Properties': propertyClusterGroup,
    'Locations': locationClusterGroup,
}).addTo(map);

// Add cluster groups to the map (this will enable clustering and visibility)
propertyClusterGroup.addTo(map);
locationClusterGroup.addTo(map);
// #endregion

// #region Map Refresh Function
function mapRefresh() {
    // Clear all markers from the cluster groups
    propertyClusterGroup.clearLayers();
    locationClusterGroup.clearLayers();

    // Fetch updated data and re-populate the map
    fetchMapData();
}
// #endregion


// #region Event Handlers & Listeners

// Click on map event --> Show add location version of modal
map.on('click', (e) => {
    const { lat, lng } = e.latlng; // Extract latitude and longitude
    initializeModal({ latitude: lat, longitude: lng });
});

// If no data, then empty otherwise populate the modal field with values
function initializeModal(data = {}) {
    document.getElementById('locationId').value = data.id || '';
    document.getElementById('locationName').value = data.locationName || '';
    document.getElementById('locationAddress').value = data.address || '';
    document.getElementById('locationCity').value = data.city || '';
    document.getElementById('locationPostcode').value = data.postcode || '';
    document.getElementById('locationLat').value = data.latitude || '';
    document.getElementById('locationLng').value = data.longitude || '';

    const modalTitle = document.getElementById('locationModalLabel');
    const saveButton = document.getElementById('saveLocationBtn');

    if (data.id) {
        modalTitle.textContent = 'Edit Location';
        saveButton.textContent = 'Save Changes';
    } else {
        modalTitle.textContent = 'Add Location';
        saveButton.textContent = 'Save';
    }

    const modal = new bootstrap.Modal(document.getElementById('locationModal'));
    modal.show();
};

document.getElementById('saveLocationBtn').addEventListener('click', function (e) {
    console.log('Button clicked'); // This will show up in your browser console
    e.preventDefault();
    const form = document.getElementById('locationForm');
    form.dispatchEvent(new Event('submit')); // Manually trigger form submit
});


// Modal form submit event --> either an add or edit depending on value of location Id 
document.getElementById('locationForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const locationId = document.getElementById('locationId').value;
    const locationName = document.getElementById('locationName').value;
    const address = document.getElementById('locationAddress').value;
    const city = document.getElementById('locationCity').value;
    const postcode = document.getElementById('locationPostcode').value;
    const latitude = document.getElementById('locationLat').value;
    const longitude = document.getElementById('locationLng').value;

    const locationData = {
        id: locationId, // only populate for edit
        locationName,
        address,
        city,
        postcode,
        latitude,
        longitude
    };

    // AJAX call for Add or Edit (based on locationId)
    const url = locationId ? `/User/Map/Edit/${locationId}` : '/User/Map/SaveNewLocation';
    const type = locationId ? 'PUT' : 'POST';

    console.log(url);
    console.log(type);

    // AJAX request depending on modal edit in data table or add via map
    $.ajax({
        type: type,
        url: url,
        contentType: 'application/json',
        data: JSON.stringify(locationData),
        success: function (response) {
            if (response.success) {
                console.log("this dynamic ajax reached");
                const marker = L.marker([latitude, longitude])
                    .bindPopup(`<strong>${locationName}</strong><br>${address}, ${city} ${postcode}`)
                    .addTo(locationClusterGroup);

                alert('Location saved successfully!');
                const modal = bootstrap.Modal.getInstance(document.getElementById('locationModal'));
                modal.hide(); // Close the modal
                locationsTable.ajax.reload(); // Refresh the table with the new data
                mapRefresh();
            } else {
                alert('Error saving location. Please try again.');
            }
        },
        error: function (error) {
            console.error('Error saving location:', error);
            alert('An error occurred while saving the location.');
        }
    });
});

// #region Location Datatable
let locationsTable;

$(document).ready(function () {
    loadLocationsTable();
});

function loadLocationsTable() {
    locationsTable = $('#tblLocations').DataTable({
        "ajax": { url: '/User/Map/GetMapLocations', type: 'GET' }, 
        "columns": [
            { data: 'locationName', "width": "30%" },
            { data: 'address', "width": "40%" },
            { data: 'city', "width": "20%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <button class="btn btn-primary mx-2" onclick="editLocation(${data})"> 
                            <i class="bi bi-pencil-square"></i> Edit
                        </button>
                        <button class="btn btn-secondary mx-2" onclick="deleteLocation(${data})">
                            <i class="bi bi-trash-fill"></i> Delete
                        </button>
                    </div>`;
                },
                "width": "10%"
            }
        ]
    });
}

// Edit Get request when selecting edit option in datatable
function editLocation(id) {
    $.ajax({
        url: `/User/Map/GetLocation/${id}`,
        type: 'GET',
        success: function (data) {
            console.log(data);
            if (data && data.locationName && data.address && data.city && data.postcode) {
                initializeModal(data);          
            } else {
                toastr.error("Location not found.");
            }
        },
        error: function (error) {
            toastr.error("Failed to load location data.");
        }
    });
}
// Delete API is working bar toastr
function deleteLocation(id) {
    if (confirm('Are you sure you want to delete this location?')) {
        $.ajax({
            url: `/User/Map/DeleteLocation/${id}`,
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    locationsTable.ajax.reload(); // Reload table after deletion
                    mapRefresh();
                } else {
                    alert('Failed to delete location.');
                }
            },
            error: function () {
                alert('An error occurred while deleting the location.');
            }
        });
    }
}

// #endregion

// #endregion
