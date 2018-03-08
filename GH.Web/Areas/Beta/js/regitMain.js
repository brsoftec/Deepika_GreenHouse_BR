var RegitTemplatePath = '/Areas/Beta/templates';
var RegitViewPath = '/Areas/Beta/uiviews/';
var myApp = getApp('myApp');

angular.module('regitMain', ['ngRoute', 'ngSanitize', 'ui.bootstrap', 'mgcrea.ngStrap', 'ui.select', 'oitozero.ngSweetAlert', 'internationalPhoneNumber', 'myApp', 'CommonDirectives', 'UserModule', 'DataModule', 'TranslationModule', 'BusinessAccountModule', 'defs', 'regit.ui', 'calendar', 'msg'])

    .controller('MainCtrl', function ($scope) {
        // angular.bootstrap(document.querySelector('.msg-app'), ['msgApp']);
        $scope.Confirm = false;

        $scope.closeModule = function (event) {
            var module = angular.element(event.target).closest('.module');
            module.hide();
        };
        $scope.currentView = {
            showingUserMenu: false,
            showingNotifications: false
        };
        $scope.user = {
            fullName: 'Quay Trong Do',
            business: false,
            avatar: '/Areas/regitUI/img/avatars/2.jpg'
        };
        $scope.vault = {
            strength: 40
        };
        $scope.openMainUserMenu = function (event) {
            event.preventDefault();
        };

        $scope.notifications = [
            {
                type: 'expiry',
                desc: 'Your passport will expire in 5 days.',
                time: '1 minute ago'
            },
            {
                type: 'expiry',
                desc: 'Your credit card Credit Card Name expired 1 week ago.',
                time: 'Yesterday'
            },
            {
                type: 'messaging',
                desc: 'Sent message to Superman',
                time: 'Just now'
            },
            {
                type: 'registration',
                desc: 'Registered to Golden Hair Saloon',
                time: '10 mins ago'
            },
            {
                type: 'delegation',
                desc: 'You have received delegation from Jennifer Pham',
                time: '25 mins ago'
            },
            {
                type: 'system',
                desc: 'User logged in',
                time: '1 hour ago'
            },
            {
                type: 'event',
                desc: 'Joined XXX Grand Opening event',
                time: 'June 27'
            },
            {
                type: 'follow',
                desc: 'Followed Business A',
                time: 'June 4'
            }
        ];
        $scope.whenNotification = function (notification) {
            return notification.time;
        };
        $scope.gotoNotifications = function () {
            $scope.currentView.showingNotifications = false;
        };
        $scope.feedLoadMore = function() {

        };

    })
    .controller('ModalCtrl', function ($scope, $uibModalInstance) {
        $scope.submit = function () {
            $uibModalInstance.close(0);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    });


