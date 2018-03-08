angular.module('regitBusiness').controller('InteractionEditorCtrl', function ($scope, $location, $routeParams, action, interactionService, interactionTypeAbbrs, $uibModal) {
    $scope.budget = 0;
    $scope.unitCost = 1;
    $scope.flashCost = 10;

    $scope.totalUsers = 1000;
    $scope.targetUsers = 0;
    $scope.reachPercentage = 0;



    $scope.close = function () {
        $location.path('/Interactions/Manage');
    };

    switch (action) {
        case 'new':
            var type = $routeParams.campaignType;
            $scope.campaign = interactionService.getBlankCampaign(type);
            $scope.pageTitle = 'New Interaction - ' + type;
            $scope.campaign.campaignId = Date.now().toString();
            $scope.campaign.UserPay = 'Free';
            break;
        case 'newFromTemplate':
            var templateId = $routeParams.templateId;
            var campaign = interactionService.getCampaignById(templateId);
            if (!campaign) {
                $scope.close();
                break;
            }
            campaign = angular.copy(campaign);
            campaign.campaignId = Date.now().toString();
            campaign.Role = false;
            campaign.Description = '';
            $scope.pageTitle = 'New Interaction: ' + campaign.CampaignName;
            campaign.UserPay = 'Free';
            $scope.campaign = campaign;
            break;
        case 'edit':
            campaign = interactionService.getCampaignById($routeParams.campaignId);
            if (!campaign) {
                $scope.close();
                return;
            }
            campaign = angular.copy(campaign);
            $scope.campaign = campaign;
            $scope.pageTitle = 'Edit ' + (campaign.Role || 'Interaction') + ': ' + campaign.CampaignName;
            break;
    }
    $scope.campaign.GlobalComment = 'Sample comment';
    $scope.campaign.location = {
        country: 'Viet Nam',
        region: [],
        city: 'Bac Giang'
    };
    $scope.campaign.keywords = '';
    $scope.campaign.EventStartDate = new Date();
    $scope.type = $scope.campaign.CampaignType;
    $scope.campaign.PublicURL = 'http://dev.regitsocial.com/interaction/' + $scope.type.toLowerCase() + '/' + $scope.campaign.campaignId;
    $scope.abbr = type === 'Advertising' ? 'ad' : (type === 'Registration' ? 'reg' : 'evt');

    if (type === 'Advertising') {
        $scope.steps = [1, 2, 3];
    } else {
        $scope.steps = [1, 2, 3, 4];
    }
    $scope.step = 1;
    $scope.onKeywordsChange = function () {
        console.log($scope.campaign.keywords);
    };
    $scope.cancelAndClose = function () {
        delete $scope.campaign;
        $scope.close();
    };

    $scope.gotoNext = function () {
        $scope.step++;
    };
    $scope.gotoPrev = function () {
        $scope.step--;
    };
    $scope.saveCampaignAndClose = function () {
        if (!$scope.campaign.Role) {
            $scope.campaign.CampaignStatus = 'Pending';
        }
        interactionService.saveCampaign($scope.campaign, function (result) {
            //  Check result . . .
            $scope.close();
        })
    };
    $scope.saveCampaignAndGotoPublic = function () {
        workingInteraction = angular.extend({}, $scope.campaign);
        interactionService.saveCampaign($scope.campaign, function (result) {
            //  Check result . . .
            $location.path('/Interactions/Public/' + $scope.campaign.CampaignId);
        });

    };
    $scope.saveAsDraft = function () {
        $scope.campaign.Role = 'Draft';
        $scope.saveCampaignAndClose();
    };
    $scope.saveFromDraft = function () {
        $scope.campaign.Role = false;
        $scope.pageTitle = 'Edit Interaction: ' + $scope.campaign.CampaignName;
    };
    $scope.saveAsTemplate = function () {
        $scope.campaign.Role = 'Template';
        $scope.pageTitle = 'Edit Template: ' + $scope.campaign.CampaignName;
        $scope.showTemplates = true;
    };

    $scope.onBudgetChange = function () {
        var budget = $scope.budget;
    };
    $scope.onCostChange = function () {
    };

    $scope.openReg = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'modal-feed-open-reg.html',
            size: 'lg',
            controller: 'ModalCtrl',
            scope: $scope
        });
    };

    $scope.exportQR = function (event) {
        var canvas = document.querySelector('qr canvas');
        // Canvas2Image.saveAsPNG(canvas, 400, 400);
        var link = event.target;
        link.href = canvas.toDataURL();
        link.download = 'QR-' + $scope.campaign.CampaignId + '.png';
    };

    // new Clipboard('.btn-copy-url');

});