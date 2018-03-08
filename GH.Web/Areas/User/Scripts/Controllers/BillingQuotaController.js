var myApp = getApp("myApp", true);

myApp.getController('BillingQuotaController', function ($scope, billingService) {

    $scope.updateBillingUsage = function(usage) {
        console.log(usage);
        if (usage.hasOwnProperty('workflowMembers')) {
            $scope.billingInfo.workflowMembers = usage.workflowMembers;
            $scope.billing.reachedWorkflowMembers = usage.workflowMembers >= $scope.billingInfo.maxWorkflowMembers;
        }
        if (usage.hasOwnProperty('syncMembers')) {
            $scope.billingInfo.syncMembers = usage.syncMembers;
            $scope.billing.reachedSyncMembers = usage.syncMembers >= $scope.billingInfo.maxSyncMembers;
        }
    };

    var billingInfo = $scope.billingInfo = billingService.getBillingInfo();
    if (!billingInfo) return;
    $scope.billing = {
        plan: billingInfo.plan,
        planFree: billingInfo.plan.name==='free',
        reachedWorkflowMembers: billingInfo.workflowMembers >= billingInfo.maxWorkflowMembers,
        reachedSyncMembers: billingInfo.syncMembers >= billingInfo.maxSyncMembers
    }

});
