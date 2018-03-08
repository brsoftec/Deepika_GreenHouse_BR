var myApp = getApp("myApp", true);
// var myApp = angular.module('myApp');

myApp.controller("HelpIntroController", function ($scope, $rootScope, $http, rguModal) {

    if ($rootScope.regitResponsive) {
        return;
    }

    var business;
    if (angular.isDefined(window.regitGlobal)) {
        business = window.regitGlobal.activeAccount.accountType === 'Business';
    }

    var stateName = business ? 'hideIntroBusiness' : 'hideIntro';
    var modalName = business ? 'help.intro.business' : 'help.intro';

    rguModal.openModal(modalName, $scope);

    $scope.numSlides = business ? 4 : 7;
    $scope.slides = [];
    for (var i=0; i<$scope.numSlides; i++) {
        $scope.slides.push( { name: i.toString() });
    }

    $scope.currentSlide = 0;
    $scope.gotoSlide = function (index) {
        $('.help-intro-start').hide();
        var slides = $('.help-intro-slide');
        var pagers = $('.help-intro-paging-item');
        slides.removeClass('active');
        pagers.removeClass('active');
        $scope.currentSlide = index;
        slides.eq(index).addClass('active');
        pagers.eq(index).addClass('active');
        $('.help-intro-nav').show();
    };

    $scope.closeIntro = function (hideFunc) {

        if (angular.isFunction(hideFunc)) {
            hideFunc();
        }
        $http.post('/Api/AccountSettings/ViewPreference', {
            prefName: business ? 'ShowBusinessIntroSlides' : 'ShowIntroSlides',
            prefValue: business ? $scope.view.preferences.showBusinessIntroSlides : $scope.view.preferences.showIntroSlides
        }).success(function() {
        });
    }

});