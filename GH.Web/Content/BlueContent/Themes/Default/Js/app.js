var appController = function ($scope) {
    $scope.demoDate = new Date();
    $scope.dateFormat = 'mm-dd-yyyy';
    $scope.dateOptions = {showWeeks: false};
    $scope.datePicker = {opened: false};
    $scope.openDatePicker = function () {
        $scope.datePicker.opened = true;
    };
    $scope.genderModel = 'All';
    $scope.popoverNotifications = {
        content: 'Hello, World!',
        templateUrl: 'popover.notifications.html',
        title: 'Notifications'
    };
    $scope.openNotifications = function ($event) {
        $event.preventDefault();

    };

};
var app = angular.module('app', ['ui.bootstrap']);
var gridApp = angular.module('gridApp', ['ui.bootstrap', 'ui.grid', 'ui.grid.edit', 'ui.grid.rowEdit']);
var formApp = angular.module('formApp', ['ui.bootstrap', 'ang-drag-drop']);
app.controller('AppCtrl', appController);
app.controller('NotificationsCtrl', function ($scope) {
    $scope.notifications = [
        { type: 'messaging', desc: 'Sent message to Superman', date: 'Just now' },
        { type: 'registration', desc: 'Registered to Golden Hair Saloon', date: '10 mins ago' },
        { type: 'delegation', desc: 'You have received delegation from Jennifer Pham', date: '25 mins ago'},
        { type: 'system', desc: 'User logged in', date: '1 hour ago' },
        { type: 'event', desc: 'Joined XXX Grand Opening event', date: 'June 27' },
        { type: 'follow', desc: 'Followed Business A', date: 'June 4' }
    ];
});

gridApp.controller('AppCtrl', appController);
formApp.controller('AppCtrl', appController);
gridApp.controller('GridCtrl', function ($scope) {
    $scope.familyCellClass = function (grid, row, col, rowRenderIndex, colRenderIndex) {
        return 'lastfield row' + rowRenderIndex;
    };
    //  Family Members Page
    $scope.familyColumns = [
        {
            displayName: 'First Name',
            field: 'fname',
            name: 'fname'
        },
        {
            displayName: 'Middle Name',
            field: 'mname'
        },
        {
            displayName: 'Last Name',
            field: 'lname'
        },
        {
            displayName: 'DOB',
            field: 'dob',
            type: 'date'
        },
        {
            displayName: 'Relationship',
            field: 'rel',
            cellClass: $scope.familyCellClass
        }
    ];
    $scope.familyMembers = [{
        fname: 'Quay',
        mname: '',
        lname: 'Do',
        dob: new Date(),
        rel: 'Boss'
    }, {
        fname: 'Tung',
        mname: 'Thanh',
        lname: 'Nguyen',
        dob: new Date(),
        rel: 'Brother'
    }
    ];

    $scope.familyGridOptions = {
        columnDefs: $scope.familyColumns,
        data: $scope.familyMembers

    };
    $scope.addFamilyMember = function (e) {
        e.preventDefault();
        $scope.familyMembers.push({
            fname: "New"
        });
    };
    $scope.removeFamilyMember = function () {
        //if($scope.gridOpts.data.length > 0){
        $scope.familyMembers.splice(0, 1);
        //}
    };

});

app.controller('AdCtrl', function ($scope) {
    $scope.showingEndDate = false;
});
app.controller('RegCtrl', function ($scope) {
    $scope.showingEndDate = true;
});
app.controller('CampaignsCtrl', function ($scope) {
    $scope.actionHandler = function (e, action) {
        e.preventDefault();
    };
    $scope.totalCampaigns = 64;
    $scope.currentPage = 4;
    $scope.currentDraftPage = 2;
});
app.controller('FeedCtrl', function ($scope, $uibModal) {
    $scope.openReg = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'modal-feed-open-reg.html',
            size: 'lg',
            controller: 'ModalCtrl'
        });

    };
});
app.controller('ModalCtrl', function ($scope, $uibModalInstance) {
    $scope.submit = function () {
        $uibModalInstance.close(0);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});
formApp.controller('FormCtrl', function ($scope) {
    $scope.vault =
    {
        personal: {
            personalId: [
                {
                    label: 'Title',
                    type: 'option'
                },
                {
                    label: 'First Name',
                    type: 'text'
                },
                {
                    label: 'Last Name',
                    type: 'text'
                },
                {
                    label: 'Alias',
                    type: 'text'
                }
            ],
            bodyMetrics: [
                {
                    label: 'Hair Color',
                    type: 'text'
                },
                {
                    label: 'Eye Color',
                    type: 'text'
                },
                {
                    label: 'Height',
                    type: 'number'
                },
                {
                    label: 'Weight',
                    type: 'number'
                },
                {
                    label: 'Body Type',
                    type: 'list'
                }
            ]
        },
        professional: {
            currentEmployment: [
                {
                    label: 'Company Name',
                    type: 'text'
                },
                {
                    label: 'Title',
                    type: 'text'
                },
                {
                    label: 'Email',
                    type: 'email'
                },
                {
                    label: 'Phone Number',
                    type: 'text'
                },
                {
                    label: 'Address',
                    type: 'text'
                }
            ]

        }

    };

    $scope.formFields = [ ];

    $scope.addText = "";

    $scope.deleteField = function (index, array) {
        if (array === $scope.formFields) {
            array.splice(index, 1);
        }
    };

    $scope.onDrop = function ($event, $data, array) {
        if (array === $scope.formFields) {
            array.push($data);
        }
    };

});
app.controller('DelegationCtrl', function ($scope, $uibModal) {
    $scope.showingEndDate = true;
    $scope.toTitleCase = function(str)
    {
        return str.replace(/\w\S*/g, function(txt){return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();});
    };
    $scope.delegatees = [
        { name: 'Jennifer Pham', role: 'super', expires: 'indefinite', status: 'active' },
        { name: 'Andy Roberto', role: 'secretary', expires: 'indefinite', status: 'active' },
        { name: 'Maria Ginza', role: 'normal', expires: '2 July 2016', status: 'inactive' },
        { name: 'Superman', role: 'custom', expires: '12 July 2016', status: 'pending' },
        { name: 'Hugo Lina', role: 'normal', expires: '30 Jan 2016', status: 'expired' }
    ];
    $scope.openDelegate = function ($event,action,status) {
        $event.preventDefault();
        $scope.currentDelegationStatus = status;
        var modalInstance = $uibModal.open({
            templateUrl: 'modal-delegate-' + action + '.html',
            size: 'md',
            controller: 'ModalCtrl',
            scope: $scope
        });
    };
    $scope.openDelete = function ($event,type) {
        $event.preventDefault();
        var modalInstance = $uibModal.open({
            templateUrl: type === 'delegatee' ? 'modal-delegatee-delete.html':'modal-delegator-delete.html',
            size: 'sm',
            controller: 'ModalCtrl'
        });
    };
    $scope.submit = function () {
        $uibModalInstance.close(0);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

app.controller('ActivityCtrl', function ($scope) {
    $scope.activities = [
        { desc: 'Send message to Superman', date: 'Just now' },
        { desc: 'Register to Golden Hair Saloon', date: '10 mins ago' },
        { desc: 'Accept delegation from Jennifer Pham', date: '25 mins ago' },
        { desc: 'Log in', date: '1 hour ago' },
        { desc: 'Join XXX Grand Opening event', date: 'June 27' },
        { desc: 'Follow Business A', date: 'June 4' }
    ];
});


