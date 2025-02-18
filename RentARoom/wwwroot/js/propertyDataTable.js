// Declare a global variable to store the DataTable instance
var dataTable;  

// Call the function to initialize the DataTable on page load
$(function () {
    checkUserRole(); // Check user role on page load
});

// Function to check user role and call the appropriate table loader
function checkUserRole() {
    if (isAgent) {
        loadAgentTable(); // Load the agent table
    } else {
        loadAdminTable(); // Load the admin table
    }
}


// Function to load the Admin Table
function loadAdminTable() {
    var columns = [
        { data: 'applicationUser.name', "width": "10%" }, // Show "Agent" column for admin
        {
            data: 'address',
            "width": "15%",
            "render": function (data, type, row) {
                return `<a href="/User/Home/Details/${row.id}" class="text-decoration-none">${data}</a>`;
            }
        },
        { data: 'postcode', "width": "10%" },
        { data: 'price', "width": "5%" },
        { data: 'numberOfBedrooms', "width": "5%" },
        { data: 'floorArea', "width": "5%" },
        { data: 'city', "width": "5%" },
        { data: 'propertyType.name', "width": "10%" },
        {
            data: 'id',
            "render": function (data) {
                return `
                <div class="w-75 btn-group" role="group">
                    <a href="properties/upsert/?id=${data}" class="btn btn-primary mx-2" data-bs-toggle="tooltip" title="Edit Property"> 
                    <i class="bi bi-pencil-square"></i>
                    </a>
                    <a onClick=Delete('properties/delete/${data}') class="btn btn-secondary mx-2" data-bs-toggle="tooltip" title="Delete Property">
                    <i class="bi bi-trash-fill"></i>
                    </a>
                </div>`;
            },
            "width": "15%", responsivePriority: 1
        }
    ];

    // Initialize the admin DataTable
    dataTable = $('#tblAdmin').DataTable({
        "ajax": { url: 'Properties/GetAll', type: 'GET' },
        "columns": columns,
        "order": [[0, 'asc']], // Sorting by 'Agent' column
        responsive: true
    });

    $('#tblAgent').hide(); // Hide the agent table
}

// Function to load the Agent Table
function loadAgentTable() {
    var columns = [
        {
            data: 'address',
            "width": "15%",
            "render": function (data, type, row) {
                return `<a href="/User/Home/Details/${row.id}" class="text-decoration-none">${data}</a>`;
            }
        },
        { data: 'postcode', "width": "10%" },
        { data: 'price', "width": "5%" },
        { data: 'numberOfBedrooms', "width": "5%" },
        { data: 'floorArea', "width": "5%" },
        { data: 'city', "width": "5%" },
        { data: 'propertyType.name', "width": "10%" },
        {
            data: 'id',
            "render": function (data) {
                return `
                <div class="w-75 btn-group" role="group">
                    <a href="properties/upsert/?id=${data}" class="btn btn-primary mx-2" data-bs-toggle="tooltip" title="Edit Property"> 
                    <i class="bi bi-pencil-square"></i>
                    </a>
                    <a onClick=Delete('properties/delete/${data}') class="btn btn-secondary mx-2" data-bs-toggle="tooltip" title="Delete Property">
                    <i class="bi bi-trash-fill"></i>
                    </a>
                </div>`;
            },
            "width": "15%", responsivePriority: 1
        }
    ];

    // Initialize the agent DataTable
    dataTable = $('#tblAgent').DataTable({
        "ajax": { url: 'Properties/GetAll', type: 'GET' },
        "columns": columns,
        "order": [[1, 'asc']], // Sorting by 'address' column
        responsive: true
    });

    $('#tblAdmin').hide(); // Hide the admin table
}

//function loadDataTable() {
//    dataTable = $('#tblData').DataTable({
//        "ajax": { url: 'Properties/getall' },
//        "columns": [
//        { data: 'address', "width": "15%" },
//        { data: 'postcode', "width": "10%" },
//        { data: 'applicationUser.userName', "width": "10%" },
//        { data: 'price', "width": "5%" },
//        { data: 'numberOfBedrooms', "width": "5%" },
//        { data: 'floorArea', "width": "5%" },
//        { data: 'city', "width": "5%" },
//        { data: 'propertyType.name', "width": "10%" },
//        {
//            data: 'id',
//            "render": function (data) {
//                return `
//                <div class="w-75 btn-group" role="group">
//                    <a href="properties/upsert/?id=${data}" class="btn btn-primary mx-2" data-bs-toggle="tooltip" title="Edit Property"> 
//                    <i class="bi bi-pencil-square"></i>
//                    </a>
//                    <a onClick=Delete('properties/delete/${data}') class="btn btn-secondary mx-2" data-bs-toggle="tooltip" title="Delete Property">
//                    <i class="bi bi-trash-fill"></i>
//                    </a>
//                </div>`
//            },
//            "width": "15%", responsivePriority: 1
//        }
//        ],
//        responsive: true // Enable responsive mode
//    });
//}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message)
                }
            })
        }
    });
}


