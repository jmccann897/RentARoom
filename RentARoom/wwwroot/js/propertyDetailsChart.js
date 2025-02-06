// Replace with the actual property ID
const propertyId = document.getElementById("viewCount").dataset.propertyId;

document.addEventListener('DOMContentLoaded', function () {
    fetchViewsPerDayData();
});

// Function to fetch view data
function fetchViewsPerDayData() {
    // Fetch views per day data
    fetch(`/User/Home/GetViewsPerDay?propertyId=${propertyId}`)
        .then(response => response.json())
        .then(data => {
            console.log("APi data: ", data);
            // Prepare data for Chart.js
            const labels = data.map(item => item.date);
            const viewsData = data.map(item => item.views);

            // Call a function to render the chart
            renderChart(labels, viewsData);
        })
        .catch(error => {
            console.error('Error fetching views per day data:', error);
        });
}

// Function to render the Chart.js line chart
function renderChart(labels, viewsData) {
    const ctx = document.getElementById('viewsChart').getContext('2d');

    new Chart(ctx, {
        type: 'line', // Line chart
        data: {
            labels: labels,
            datasets: [{
                label: 'Views Per Day',
                data: viewsData,
                borderColor: '#007bff', // Line color
                backgroundColor: 'rgba(0, 123, 255, 0.2)', // Fill color
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Date'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Views'
                    },
                    beginAtZero: true
                }
            }
        }
    });
}
