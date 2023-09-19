// Javascript code for some Company functions.

var dataTable;

$(document).ready(function () {
    loadDataTable();
});

// Function for displaying DataTable.
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/company/getall' },
        // Number of columns cannot exceed number of columns in cshtml document(<th> elements).
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'streetAddress', "width": "15%" },
            { data: 'city', "width": "10%" },
            { data: 'state', "width": "10%" },
            { data: 'postalCode', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/company/edit?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                    <a onClick="Delete('/admin/company/delete/${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

// Function for displaying SweetAlerts.
function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        // When deleting is confirmed we apply to Delete action of CompanyController with [HttpDelete] attribute.
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
    })
}
