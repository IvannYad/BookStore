// Javascript code for some Users functions.

var dataTable;

$(document).ready(function () {
    loadDataTable();
});

// Function for displaying DataTable.
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        // Number of columns cannot exceed number of columns in cshtml document(<th> elements).
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "15%" },
            { data: '', "width": "10%" },
            {
                data: 'id', 
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                    </div>`
                },
                "width": "25%" 
            }
        ]
    });
}




