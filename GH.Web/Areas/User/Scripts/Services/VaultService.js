var _userApp = getApp('UserModule');

_userApp.factory('VaultService',['$http', '$q', function ($http, $q) {

    var _getFormVault = function (model) {
        var deferer = $q.defer();
        $http.post('/api/InformationVaultService/GetFormVault', model)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

    var _updateFormVault = function (model) {
        var deferer = $q.defer();
        var bsonString = JSON.stringify(model.FormString);
        var formVault = new Object();
        formVault.FormString = bsonString;
        formVault.AccountId = model.AccountId;
        formVault.FormName = model.FormName;
        $http.post('/api/InformationVaultService/UpdateFormVault', formVault)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getVault = function (model) {
        var deferer = $q.defer();
        $http.post('/api/InformationVaultService/GetVault', model)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    var _updateVault = function (model) {
        var deferer = $q.defer();
        var bsonStringVault = JSON.stringify(model.vaultString);
            var VaultInformationModelView = new Object();
            VaultInformationModelView.StrVaultInformation = bsonStringVault;
            VaultInformationModelView.UserId = model.userId;
        
        $http.post('/api/InformationVaultService/UpdateVault', VaultInformationModelView)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    
    return {
      
        GetFormVault: _getFormVault,
        UpdateFormVault: _updateFormVault,
        GetVault: _getVault,
        UpdateVault: _updateVault
    }


}])





