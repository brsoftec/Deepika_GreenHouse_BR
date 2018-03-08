var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'NotificationModule'], true);

myApp.getController('MainregitController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'SmSAuthencationService', '$http', function ($scope, $rootScope, $sce, _busAccService, _UserService, _sweetAlert, _authService, _smSAuthencationService, $http) {
    $scope.regitGlobal = window.regitGlobal;
    $scope.listAdBusiness = [];
    $scope.ProfileViewed = [];
    $scope.logout = function () {
        if (_authService.IsAuthorized()) {
            _authService.Logout().then(function () {
                window.location.href = "/";
            });
        } else {
            window.location.href = "/";
        }
    }
    $scope.SMSAuthencationVault = function () {
        _smSAuthencationService.OpenRedirect("Vault");
    }
    _UserService.GetCurrentUserProfile().then(function (profile) {
        if (profile) {
            $scope.user = profile;
        }
    });

    $scope.Exportuser = function () {
        $http.post('/Api/Account/GetUserInfoExport')
            .success(function (response) {
                alasql('SELECT * INTO XLSX("' + "Users Export" + '.xlsx",{headers:true}) FROM ?', [response]);
            }).error(function (errors, status) {
            })
    }

    $scope.profile = {};
    _busAccService.GetBusinessProfile().then(function (rs) {
        if (rs) {
            $scope.userBusiness = rs;
            var id = rs.AccountId;
            _busAccService.GetCompanyObjectDetailsById(id).then(function (response) {
                $scope.profile = response;

            });
        }

    });


    $scope.IsShowProfile = function () {
        if ($scope.ProfileViewed.IsShowProfile) {
            return true;
        }

        if ($scope.currentUserProfile && $scope.UserId == $scope.currentUserProfile.Id) {
            return true;
        }
        return false;
    };
    $scope.initProfile = function (id) {
        $scope.UserId = id;
        _UserService.GetUserProfile(id).then(function (response) {
            $scope.ProfileViewed = response;
        }, function (errors) {

        });
    }

    $scope.closeModule = function (event) {
        var module = angular.element(event.target).closest('.module');
        module.hide();
    };

    $scope.followBusiness = function (business) {
        _busAccService.FollowBusiness({ Id: business.Id }).then(function (followSummary) {
            business.Followed = true;
        }, function (errors) {
        })
    };

    $scope.unfollowBusiness = function (business) {
        _busAccService.UnfollowBusiness({ Id: business.Id }).then(function (followSummary) {
            business.Followed = false;
        }, function (errors) {
        })

    };

    $scope.followBus = function (business) {
        _busAccService.FollowBusiness({ Id: business.id }).then(function (followSummary) {
            business.following = true;
        }, function (errors) {
        })

    };

    $scope.unfollowBus = function (business) {
        _busAccService.UnfollowBusiness({ Id: business.id }).then(function (followSummary) {
            business.following = false;
        }, function (errors) {
        })

    };

}]);

myApp.getController('DiscoveryController', [ function($scope, $http) {
    var loadSize = 5;
    var loadUsers = [];
    $scope.checkLoadMore = false;
    $scope.totalUsers = 0;
    $scope.getAdBusiness = function()
    {
        $scope.checkLoadMore = false;
        var data = new Object();
        data.PageSize = loadSize;
        data.results = loadUsers;
        $http.post('/api/SearchService/GetAdBusisness', data)
            .success(function (response) {
                $scope.listAdBusiness = response.AdBusinesUsers;
                $scope.totalUsers = response.TotalUser;
                loadUsers = response.results;
                if (response.TotalUser > loadSize)
                    $scope.checkLoadMore = true;
            })
            .error(function (errors, status) {
            });

    };
    $scope.getAdBusiness();
    $scope.loadMore = function () {
        loadSize = loadSize + 5;
        $scope.getAdBusiness();
    }


}]);

myApp.getController('MainregitBussinessController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'SmSAuthencationService', '$http',  function ($scope, $rootScope, $sce, _busAccService, _UserService, _sweetAlert, _authService, _smSAuthencationService, $http) {
    $scope.listAdBusiness = [];
    //$scope.checkSMSAuthencation = function () {

    //    $http.post('/Api/Account/Verify/IsSMSAuthenticated', null)
    //  .success(function (resp) {
    //      if (!resp) {

    //          _authService.SendSMS().then(function (rs) {
    //              if (rs)
    //                  window.location.href = '/BusinessAccount/SignIn?requestId=' + rs;
    //              else
    //                  window.location.href = '/BusinessAccount/SignIn';
    //          });
    //      }
    //  });
    //}
    //$scope.checkSMSAuthencation();
    $scope.ProfileViewed = [];
    $scope.logout = function () {
        if (_authService.IsAuthorized()) {
            _authService.Logout().then(function () {
                window.location.href = "/";
            });
        } else {
            window.location.href = "/";
        }
    }
    $scope.SMSAuthencationVault = function () {
        _smSAuthencationService.OpenRedirect("Vault");
    }
    _UserService.GetCurrentUserProfile().then(function (profile) {
        if (profile) {
            $scope.user = profile;
        }
    });

    $scope.Exportuser = function () {
        $http.post('/Api/Account/GetUserInfoExport')
            .success(function (response) {
                alasql('SELECT * INTO XLSX("' + "Users Export" + '.xlsx",{headers:true}) FROM ?', [response]);
            }).error(function (errors, status) {
            })
    }

    $scope.profile = {};
    _busAccService.GetBusinessProfile().then(function (rs) {
        if (rs) {
            $scope.userBusiness = rs;
            var id = rs.AccountId;
            _busAccService.GetCompanyObjectDetailsById(id).then(function (response) {
                $scope.profile = response;
            });
        }
    });

    $scope.getAdBusiness = function () {
        var data = new Object();
        $http.post('/api/SearchService/GetAdBusisness', data)
            .success(function (response) {
                $scope.listAdBusiness = response;
            })
            .error(function (errors, status) {
            });
    }
    $scope.getAdBusiness();

    $scope.IsShowProfile = function () {
        if ($scope.ProfileViewed.IsShowProfile) {
            return true;
        }

        if ($scope.currentUserProfile && $scope.UserId == $scope.currentUserProfile.Id) {
            return true;
        }
        return false;
    };
    $scope.initProfile = function (id) {
        $scope.UserId = id;
        _UserService.GetUserProfile(id).then(function (response) {
            $scope.ProfileViewed = response;
        }, function (errors) {

        });
    }
    $scope.closeModule = function (event) {
        var module = angular.element(event.target).closest('.module');
        module.hide();
    };

    $scope.followBusiness = function (business) {
        _busAccService.FollowBusiness({ Id: business.Id }).then(function (followSummary) {
            business.Followed = true;
        }, function (errors) {
        })
    };

    $scope.unfollowBusiness = function (business) {
        _busAccService.UnfollowBusiness({ Id: business.Id }).then(function (followSummary) {
            business.Followed = false;
        }, function (errors) {
        })
    };

}]);
