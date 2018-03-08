
/*
$(function () {
    var chat = $.connection.chatHub;

    chat.client.broadcastMessage = function (name, message) {
    };

    $.connection.hub.start().done(function () {
        //$('#57bfc4509e9e836e38d0190d').click();
        $('#sendmessage').click(function () {
            // Call the Send method on the hub. 
            chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment. 
            $('#message').val('').focus();
        });
    });
});
*/