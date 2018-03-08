(function() {
    var app;
    var regitGlobal = regitGlobal || {};
    if (typeof(regitGlobal !== 'undefined') && regitGlobal.hasOwnProperty('ngApp') && regitGlobal.ngApp.length) {
        app = angular.module(regitGlobal.ngApp);
    } else {
        try {
            app = angular.module("regitAbout");
        } catch (err) {
            try {
                app = angular.module("regitSignup");
            } catch (err) {
                try {
                    app = angular.module("regitPublic");
                } catch (err) {
                    try {
                        app = angular.module("myApp");
                    } catch (err) {
                        app = angular.module('myApp', []);
                    }
                }
            }

        }
    }

    app.directive('body', function ($window, $document) {

        return {
            restrict: 'E',
            link: function (scope) {

                scope.regitGlobal = $window.regitGlobal || {};

                function onClick(e) {
                    scope.$broadcast('document::click', {event: e});
                }

                function onResize(e) {
                    scope.$broadcast('window::resize', {event: e});
                }

                function onContextMenu(e) {
                    scope.$broadcast('document::contextmenu', {event: e});
                }

                angular.element($window).on('resize', onResize);
                $document.on('click', onClick);
                $document.on('contextmenu', onContextMenu);
                scope.$on('$destroy', function () {
                    $document.off('click', onClick);
                    angular.element($window).off('resize', onResize);
                    $document.off('contextmenu', onContextMenu);
                });
            }
        };
    })

        .directive('responsiveHeader', function ($rootScope) {
            return {
                restrict: 'AC',
                link: function (scope, elem, attrs) {
                    function responsiveUpdate() {
                        var width = $(window).width(), height = $(window).height();
                        if (width <= 840) {
                            $rootScope.regitResponsive = 'mobile';
                        } else {
                            $rootScope.regitResponsive = false;
                        }
                    }

                    scope.$on('resize::resize', function () {
                        responsiveUpdate();
                    });
                    responsiveUpdate();
                    scope.openMenu = function (event) {
                        elem.find('#responsive-menu').addClass('active');
                    };
                    scope.closeMenu = function () {
                        elem.find('#responsive-menu').removeClass('active');
                    };
                    scope.openSearch = function (event) {
                        elem.find('#responsive-search').addClass('active');
                    };
                    scope.closeSearch = function () {
                        elem.find('#responsive-search').removeClass('active');
                    };

                }
            };
        });
})();




