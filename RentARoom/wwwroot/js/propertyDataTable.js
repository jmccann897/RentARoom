var dataTable;

$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: 'Properties/getall' },
        "columns": [
        { data: 'address', "width": "15%" },
        { data: 'postcode', "width": "10%" },
        { data: 'applicationUser.userName', "width": "10%" },
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
                </div>`
            },
            "width": "15%", responsivePriority: 1
        }
        ],
        responsive: true // Enable responsive mode
    });
}

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


