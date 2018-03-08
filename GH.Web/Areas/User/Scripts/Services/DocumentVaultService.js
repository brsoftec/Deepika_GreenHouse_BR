var _userApp = getApp('UserModule');

_userApp.factory('DocumentVaultService', [
    '$http', '$q', function ($http, $q) {



        var _deleteFile = function (model) {
            var deferer = $q.defer();
            $http.post('/api/InformationVaultService/DeleteDocumentFile', model)
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

            return deferer.promise;
        }

        return {

            DeleteFile: _deleteFile
        };
    }
]);