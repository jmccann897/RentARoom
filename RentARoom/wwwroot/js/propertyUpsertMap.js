let map, marker;

document.addEventListener("DOMContentLoaded", () => {
    // Initialise the modal event listeners
    const mapModal = document.getElementById('mapModal');
    mapModal.addEventListener('shown.bs.modal', function () {
        if (!map) {
            // Initialise map only when the modal is shown
            map = L.map('map').setView([54.607868, -5.926437], 13)

            // Add a tile layer
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
            }).addTo(map);

            // Add marker on click
            map.on('click', function (e) {
                const { lat, lng } = e.latlng;

                // Update or create marker
                if (marker) {
                    marker.setLatLng(e.latlng);
                } else {
                    marker = L.marker(e.latlng).addTo(map);
                }

                // Update input fields
                document.getElementById('latitude').value = lat.toFixed(6);
                document.getElementById('longitude').value = lng.toFixed(6);
            });
        }

        // Resize map after modal becomes visible
        setTimeout(() => {
            map.invalidateSize();
        }, 100);
    });

    mapModal.addEventListener('hidden.bs.modal', function () {
        // Clear map
        map.remove();
        map = null;
    });
});
