angular.module('regitApp').controller('BusinessPageCtrl', function ($scope) {

    $scope.business = {
        followed: false,
        followers: 2300,
        fullName: 'Business #01',
        avatar: 'img/business-avatar.jpg'
    };

    $scope.srfis = [
        {
            label: 'SRFI Form 1'
        },        {
            label: 'SRFI Form 2'
        },        {
            label: 'SRFI Form 3'
        },        {
            label: 'SRFI Form 4'
        }
    ];

    $scope.follow = function () {

    };
    $scope.selectSRFI = function (srfi) {
        $scope.showingSRFISelect = false;
    };


});