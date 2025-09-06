function openCustomerModal() {
    $('#customerModal').modal('show');
    $('#customerModal form')[0].reset();
    $('#customerModal input[name="Id"]').val(0);
}

function editCustomer(id) {
    $.get('/Admin/GetCustomer/' + id, function (data) {
        $('#customerModal input[name="Id"]').val(data.Id);
        $('#customerModal input[name="CustomerName"]').val(data.CustomerName);
        $('#customerModal input[name="Email"]').val(data.Email);
        $('#customerModal input[name="Phone"]').val(data.Phone);
        $('#customerModal input[name="City"]').val(data.City);
        $('#customerModal input[name="IsActive"]').prop('checked', data.IsActive);
        $('#customerModal').modal('show');
    });
}

