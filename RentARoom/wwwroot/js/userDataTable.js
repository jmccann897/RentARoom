
// Declare a global variable to store the DataTable instance
var dataTable;  

// Call the function to initialize the DataTable on page load
$(function () {
    checkUserRole(); // Check user role on page load
});

// Function to check user role and call the appropriate table loader
function checkUserRole() {
    if (isAdmin) {
        loadUserTable(); // Load the agent table
    }
}


// Function to load the Admin Table
function loadUserTable() {
    var columns = [
        { data: 'name', title: "Name" },
        { data: 'email', title: "Email" },
        { data: 'phoneNumber', title: "Phone" },
        { data: 'role', title: "Role" },
        {
            data: 'id',
            title: 'Actions',
            "render": function (data) {
                return `
                <div class="w-75 btn-group" role="group">
                    
                    <a onClick=Delete('Admin/DeleteUser/${data}') class="btn btn-secondary mx-2" data-bs-toggle="tooltip" title="Delete Property">
                    <i class="bi bi-trash-fill"></i>
                    </a>
                </div>`;
            },
            responsivePriority: 1
        }
    ];

    // Initialize the admin DataTable
    dataTable = $('#tblUsers').DataTable({
        "ajax": {
            url: 'Admin/GetAllUsers',
            type: 'GET',
        },
        "columns": columns,
        "order": [[0, 'asc']], // Sorting by 'Name' column
        responsive: true
    });
}

// Delete User handling
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
                    if (data.success) {
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}


