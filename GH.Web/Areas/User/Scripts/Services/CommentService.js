var _userApp = getApp('UserModule');
_userApp.factory('ConfirmDialogService', ['$http', '$q', '$cookies', '$interval', '$timeout', '$uibModal', function ($http, $q, $cookies, $interval, $timeout, $uibModal) {
    return {
        Open: function (message, functionclose) {
            var modalInstance = $uibModal.open({
                templateUrl: '/Areas/User/Views/Shared/Template/DialogConfirmTemplate.html',
                controller: 'DialogConfirmController',
                size: "",
                resolve: {
                    Content: { Message: message }
                }
            });
            modalInstance.result.then(function () {
                functionclose();
            }, function () {
                
            });
        }
    };
}])


_userApp.factory('fileUpload', ['$http', '$q', '$cookies', '$interval', '$timeout', function ($http, $q, $cookies, $interval, $timeout) {
    return {
        uploadFileToUrl: function (file, uploadUrl, uploadsuccessFunction, uploadfailFunction, data) {
            var fd = new FormData();
            fd.append('file', file)
            fd.append('data', data);
            $http.post(uploadUrl, fd, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            })
            .success(function (reponse) {
                uploadsuccessFunction(reponse);
            })
            .error(function () {
                uploadfailFunction();
            });
        }
    };
}])
_userApp.factory('CommonService', ['$http', '$q', '$cookies', function ($http, $q, $cookies) { 
    return {
        GetQuerystring: function (name, url) {
            if (!url) url = window.location.href;
            
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    };
}])
