var myApp = getApp('myApp', ['ngRoute', 'ui.bootstrap', 'regit.ui', 'windows', 'msg'], true);


myApp.getController('MsgCtrl', ['$scope', 'msgService',
    function ($scope, msgService) {
        bringToFront = function (wid) {
            $('.msg-box').mousedown(function () {
                $('.msg-box').css('zIndex', 1000);
                $(this).css('zIndex', 9999);
            });
        }
    }    
]);
