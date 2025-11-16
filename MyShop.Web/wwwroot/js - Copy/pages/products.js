var dtbl;

$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtbl = $("#mytable").DataTable({
        ajax: {
            url: "/Admin/Product/GetData", // لو في صفحة المنتجات
            type: "GET",
            dataSrc: "data" // خليه "" لو السيرفر بيرجع Array مش object
        },
        columns: [
            { data: "name" },
            { data: "discription" }, // حافظنا على الاسم زي ما طلبت
            { data: "price" },
            { data: "category.name" },
            {
                data: "id",
                render: function (data) {
                    return `
                        <a href="/Admin/Product/Edit/${data}" class="btn btn-success btn-sm">Edit</a>
                        <button onclick="DeleteItem('/Admin/Product/DeleteProduct/${data}')" class="btn btn-danger btn-sm">Delete</button>
                    `;
                },
                orderable: false,
                searchable: false
            }
        ],
        responsive: true,
        processing: true
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dtbl.ajax.reload();
                    } else {
                        toastr.error(data.message || 'Failed to delete the item.');
                    }
                },
                error: function () {
                    toastr.error('An error occurred while deleting.');
                }
            });
        }
    });
}
