var myApp = getApp('myApp');

myApp.run(['UserManagementService', 'AuthorizationService', '$rootScope',
    function (_userManager, _authService, $rootScope) {
    $rootScope.linkedNetworks = {
        Facebook: false,
        Twitter: false
    }
    $rootScope.linkedWithBusinessAccount = false;

    if (_authService.IsAuthorized()) {
        _userManager.CheckSocialNetworks().then(function (checks) {
            $rootScope.linkedNetworks = checks;
            $rootScope.$broadcast('LINKED_NETWORKS_LOADED');
        })

        _authService.IsLinkedWithBusinessAccount(true).then(function (linked) {
            $rootScope.linkedWithBusinessAccount = linked;
        })
    }
}])

myApp.getController("UserLayoutController",
['$scope', '$rootScope', '$http', 'AuthorizationService', 'UserManagementService', 'SmSAuthencationService',
function ($scope, $rootScope, $http, _authService, _userManager,_smSAuthencationService) {
    $scope.logout = function () {
        if (_authService.IsAuthorized()) {
            _authService.Logout().then(function () {
                window.location.href = "/";
            });
        } else {
            window.location.href = "/";
        }
    }

    $scope.RediectToAction = function (path) {
        $location.path(path);
    }

     $scope.SMSAuthencationVault = function () {
         _smSAuthencationService.OpenRedirect("Vault");
    }

    if (_authService.IsAuthorized()) {
        _userManager.GetCurrentUserProfile().then(function(profile) {
            $rootScope.currentUserProfile = profile;
            _userManager.CurrentUser = profile;
        });
    }

}])