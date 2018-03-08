angular.module('regitBusiness', ['regitMain', 'interactions', 'analytics'])
    .config(
        function ($routeProvider) {
            $routeProvider
                .when('/', {
                    templateUrl: 'views/business-homepage.html',
                    controller: 'BusinessCtrl'
                })
                .when('/Business/Account', {
                    templateUrl: 'views/business-account.html'
                })
                .when('/Business/Analytics', {
                    templateUrl: 'views/business-analytics.html',
                    controller: 'AnalyticsCtrl'
                })
                .when('/Interactions/Manage', {
                    templateUrl: 'views/interactions.html',
                    controller: 'InteractionManagerCtrl'
                })


                .when('/Interactions/NewInteraction/:campaignType', {
                    templateUrl: 'views/interaction-editor.html',
                    controller: 'InteractionEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'new';
                        }
                    }
                })
                .when('/Interactions/NewInteractionFromTemplate/:templateId', {
                    templateUrl: 'views/interaction-editor.html',
                    controller: 'InteractionEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'newFromTemplate';
                        }
                    }
                })
                .when('/Interactions/EditInteraction/:campaignId', {
                    templateUrl: 'views/interaction-editor.html',
                    controller: 'InteractionEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Interactions/EditDraft/:campaignId', {
                    templateUrl: 'views/interaction-editor.html',
                    controller: 'InteractionEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Interactions/EditTemplate/:campaignId', {
                    templateUrl: 'views/interaction-editor.html',
                    controller: 'InteractionEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Interactions/Public/:campaignId', {
                    templateUrl: 'views/interaction-public.html',
                    controller: 'InteractionPublicCtrl',
                    resolve: {
                        action: function () {
                            return 'public';
                        }
                    }
                });

        })
    .controller('BusinessCtrl', function ($scope) {
        $scope.user.fullName =  'Qudy Web Solutions';
        $scope.user.business =  true;
        $scope.user.avatar =  'img/avatar-qudy.png';
    });
