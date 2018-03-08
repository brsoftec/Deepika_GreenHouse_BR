var myApp = getApp("myApp", true);

myApp.config(function ($locationProvider) {
    $locationProvider.hashPrefix('!');
    //$locationProvider.html5Mode({
    //    enabled: true,
    //    requireBase: false
    //});
});


myApp.getController('NetworkController', function ($scope, $location, $anchorScroll, rguView) {


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
        $scope.openAccordion('personal');
        $scope.openAccordion('business');
    }
    $scope.openAccordion(hash);

    $anchorScroll();

});
