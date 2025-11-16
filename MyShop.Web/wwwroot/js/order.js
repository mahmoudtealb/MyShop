var dtbl;

$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtbl = $("#mytable").DataTable({
        ajax: {
            url: "/Admin/Order/GetData",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "id" },
            { data: "name" },
            { data: "phoneNumber" },
            { data: "applicationUserEmail" },
            { data: "orderStatus" },
            { data: "totalPrice" }, // ✅ مطابق للسيرفر الآن
            {
                data: "id",
                render: function (data) {
                    return `
                        <a href="/Admin/Order/Details?orderid=${data}" class="btn btn-warning btn-sm">Details</a>
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
