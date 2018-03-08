﻿angular.module('social', ['interaction'])
    .constant('socialProviderAliases', ['fb', 'tw', 'gg', 'li'])
    .constant('socialProviderNames', ['facebook', 'twitter', 'google', 'linkedin'])
    .factory('socialService', function () {
        return {
            openShare: function (sharer) {
            }
        };

    })

    .directive('socialAction', function ($rootScope, $window) {

        return {
            restrict: 'EA',
            replace: true,
            scope: {},
            templateUrl: '/Areas/regitUI/templates/social-action.html?v=1',
            link: function (scope, elem, attrs) {
                var provider = scope.provider = attrs.provider;
                scope.openShare = function () {
                    var width = 800;
                    var height = 500;
                    var top = ($window.innerHeight - height) / 2;
                    var left = ($window.innerWidth - width) / 2;
                    switch (provider) {
                        case 'fb':
                            $window.open(
                                'https://www.facebook.com/sharer/sharer.php?u=' + encodeURIComponent(attrs.url || $window.location.href)
                                , 'facebook-share', 'toolbar=0,status=0,resizable=yes,width=' + width + ', height=' + height
                                + ',top=' + top + ',left=' + left);
                            break;
                        case 'tw':
                            $window.open(
                                'https://www.twitter.com/intent/tweet?url=' + encodeURIComponent(attrs.url || $window.location.href)
                                , 'twitter-share', 'toolbar=0,status=0,resizable=yes,width=' + width + ', height=' + height
                                + ',top=' + top + ',left=' + left);
                            break;
                        case 'gg':
                            $window.open(
                                'https://plus.google.com/share?url=' + encodeURIComponent(attrs.url || $window.location.href)
                                , 'google-share', 'toolbar=0,status=0,resizable=yes,width=' + width + ', height=' + height
                                + ',top=' + top + ',left=' + left);
                            break;
                        case 'li':
                            $window.open(
                                'https://www.linkedin.com/shareArticle?mini=true&url=' + encodeURIComponent(attrs.url || $window.location.href)
                                , 'linkedin-share', 'toolbar=0,status=0,resizable=yes,width=' + width + ', height=' + height
                                + ',top=' + top + ',left=' + left);
                            break;
                    }
                };

            }
        };
    })
    .directive('socialActions', function (socialProviderAliases, interactionService) {
        return {
            restrict: 'EA',
            scope: {
                interaction: '<'
            },
            templateUrl: '/Areas/regitUI/templates/social-actions.html?v=2',
            link: function (scope, elem, attrs) {
               scope.publicUrl = interactionService.publicUrl(scope.interaction);
               scope.sharing = true;
               scope.view = {
                   openingSocialLink: false
               };
                var providers = [];
                var socialShare = scope.interaction.socialShare;
                if (!socialShare) {
                    scope.sharing = false;
                } else if (socialShare === 'all') {
                    providers = socialProviderAliases;
                } else {
                    providers = socialShare.split(',') ;
                }
                scope.sharers = [];
                providers.forEach(function (provider) {
                    scope.sharers.push(
                        {
                            provider: provider,
                            url: scope.publicUrl
                        }
                    )
                });

                scope.openSocialLink = function() {
                    scope.view.openingSocialLink = true;
                };
                scope.closeSocialLink = function() {
                    scope.view.openingSocialLink = false;
                };
                scope.toggleSocialLink = function() {
                    scope.view.openingSocialLink = !scope.view.openingSocialLink;
                };
            }
        };
    })
    .directive('socialProviderSelect', function (socialProviderAliases) {

        return {
            restrict: 'EA',
            replace: true,
            scope: {
                ngModel: '=',
                mode: '@?'
            },
            templateUrl: '/Areas/regitUI/templates/social-provider-select.html?v=3',
            link: function (scope, elem, attrs) {
                scope.providerNames = socialProviderAliases;
                scope.providers = [];
                scope.providerNames.forEach(function(name) {
                    scope.providers[name] = scope.ngModel==='all' || scope.ngModel.indexOf(name) >= 0;
                });
                function updateModel() {
                    var providers = [];
                    scope.providerNames.forEach(function(name) {
                        if (scope.providers[name]) {
                            providers.push(name);
                        }
                    });
                    scope.ngModel = providers.length===4 ? 'all' : providers.join(',');
                }
                scope.toggleProvider = function(provider) {
                    scope.providers[provider] = !scope.providers[provider];
                    updateModel();
                }
            }
        };
    });

