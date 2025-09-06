function openContactModal() {
    $('#contactModal').modal('show');
    $('#contactModal form')[0].reset();
    $('#contactModal input[name="ContactId"]').val(0);
}

function editContact(id) {
    $.get('/Admin/GetContact/' + id, function (data) {
        $('#contactModal input[name="ContactId"]').val(data.ContactId);
        $('#contactModal input[name="Name"]').val(data.Name);
        $('#contactModal input[name="Email"]').val(data.Email);
        $('#contactModal input[name="PhoneNumber"]').val(data.PhoneNumber);
        $('#contactModal input[name="Designation"]').val(data.Designation);
        $('#contactModal select[name="CustomerId"]').val(data.CustomerId);
        $('#contactModal').modal('show');
    });
}