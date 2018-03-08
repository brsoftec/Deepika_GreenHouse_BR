angular.module('regitApp').controller('VaultAddFormCtrl', function ($scope, $location) {

    $scope.goBack = function() {
        $location.path('/Vault/Categories');
    };
    $scope.closePanel = function() {
        $scope.goBack();
    };
    $scope.savePanel = function() {
        console.log($scope.location, $scope.setDefault, $scope.from, $scope.to);
        $scope.goBack();
    };
    $scope.addressTypes = [ 'Temporary address', 'Delivery address', 'Mailing address', 'P.O. Box'];
    $scope.addressType = 'Address';
    $scope.coords = {latitude:0, longitude:0};

    $scope.otherTypes = [ 'Body', 'Favorites', 'Preferences'];
    $scope.otherType = 'Favorites';
    $scope.seats= ['No preference', 'Window', 'Aisle', 'Exit'];
    $scope.seat = 'No preference';

    $scope.pref = { interests: '', food: [] };
    $scope.body = {
        eyeColor: [], hairColor: []
    };
    $scope.bodyTypes = ['Average','Slim','Athletic','Obese'];

    $scope.location = {country: null, region:null,  city: null, zipCode: null};
    $scope.from = new Date();
    $scope.to = new Date();

    $scope.setDefault = true;

    $scope.renderFieldValue = function (value) {
        if (!angular.isDefined(value)) return '';
        if (angular.isString(value)) return value;
        if (angular.isDate(value)) return $moment(value).format('DD MMM YYYY');
        if (angular.isArray(value)) return value.join(', ');
                return '';
    };

});



