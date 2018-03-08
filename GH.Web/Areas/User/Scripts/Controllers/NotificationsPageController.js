var myApp = getApp("myApp", true);

myApp.getController('NotificationsPageController', function ($scope) {

    $scope.getNotificationWhen = function(notif) {
        if (notif.Title.indexOf('expired') >= 0)
            return '';
        return notif.DateTime;
    };



});
