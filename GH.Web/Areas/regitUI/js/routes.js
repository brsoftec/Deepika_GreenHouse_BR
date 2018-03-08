angular.module('regitApp')
    .config(['$routeProvider', '$locationProvider',
        function ($routeProvider, $locationProvider) {
            $routeProvider
                .when('/', {
                    templateUrl: 'views/individual-homepage.html'
                    // controller: 'IndexCtrl',
                    // controllerAs: 'index'
                })
                .when('/Calendar', {
                    templateUrl: 'views/calendar.html',
                    controller: 'CalendarCtrl'
                })
                .when('/Calendar/Event/New', {
                    templateUrl: 'views/calendar-event-editor.html',
                    controller: 'EventEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'new'
                        }
                    }
                })
                .when('/Calendar/Event/Edit/:eventId', {
                    templateUrl: 'views/calendar-event-editor.html',
                    controller: 'EventEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Vault/', {
                    templateUrl: 'views/vault.html',
                    controller: 'VaultManagerCtrl'
                })
                .when('/Vault/Categories', {
                    templateUrl: 'views/vault-categories.html',
                    controller: 'VaultManagerCtrl'
                })
                .when('/Vault/Address', {
                    templateUrl: 'views/vault-address.html',
                    controller: 'VaultAddFormCtrl'
                })
                .when('/Campaigns/Manage', {
                    templateUrl: 'views/campaign-module.html',
                    controller: 'CampaignManagerCtrl'
                })

                .when('/Campaigns/NewCampaign/:campaignType', {
                    templateUrl: 'views/campaign-editor.html',
                    controller: 'CampaignEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'new';
                        }
                    }
                })
                .when('/Campaigns/NewCampaignFromTemplate/:templateId', {
                    templateUrl: 'views/campaign-editor.html',
                    controller: 'CampaignEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'newFromTemplate';
                        }
                    }
                })
                .when('/Campaigns/EditCampaign/:campaignId', {
                    templateUrl: 'views/campaign-editor.html',
                    controller: 'CampaignEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Campaigns/EditDraft/:campaignId', {
                    templateUrl: 'views/campaign-editor.html',
                    controller: 'CampaignEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                })
                .when('/Campaigns/EditTemplate/:campaignId', {
                    templateUrl: 'views/campaign-editor.html',
                    controller: 'CampaignEditorCtrl',
                    resolve: {
                        action: function () {
                            return 'edit'
                        }
                    }
                });
            $locationProvider.html5Mode(false);
        }]);
