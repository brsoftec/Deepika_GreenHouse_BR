angular.module('regitBusiness')
    .controller('InteractionPreviewCtrl', function ($scope, workingForm) {
        $scope.interaction = $scope.campaign;
        $scope.fields = workingForm.fields;
        $scope.filterRequired = function (field) {
            return !field.optional && !field.membership && !field.qa;
        };
        $scope.filterOptional = function (field) {
            return !!field.optional;
        };
        $scope.filterMembership = function (field) {
            return !!field.membership;
        };
        $scope.filterQA = function (field) {
            return !!field.qa;
        };
    });