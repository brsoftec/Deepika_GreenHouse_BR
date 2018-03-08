var _userApp = getApp('UserModule');

_userApp.factory('appInfo', [ function () {

  var service = {
            status: {
                busy: false,
                message: ''
            },
            setInfo: setInfo
        };

        return service;

        function setInfo(args) {
            if (args) {
                if (args.hasOwnProperty('busy')) {             
                    service.status.busy = args.busy;
                }
                if (args.hasOwnProperty('message')) {                    
                    service.status.message = args.message;
                }
            } else {
                service.status.busy = false;
                service.status.message = '';
            }
        }
}]);