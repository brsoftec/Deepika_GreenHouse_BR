var _userApp = getApp('UserModule');
_userApp.factory('alertService', ['$http', '$q', '$cookies', '$interval','$timeout', function ($http, $q, $cookies, $interval, $timeout) {
    
    var _alerts = [];
    var _messageBox = "";
    var setValidationErrors = function (scope, validationErrors) {
        for (var prop in validationErrors) {
            var property = prop + "InputError";
            scope[property] = true;
        }
    }

    var returnFormattedMessage = function () {
        return _messageBox;
    }

    var returnAlerts = function () {
        return _alerts;
    }

    var renderErrorMessage = function (message) {

        var messageBox = formatMessage(message);
    
        _alerts = [];
        _messageBox = messageBox;
        _alerts.push({ 'type': 'danger', 'msg': '' });

    };
    var renderSuccessMessage = function (message) {

        var messageBox = formatMessage(message);

        _alerts = [];
        _messageBox = messageBox;
        _alerts.push({ 'type': 'success', 'msg': '' });
       
    };

    var renderWarningMessage = function (message) {

        var messageBox = formatMessage(message);
   
        _alerts = [];
        _messageBox = messageBox;
        _alerts.push({ 'type': 'warning', 'msg': '' });
     
    };

    var renderInformationalMessage = function (message) {

        var messageBox = formatMessage(message);

        _alerts = [];
        _messageBox = messageBox;
        _alerts.push({ 'type': 'info', 'msg': '' });
              
    };

    function formatMessage(message) {
        var messageBox = "";
        if (angular.isArray(message) == true) {
            for (var i = 0; i < message.length; i++) {
                messageBox = messageBox + message[i] + "<br/>";
            }
        }
        else {
            messageBox = message;
        }
        return messageBox;
    }

    function closeAlert() {
        _alerts.splice(0, 1);
    };

    return {
        setValidationErrors: setValidationErrors,
        returnFormattedMessage: returnFormattedMessage,
        returnAlerts: returnAlerts,
        renderErrorMessage: renderErrorMessage,
        renderSuccessMessage: renderSuccessMessage,
        renderWarningMessage: renderWarningMessage,
        renderInformationalMessage: renderInformationalMessage,
        closeAlert: closeAlert
    }
}]);
