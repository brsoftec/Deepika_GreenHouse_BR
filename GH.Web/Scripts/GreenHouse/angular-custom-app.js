//******************************* Construct a custom Angular App *******************************//
// use getApp(appName: string, library: array<string>, useAjaxLoading: bool) to get custom app
// use app.getController(controllerName: string, functionBody: function) to create a controller with do function as functionBody
// use app.getController(controllerName: string, [...params-services: string, functionBody: function]) to create a controller with explicit service injected into functionBody

var theApps = [];

$('body').append('<ajaxLoader></ajaxLoader>');
function getApp(appName, library, useAjaxLoading, qudy) {
    //is the app is constructed before
    if (typeof theApps[appName] == 'undefined' || theApps[appName] == null) {
        if (!library) {
            library = [];
        }

        if (library.indexOf('ngCookies') < 0) {
            library.push('ngCookies');
        }
        if (library.indexOf('ui.bootstrap') < 0) {
            library.push('ui.bootstrap');
        }
        if (library.indexOf('bootstrapLightbox') < 0) {
            library.push('bootstrapLightbox');
        }
        if (library.indexOf('signalR.eventAggregator') < 0) {
            library.push('signalR.eventAggregator');

        }
        if (library.indexOf('TranslationModule') < 0) {
            library.push('TranslationModule');

        }
        if (library.indexOf('highcharts-ng') < 0) {
            library.push('highcharts-ng');
        }
        if (library.indexOf('regit.ui') < 0) {
            library.push('regit.ui');
        }
        if (library.indexOf('notifications') < 0) {
            library.push('notifications');
        }
        if (library.indexOf('mgcrea.ngStrap') < 0) {
            library.push('mgcrea.ngStrap');
        }
        if (library.indexOf('angular-momentjs') < 0) {
            library.push('angular-momentjs');
        }
        if (library.indexOf('campaigns') < 0) {
            library.push('campaigns');
        }
        // if (library.indexOf('interaction') < 0) {
        //     library.push('interaction');
        // }

        if (library.indexOf('calendar') < 0) {
            library.push('calendar');
        }

        if (library.indexOf('defs') < 0) {
            library.push('defs');
        }
        if (library.indexOf('location') < 0) {
            library.push('location');
        }
        if (library.indexOf('ngSanitize') < 0) {
            library.push('ngSanitize');
        }
        //if (library.indexOf('ngAnimate') < 0) {
        //    library.push('ngAnimate');
        //}
        if (library.indexOf('vault') < 0) {
            library.push('vault');
        }

        if (library.indexOf('ui.select') < 0) {
            library.push('ui.select');
        }

        if (library.indexOf('windows') < 0) {
            library.push('windows');
        }

        if (library.indexOf('msg') < 0) {
            library.push('msg');
        }


        if (library.indexOf('defs') < 0) {
            library.push('defs');
        }

        if (library.indexOf('ngMessages') < 0) {
            library.push('ngMessages');
        }

        if (library.indexOf('analytics') < 0) {
            library.push('analytics');
        }
        if (library.indexOf('angular-jqcloud') < 0) {
            library.push('angular-jqcloud');
        }
        if (library.indexOf('signalR.eventAggregator') < 0) {
            library.push('signalR.eventAggregator');
        }

        if (library.indexOf('ja.qr') < 0) {
            library.push('ja.qr');


        }
        if (library.indexOf('ngclipboard') < 0) {
            library.push('ngclipboard');
        }
        if (library.indexOf('CommonDirectives') < 0) {
            library.push('CommonDirectives');
        }
        if (library.indexOf('ngResource') < 0) {
            library.push('ngResource');
        }

        if (library.indexOf('BusinessAccountModule') < 0) {
            library.push('BusinessAccountModule');
        }

        if (library.indexOf('DataModule') < 0) {
            library.push('DataModule');
        }

        if (library.indexOf('users') < 0) {
            library.push('users');
        }
        // if (library.indexOf('interaction') < 0) {
        //     library.push('interaction');
        // }
        // if (library.indexOf('social') < 0) {
        //     library.push('social');
        // }


        if (qudy) {
            var modules = ['ngRoute', 'UserModule', 'BusinessAccountModule', 'oitozero.ngSweetAlert', 'SocialModule', 'ui.rCalendar',
                'kendo.directives', 'NotificationModule', 'DataModule', "RegitApp"];


            library = library.concat(modules);


            //library = angular.extend(library, modules);

            //if (library.indexOf('kendo.directives') < 0) {
            //   library.push('kendo.directives');
            //}
            //if (library.indexOf('kendo.directives') < 0) {
            //   library.push('kendo.directives');
            //}
        }

        //declare the app
        var theApp = angular.module(appName, library).run(['$rootScope', function ($rootScope) {

            $rootScope.ajaxLoader = {IsLoading: false, ActiveRequest: 0, ActiveRequestShowLoader: 0};

            $rootScope.$watch(function () {
                return $rootScope.ajaxLoader.ActiveRequestShowLoader;
            }, function (newVal) {
                if (newVal > 0) {
                    $rootScope.ajaxLoader.IsLoading = true;
                } else {
                    $rootScope.ajaxLoader.IsLoading = false;
                }
            });

        }]).factory('htppBearerAuthorizationInterceptor', ['$rootScope', '$cookies', '$q', '$location', '$window', function ($rootScope, $cookies, $q, $location, $window) {
            return {
                'request': function (config) {
                    if (config.params && config.params.isExternalRequest) {
                        delete (config.params.isExternalRequest)
                    } else {
                        var accessToken = $cookies.get('access_token');
                        if (typeof accessToken != 'undefined') {
                            config.headers.Authorization = 'bearer ' + accessToken;
                        } else if (typeof externalAccessToken != 'undefined') {
                            config.headers.Authorization = 'bearer ' + externalAccessToken;
                        }
                        else {
                            window.location.href = '/User/SignIn'
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
                    if (rejection.status == 401) {
                        $window.location.href = "/User/SignUpTimeout";
                    }
                    return $q.reject(rejection);
                }
            };
        }]).config(['$httpProvider', function ($httpProvider) {
            $httpProvider.interceptors.push('htppBearerAuthorizationInterceptor');
        }]);

        if (useAjaxLoading) {
            theApp.directive('ajaxloader', ['$rootScope', '$timeout', function ($rootScope, $timeout) {
                return {
                    restrict: 'E',
                    replace: 'true',
                    template: '<div id="ajaxLoader"><div id="fountainTextG"><div id="fountainTextG_1" class="fountainTextG">L</div><div id="fountainTextG_2" class="fountainTextG">o</div><div id="fountainTextG_3" class="fountainTextG">a</div><div id="fountainTextG_4" class="fountainTextG">d</div><div id="fountainTextG_5" class="fountainTextG">i</div><div id="fountainTextG_6" class="fountainTextG">n</div><div id="fountainTextG_7" class="fountainTextG">g</div></div><div id="ajaxLoaderBackdrop"></div></div>',
                    link: function (scope, element) {
                        scope.beginningLoad = false;
                        scope.runningLoad = false;
                        var timer;
                        $rootScope.$watch(function () {
                            return $rootScope.ajaxLoader.IsLoading;
                        }, function (val) {

                            if (val) {
                                // console.log('start');
                                if (!scope.beginningLoad || !scope.runningLoad) {
                                    scope.beginningLoad = true;

                                    timer = $timeout(function () {
                                        if (!scope.beginningLoad) return;
                                        scope.beginningLoad = false;
                                        scope.runningLoad = true;
                                        $(element).addClass('active');
                                        // console.log('cover');

                                    }, 1000)
                                }
                            } else {
                                // console.log('loaded');
                                if (timer) {
                                    $timeout.cancel(timer);

                                }
                                if (scope.runningLoad) {
                                    $(element).removeClass('active');
                                }
                                scope.beginningLoad = scope.runningLoad = false;
                            }
                        });
                    }
                }
            }])
        }

        theApp.controllers = [];

        //function to construct custom angular Controller
        //controllerName is name of controller in string
        //functionbody is the handling code, functions of controller
        //custom controller define some generic functions
        theApp.getController = function (controllerName, functionbody) {
            var dependencies = [];

            if (functionbody.constructor === Array) {
                dependencies = functionbody.splice(0, functionbody.length - 1);
                functionbody = functionbody[0];
            }

            var expectParams = dependencies.length != 0 ? dependencies : angular.injector.$$annotate(functionbody);

            if (typeof theApp.controllers[controllerName] == 'undefined' || theApp.controllers[controllerName] == null) {
                theApp.controllers[controllerName] = theApp.controller(controllerName, ['$scope', '$injector', standardController]);
                return theApp.controllers[controllerName];
            } else {
                return theApp.controllers[controllerName];
            }

            function standardController($scope, $injector) {

                //inject to get services expected
                var services = [];
                for (var i = 0; i < expectParams.length; i++) {
                    //$scope need pass directly
                    if (expectParams[i] == '$scope') {
                        services.push($scope);
                    } else {
                        var dep = $injector.get(expectParams[i]);
                        services.push(dep);
                    }
                }

                //call the function body
                functionbody.apply(this, services);
            }
        }
        theApps[appName] = theApp;
        return theApp;
    } else {
        return theApps[appName];
    }

}
