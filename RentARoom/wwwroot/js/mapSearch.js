var map = L.map('map').setView([54.607868, -5.926437], 13);

L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);


// Create an array of markers to add to the map

// Style markers

// Add cards to markers
var marker = L.marker([54.595614, -5.960767]).addTo(map);


// Cirlce
var circle = L.circle([54.5945, -5.9542], {
    color: 'red',
    fillColor: '#f03',
    fillOpacity: 0.5,
    radius: 50
}).addTo(map);

// Generic popups based on object mapped
// Can dynamic popups be used i.e specific to 
marker.bindPopup("<b>Hello world!</b><br>I am a popup.").openPopup();
circle.bindPopup("I am a circle.");
polygon.bindPopup("I am a polygon.");

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

map.on('click', onMapClick);