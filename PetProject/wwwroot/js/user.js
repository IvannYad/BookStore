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
            { data: 'name', "width": "20%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "15%" },
            { data: 'roleName', "width": "10%" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        // User locked.
                        return `
                        <div class="text-center">    
                            <a onClick=LockUnlock('${data.id}') class="btn btn-success text-white mb-1 mt-1" style="cursor:pointer; width:100px;">
                                <i class="bi bi-unlock-fill"></i> Unlock
                            </a>
                            <a class="btn btn-danger text-white" style="cursor:pointer; width:140px;">
                                <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                        `
                    }
                    else {
                        // User unlocked.
                        return `
                        <div class="text-center">    
                            <a onClick=LockUnlock('${data.id}') class="btn btn-danger text-white mb-1 mt-1" style="cursor:pointer; width:100px;">
                                <i class="bi bi-lock-fill"></i> Lock
                            </a>
                            <a class="btn btn-danger text-white" style="cursor:pointer; width:140px;">
                                <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                        `
                    }


                },
                "width": "30%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}


