

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: 'Properties/getall' },
        "columns": [
        { data: 'address', "width": "25%" },
        { data: 'postcode', "width": "10%" },
        { data: 'owner', "width": "10%" },
        { data: 'price', "width": "10%" },
        { data: 'numberOfBedrooms', "width": "10%" },
        { data: 'floorArea', "width": "10%" },
        { data: 'propertyType.name', "width": "15%" }
        ]
    });
}



