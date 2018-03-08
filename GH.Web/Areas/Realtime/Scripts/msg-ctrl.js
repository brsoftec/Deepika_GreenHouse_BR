
//var myApp = getApp('myApp', ['ngRoute', 'ui.bootstrap', 'regit.ui', 'windows', 'msg'], true);


//Inject dependencies for CHAT into myApp
//if (myApp.requires.indexOf('ngRoute') < 0) {
//    myApp.requires.push('ngRoute');
//}
//if (myApp.requires.indexOf('ui.bootstrap') < 0) {
//    myApp.requires.push('ui.bootstrap');
//}
//if (myApp.requires.indexOf('regit.ui') < 0) {
//    myApp.requires.push('regit.ui');
//}
//if (myApp.requires.indexOf('windows') < 0) {
//    myApp.requires.push('windows');
//}
//if (myApp.requires.indexOf('msg') < 0) {
//    myApp.requires.push('msg');
//}

//myApp.getController('MsgCtrl', ['$scope', 'msgService',
//    function ($scope, msgService) {
//        $scope.msgList = msgService.msgList;
//    }
//]);

/*
$(function () {
    var chat = $.connection.chatHub;

    chat.client.broadcastMessage = function (userId, message) {
        $('#' + userId + '').click();
    };

    $.connection.hub.start().done(function () {
        $('#57bfc4509e9e836e38d0190d').click();
        $('#sendmessage').click(function () {
            // Call the Send method on the hub. 
            chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment. 
            $('#message').val('').focus();
        });
    });
});
*/