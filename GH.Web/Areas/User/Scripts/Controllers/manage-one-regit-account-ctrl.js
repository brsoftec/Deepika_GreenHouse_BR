myApp.getController('ManageOneRegitAccountController', ['$scope', '$rootScope', '$http', '$timeout', 'UserManagementService', 'AuthorizationService', 'BusinessAccountService', 'SweetAlert', function ($scope, $rootScope, $http, $timeout, _userManager, _authService, _busAccService, _sweetAlert) {
    $scope.preloaded = false;

    $scope.initFbCompleted = false;

    $scope.$on('FACEBOOK_INIT_COMPLETED',
        function() {
            $scope.initFbCompleted = true;
        });

    $scope.$on('LINKED_NETWORKS_LOADED',
        function() {
            $scope.socialsLinked = angular.copy($rootScope.linkedNetworks);
            $scope.preloaded = true;
        });

    $scope.configs = {
        ShowLinkFacebookPageModal: false
    }

    $scope.processLinking = function (link) {
        if ($scope.preloaded) {
            if ($scope.socialsLinked[link]) {
                _authService.GetExternalLoginProviders()
                    .then(function(providers) {
                        var login = null;

                        for (var i = 0; i < providers.length; i++) {
                            var provider = providers[i];
                            provider.Url = provider.Url.replace('/api/Account/ExternalLogin',
                                '/api/AccountLink/ConnectSocialNetwork');
                            if (provider.Name == link) {
                                login = provider;
                                break;
                            }
                        }

                        _authService.ExternalLogin(login)
                            .then(function(data) {
                                    if (typeof data.error != 'undefined') {
                                        __common.swal(_sweetAlert,'warning', data.error, 'warning');
                                        $scope.socialsLinked[link] = false;
                                    } else {
                                        _userManager.LinkAccount({
                                                Network: link,
                                                AccessToken: data.accessToken,
                                                SecretAccessToken: data.accessTokenSecret,
                                                SocialAccountId: data.socialAccountId,
                                                TwitterName: data.twitterName
                                            })
                                            .then(function() {
                                                    if (link == 'Facebook') {
                                                        $rootScope.linkedNetworks.Facebook = true;
                                                        __common.swal(_sweetAlert,$rootScope
                                                            .translate('Link_Facebook_Success_Message'),
                                                            '',
                                                            'success');
                                                    }
                                                    if (link == 'Twitter') {
                                                        $rootScope.linkedNetworks.Twitter = true;
                                                        _sweetAlert
                                                            .swal($rootScope.translate('Link_Twitter_Success_Message'),
                                                                '',
                                                                'success');
                                                    }
                                                },
                                                function(errors) {
                                                    $scope.socialsLinked[link] = false;
                                                    __errorHandler
                                                        .Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                                                });
                                    }
                                },
                                function() {
                                    $scope.socialsLinked[link] = false;
                                });
                    });
            } else {
                _userManager.UnlinkAccount({ Network: link })
                    .then(function() {
                            if (link == 'Facebook') {
                                $scope.socialsLinked.FacebookPage = false;
                                $rootScope.linkedNetworks.Facebook = false;
                                $scope.connectedPage = {};
                                __common.swal(_sweetAlert,$rootScope
                                    .translate('UnLink_Facebook_Success_Message'),
                                    '',
                                    'success');
                            }
                            if (link == 'Twitter') {
                                $rootScope.linkedNetworks.Twitter = false;
                                __common.swal(_sweetAlert,$rootScope.translate('UnLink_Twitter_Success_Message'), '', 'success');
                            }
                        },
                        function(errors) {
                            $scope.socialsLinked[link] = true;
                            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                        });
            }
        }
    }
}])
