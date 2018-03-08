angular.module('regitApp').controller('VaultFamilyCtrl', function ($scope, $location, rguModal) {

    $scope.goBack = function() {
        $location.path('/Vault/Categories');
    };
    $scope.closePanel = function() {
        $scope.goBack();
    };
    $scope.savePanel = function() {
        $scope.goBack();
    };


    $scope.setDefault = true;

    $scope.renderFieldValue = function (value) {
        if (!angular.isDefined(value)) return '';
        if (angular.isString(value)) return value;
        if (angular.isDate(value)) return $moment(value).format('DD MMM YYYY');
        if (angular.isArray(value)) return value.join(', ');
                return '';
    };

    $scope.familyTypes = ['Family Member', 'Pet'];
    $scope.familyType = 'Family Member';
    $scope.petGenders = ['Male', 'Female', 'Unknown'];
    $scope.petGender = 'Unknown';

    // $scope.petType = 'Dog';

    $scope.docs = [
        {
            type: 'img',
            name: 'Photo 1',
            cat: 'ID',
            uploaded: new Date(),
            fname: '1.jpg',
            url: '/Areas/regitUI/img/avatars/1.jpg'
        }, {
            type: 'img',
            name: 'Photo 2',
            cat: 'Passport',
            uploaded: new Date(),
            fname: '2.jpg',
            url: '/Areas/regitUI/img/avatars/2.jpg'
        }
    ];



});



