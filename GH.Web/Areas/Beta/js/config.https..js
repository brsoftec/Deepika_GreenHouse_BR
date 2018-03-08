'use strict';
$('body').append('<ajaxLoader></ajaxLoader>');
//declare the app
angular
  .module('regitApp').run(['$rootScope', function ($rootScope) {

    $rootScope.ajaxLoader = { IsLoading: false, ActiveRequest: 0, ActiveRequestShowLoader: 0 };

    $rootScope.$watch(function () {
        return $rootScope.ajaxLoader.ActiveRequestShowLoader;
    }, function (newVal) {
        if (newVal > 0) {
            $rootScope.ajaxLoader.IsLoading = true;
        } else {
            $rootScope.ajaxLoader.IsLoading = false;
        }
    });

}]).factory('htppBearerAuthorizationInterceptor', ['$rootScope', '$cookies', '$q', function ($rootScope, $cookies, $q) {
    return {
        'request': function (config) {
            if (config.params && config.params.isExternalRequest) {
                delete (config.params.isExternalRequest)
            } else {
                var accessToken = $cookies.get('access_token');
                if (typeof accessToken != 'undefined') {
                    config.headers.Authorization =  accessToken;
                } else if (typeof externalAccessToken != 'undefined') {
                    config.headers.Authorization = 'bearer ' + externalAccessToken;
                }
            }

            if ((!config.params || (config.params && !config.params.hideAjaxLoader)) && !config.hideAjaxLoader) {
                $rootScope.ajaxLoader.ActiveRequestShowLoader++;
            } else {
                config.hideAjaxLoader = true;
                if (config.params && config.params.hideAjaxLoader) {
                    delete (config.params.hideAjaxLoader)
                }
            }

            $rootScope.ajaxLoader.ActiveRequest++;
            return config;
        },
        'requestError': function (rejection) {
            $rootScope.ajaxLoader.ActiveRequest--;
            if (!rejection.config.hideAjaxLoader) {
                $rootScope.ajaxLoader.ActiveRequestShowLoader--;
            }
            return $q.reject(rejection);
        },
        'response': function (response) {
            $rootScope.ajaxLoader.ActiveRequest--;
            if (!response.config.hideAjaxLoader) {
                $rootScope.ajaxLoader.ActiveRequestShowLoader--;
            }
            return $q.resolve(response);
        },
        'responseError': function (rejection) {
            $rootScope.ajaxLoader.ActiveRequest--;
            if (!rejection.config.hideAjaxLoader) {
                $rootScope.ajaxLoader.ActiveRequestShowLoader--;
            }
            return $q.reject(rejection);
        }
    };
}]).config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('htppBearerAuthorizationInterceptor');
}]);


    angular.module('regitApp').directive('ajaxloader', ['$rootScope', function ($rootScope) {
        return {
            restrict: 'E',
            replace: 'true',
            template: '<div id="ajaxLoader"><div id="fountainTextG"><div id="fountainTextG_1" class="fountainTextG">L</div><div id="fountainTextG_2" class="fountainTextG">o</div><div id="fountainTextG_3" class="fountainTextG">a</div><div id="fountainTextG_4" class="fountainTextG">d</div><div id="fountainTextG_5" class="fountainTextG">i</div><div id="fountainTextG_6" class="fountainTextG">n</div><div id="fountainTextG_7" class="fountainTextG">g</div></div><div id="ajaxLoaderBackdrop"></div></div>',
            link: function (scope, element) {
                $rootScope.$watch(function () {
                    return $rootScope.ajaxLoader.IsLoading;
                }, function (val) {
                    if (val) {
                        $(element).addClass('active');
                    } else {
                        $(element).removeClass('active');
                    }
                });
            }
        }
    }])
