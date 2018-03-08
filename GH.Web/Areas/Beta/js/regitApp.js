
angular.module('regitApp', ['regitMain', 'vault', 'docs'])
    .config(
        function ($routeProvider) {
            $routeProvider
                .when('/', {
                    templateUrl: RegitViewPath + 'individual-homepage.html'

                })
                .when('/Notifications', {
                    templateUrl: 'views/notifications.html'
                })
                .when('/ActivityLog', {
                    templateUrl: 'views/activity-log.html',
                    controller: 'ActivityCtrl'
                })
                .when('/Account', {
                    templateUrl: 'views/account.html',
                    controller: 'AccountCtrl'
                })
                .when('/BusinessPage', {
                    templateUrl: 'views/business-page.html',
                    controller: 'BusinessPageCtrl'
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
                .when('/Vault/Delegation', {
                    templateUrl: 'views/delegation.html',
                    controller: 'DelegationCtrl'
                })
                .when('/Vault/Address', {
                    templateUrl: 'views/vault-address.html',
                    controller: 'VaultAddFormCtrl'
                })
                .when('/Vault/Other', {
                    templateUrl: 'views/vault-other.html',
                    controller: 'VaultAddFormCtrl'
                })
                .when('/Vault/Family', {
                    templateUrl: 'views/vault-family.html',
                    controller: 'VaultFamilyCtrl'
                })
                .when('/Vault/Documents', {
                    templateUrl: 'views/vault-documents.html',
                    controller: 'VaultDocumentsCtrl'
                });

        })

    
