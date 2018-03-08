angular.module('regitBusiness').controller('InteractionManagerCtrl', function ($scope, $location, $uibModal, interactionService, interactionTypeNames) {
    $scope.totalCampaigns = 64;
    $scope.currentPage = 4;
    $scope.currentDraftPage = 2;
    $scope.boostCost = 4;

    $scope.data = {
        interactions: interactionService.getList()
    };
    $scope.campaignIndicator = interactionService.campaignIndicatorHtml;
    $scope.filterCampaignTypes = ['All Campaigns', '-', 'Advertisements', 'Registrations', 'Events', 'SRFIs'];
    $scope.filterCampaignStatuses = ['Any Status', '-', 'Active', 'Pending', 'Expired', 'Inactive'];
    $scope.selectedCampaignType = 0;
    $scope.selectedCampaignStatus = 0;
    $scope.filterCampaigns = function (campaign) {
        if (campaign.Role) return false;
        return (!$scope.selectedCampaignType || campaign.CampaignType === interactionTypeNames[$scope.selectedCampaignType-2])
            && (!$scope.selectedCampaignStatus || campaign.CampaignStatus === $scope.filterCampaignStatuses[$scope.selectedCampaignStatus].toLowerCase());
    };
    $scope.filterDrafts = function (campaign) {
        return campaign.Role === 'Draft';
    };
    $scope.filterTemplates = function (campaign) {
        return campaign.Role === 'Template';
    };
    $scope.sortCampaigns = function (campaign) {
        var sortIndex = ['active', 'pending', 'inactive', 'expired'];
        return $.inArray(campaign.CampaignStatus,sortIndex);
    };

    $scope.gotoEdit = function (campaign) {
        var path = 'EditInteraction';
        if (campaign.Role === 'Draft') {
            path = 'EditDraft'
        } else if (campaign.Role === 'Template') {
            path = 'EditTemplate'
        }
        $location.path('/Interactions/' + path + '/' + campaign.CampaignId.toString());
    };

    $scope.gotoNew = function (campaignType) {
        $location.path('/Interactions/NewInteraction/' + campaignType);
    };
    $scope.gotoNewFromTemplate = function (templateId) {
        $scope.$location.path('/Interactions/NewInteractionFromTemplate/' + templateId);
    };

    $scope.openBoost = function ($event, campaign) {
        $event.preventDefault();
        $scope.campaign = campaign;
        var modalInstance = $uibModal.open({
            templateUrl: 'modal-boost.html',
            size: 'sm',
            controller: 'ModalCtrl',
            scope: $scope
        });
    };
    $scope.submit = function () {
        $uibModalInstance.close(0);
    };
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };

});



