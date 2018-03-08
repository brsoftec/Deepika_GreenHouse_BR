angular.module('regitApp')
    .controller('InteractionPublicCtrl', function ($scope, $routeParams, campaignService) {
        $scope.user.anonymous = true;

        var campaign = campaignService.getCampaignById($routeParams.campaignId);
        if (campaign) {
            $scope.campaign = angular.copy(campaign);
        }
    });