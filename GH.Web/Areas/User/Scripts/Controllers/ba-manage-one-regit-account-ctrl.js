myApp.getController('ManageOneRegitAccountController', ['$scope', '$rootScope', '$http', '$timeout', 'UserManagementService', 'AuthorizationService', 'BusinessAccountService', 'SweetAlert', function ($scope, $rootScope, $http, $timeout, _userManager, _authService, _busAccService, _sweetAlert) {
    $scope.preloaded = false;

    $scope.initFbCompleted = false;

    $scope.$on('FACEBOOK_INIT_COMPLETED', function () {
        $scope.initFbCompleted = true;
    })

    $scope.facebookPages = [];
    $scope.connectedPage = {};

    $scope.connectFacebook = {
        SelectedPage: null
    };

    function preload() {
        $scope.socialsLinked = angular.copy($rootScope.businessLinkedNetworks);
        if ($scope.socialsLinked.FacebookPage) {
            _busAccService.GetConnectedFacebookPage().then(function (fbPage) {
                $scope.connectedPage = fbPage;
                $scope.preloaded = true;
            })
        } else {
            $scope.preloaded = true;
        }
    }

    if ($rootScope.businessLinkedNetworksLoaded) {
        preload();
    } else {
        $scope.$on('BA_LINKED_NETWORKS_LOADED', function () {
            preload();
        })
    }

    $scope.configs = {
        ShowLinkFacebookPageModal: false
    }

    $scope.processLinking = function (link) {
        if ($scope.preloaded) {
            if ($scope.socialsLinked[link]) {
                _authService.GetExternalLoginProviders().then(function (providers) {
                    var login = null;

                    for (var i = 0; i < providers.length; i++) {
                        var provider = providers[i];
                        provider.Url = provider.Url.replace('/api/Account/ExternalLogin', '/api/AccountLink/ConnectSocialNetwork');
                        if (provider.Name == link) {
                            login = provider;
                            break;
                        }
                    }

                    _authService.ExternalLogin(login).then(function (data) {
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
                            }).then(function () {
                                if (link == 'Facebook') {
                                    $rootScope.linkedNetworks.Facebook = true;
                                }
                                if (link == 'Twitter') {
                                    $rootScope.linkedNetworks.Twitter = true;
                                }
                                __common.swal(_sweetAlert,'Link ' + link + ' successfully', '', 'success');
                            }, function (errors) {
                                $scope.socialsLinked[link] = false;
                                __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                            })
                        }
                    }, function () {
                        $scope.socialsLinked[link] = false;
                    })
                })
            } else {
                _userManager.UnlinkAccount({ Network: link })
                    .then(function () {
                        if (link == 'Facebook') {
                            $scope.socialsLinked.FacebookPage = false;
                            $rootScope.linkedNetworks.Facebook = false;
                            $scope.connectedPage = {};
                        }
                        if (link == 'Twitter') {
                            $rootScope.linkedNetworks.Twitter = false;
                        }
                        __common.swal(_sweetAlert,'Unlink ' + link + ' successfully', '', 'success');
                    }, function (errors) {
                        $scope.socialsLinked[link] = true;
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    })
            }
        }
    }

    var linked = false;
    $scope.processLinkFacebookPage = function () {
        if (!$scope.socialsLinked.FacebookPage) {
            $scope.disconnectPage();
        } else {
            var promise = $scope.getListFacebookPages();
            promise.then(function (result) {
                if (result === false) {
                    $scope.socialsLinked.FacebookPage = false;
                    return;
                }
                $scope.facebookPages = result.data;
                linked = false;
                $scope.configs.ShowLinkFacebookPageModal = true;
            })
        }
    }

    $scope.connectToPage = function () {
        if (!$scope.connectFacebook.SelectedPage) {
            __common.swal(_sweetAlert,'warning', $rootScope.translate('Please_choose_a_page_to_connect'), 'warning');
            return;
        }
        _busAccService.ConnectToFacebookPage({ Id: $scope.connectFacebook.SelectedPage.id }).then(function () {
            linked = true;
            $scope.connectedPage = { Id: $scope.connectFacebook.SelectedPage.id, PageName: $scope.connectFacebook.SelectedPage.name }
            $scope.connectFacebook = { };
            __common.swal(_sweetAlert,$rootScope.translate('success'), $rootScope.translate('Connect_facebook_page_successfully'), 'success');
            $scope.configs.ShowLinkFacebookPageModal = false;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.disconnectPage = function () {
        _busAccService.DisconnectToFacebookPage().then(function () {
            __common.swal(_sweetAlert,$rootScope.translate('success'), $rootScope.translate('UnConnect_facebook_page_successfully'), 'success');
            $scope.connectedPage = {};
            $scope.connectFacebook = {};
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.onLinkFacebookPageModalHidden = function () {
        if (linked === false) {
            $scope.socialsLinked.FacebookPage = false;
        }
        $scope.selectedFacebookPage = null;
        $scope.facebookPages = [];
        linked = false;
    }

    $scope.getListFacebookPages = function () {
        return _busAccService.GetManagedFacebookPages().then(function (pages) {
            return pages;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
            return false;
        })
    }

}])
