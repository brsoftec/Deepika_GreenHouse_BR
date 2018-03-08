var _userApp = getApp('UserModule');

_userApp.factory('InformationVaultService', ['$http', '$q', '$cookies', '$interval','$timeout', function ($http, $q, $cookies, $interval, $timeout) {

   
    return {
        VaultInformation: null,
        SaveVaultInformation: function (vaultinformation, userId ) {
            var deferer = $q.defer();
            var stringvaultinformation = JSON.stringify(vaultinformation);
            var VaultInformationModelView = new Object();
            VaultInformationModelView.StrVaultInformation = stringvaultinformation;
            VaultInformationModelView.UserId = userId;
            VaultInformationModelView.VaultInformation = vaultinformation
            $http.post('/api/InformationVaultService/SaveVaultInformation', VaultInformationModelView)
          .success(function (response) {
            
              deferer.resolve(response);
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

            return deferer.promise;
        },
        GetVaultInformation: function (vaultInformationId) {
            var deferer = $q.defer();
            var VaultInformationModelView = new Object();
           
            VaultInformationModelView.VaultInformationId = vaultInformationId;
            $http.post('/api/InformationVaultService/GetVaultInformation', VaultInformationModelView)
         .success(function (response) {
             deferer.resolve(response);
             
         }).error(function (errors, status) {
             __promiseHandler.Error(errors, status, deferer);
         })
            return deferer.promise;
        }
    }
}]);

