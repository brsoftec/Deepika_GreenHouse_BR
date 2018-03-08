var myApp = getApp('myApp');

myApp.directive('permission', ['$rootScope', function ($rootScope) {

    function hasPermission(permissions) {
        if (!$rootScope.currentUserRoles) {
            return false;
        }
        for (var i = 0; i < permissions.length; i++) {
            var permission = permissions[i];

            if ($rootScope.currentUserRoles.indexOf(permission) >= 0) {
                return true;
            }
        }
        return false;
    }

    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var permissions = [];
            if (attrs.permission) {
                permissions = attrs.permission.split(',');
            }

            if (!hasPermission(permissions)) {
                element.hide();
            }

            scope.$on('ROLE_LOADED', function () {
                if (hasPermission(permissions)) {
                    element.show();
                } else {
                    element.hide();
                }
            })
        }
    }
}])

myApp.run(['UserManagementService', 'AuthorizationService', 'BusinessAccountService', '$rootScope', function (_userManager, _authService, _BAccountService, $rootScope) {
    $rootScope.businessLinkedNetworks = {
        Facebook: false,
        Twitter: false
    }

    $rootScope.businessLinkedNetworksLoaded = false;

    if (_authService.IsAuthorized()) {
        _BAccountService.CheckSocialNetworks().then(function (checks) {
            $rootScope.businessLinkedNetworks = checks;
            $rootScope.businessLinkedNetworksLoaded = true;
            $rootScope.$broadcast('BA_LINKED_NETWORKS_LOADED');
        })
    }

    if (_authService.IsAuthorized()) {
        // _BAccountService.GetBusinessProfile().then(function (profile) {
        //     $rootScope.BAProfile = profile;
        //     $rootScope.$broadcast('BUSINESS_ACCOUNT_PROFILE_LOADED', profile.Id)
        //     _userManager.CheckBASocialNetworks($rootScope.BAProfile.Id).then(function (checks) {
        //         $rootScope.linkedNetworks = checks;
        //         $rootScope.$broadcast('LINKED_NETWORKS_LOADED');
        //     })
        // })

        _BAccountService.GetRolesOfCurrentUser(true).then(function (roles) {
            $rootScope.currentUserRoles = roles;
            $rootScope.$broadcast('ROLE_LOADED');
        }, function (errors) {
            alert(__errorHandler.ProcessErrors(errors).Messages.join('\n'));
        })
    }

}])