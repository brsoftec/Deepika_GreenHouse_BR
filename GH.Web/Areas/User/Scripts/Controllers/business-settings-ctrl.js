var myApp = getApp("myApp", true);//['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'DataModule'], true);

myApp.config(function ($locationProvider) {
    $locationProvider.hashPrefix('!');
    //$locationProvider.html5Mode({
    //    enabled: true,
    //    requireBase: false
    //});
});
myApp.getController('BusinessAccountSettingsController', function ($scope, $location, $anchorScroll) {

    $scope.openAccordion = function (name) {
        $scope.accordionsOpen[name] = true;
    };

    $scope.toggleAccordion = function (name) {
        if (name in $scope.accordionsOpen) {
            $scope.accordionsOpen[name] = !$scope.accordionsOpen[name];
        } else {
            $scope.accordionsOpen[name] = true;
        }
    };
    $scope.accordionsOpen = { };

    var hash = $location.hash();
    if (!hash.length) {
        hash = 'customers';
    }
    $scope.openAccordion(hash);

    $anchorScroll();
});


myApp.getController("ViewPreferencesController", function ($scope, $http) {

    $scope.settings = $scope.settings || {};
    $scope.settings.viewprefs = regitGlobal.viewPreferences || {
            showIntroSlides: false,
            showBusinessIntroSlides: false
        };

    $scope.updateViewPreference = function (pref) {
        var prefName = undefined;
        var prefValue = undefined;
        switch (pref) {
            case 'introSlides':
                prefName = 'ShowIntroSlides';
                prefValue = $scope.settings.viewprefs.showIntroSlides;
                break;
            case 'businessIntroSlides':
                prefName = 'ShowBusinessIntroSlides';
                prefValue = $scope.settings.viewprefs.showBusinessIntroSlides;
                break;
        }

        if (prefName) {
            $http.post('/Api/AccountSettings/ViewPreference', {
                prefName: prefName, prefValue: prefValue
            }).success(function () {
            });
        }
    }

});
