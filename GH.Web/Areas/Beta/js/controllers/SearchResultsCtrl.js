angular.module('myApp').controller('SearchResultsCtrl', 'CommonService', function ($scope, CommonService) {
    $scope.keyword = CommonService.GetQuerystring("query");
    $scope.avatarPath = '/Areas/Beta/img/avatars';
    var loadSize = 3;
    var loadSizeInteractions = 5;
    $scope.initIndividuals = function () {
        data.keyword = keyword;
        data.CurrentPageNumber = 1;
        data.PageSize = loadSize;
        $scope.searchResultsIndividuals = [];
        var array = [];
        $http.post('/api/SearchService/SearchMainUser', data)
               .success(function (response) {
                   $(response.results).each(function (index, userresult) {
                       array.push({
                           id: userresult.UserAcccountid,
                           firstName: userresult.FirstName,
                           middleName: '',
                           lastName: userresult.LastName,
                           email: userresult.Email,
                           desc: userresult.Description,
                           avatar: userresult.PhotoUrl,
                           network: userresult.StatusFriend
                       })
                   });
                   $scope.searchResultsIndividuals = array;
                   // deferer.resolve(response);
               })
               .error(function (errors, status) {
               });
    }

    $scope.view = {
        limitIndividuals: loadSize,
        limitBusinesses: loadSize,
        limitInteractions: loadSizeInteractions,
    }

   
    $scope.searchResultsBusinesses = [
    {
        id: 20,
        displayName: 'Qudy Solutions',
        desc: 'Best Vietnamese startup',
        avatar: '20.jpg',
        following: true
    },
        {
            id: 21,
            displayName: 'Regit Inc.',
            desc: 'Best information network',
            avatar: '21.jpg',

        },
            {
                id: 22,
                displayName: 'Total Recall',
                desc: 'Lorem Ipsum',
                avatar: '22.jpg',

            },
                {
                    id: 23,
                    displayName: 'Father & Sons Co.',
                    desc: 'Lorem Ipsum',
                    avatar: '24.jpg',

                }
    ];
    $scope.searchResultsInteractions = [];
    
    $scope.sortIndividuals = function (user) {
        if (user.network === 'trusted')
            return 1;
        if (user.network === 'normal')
            return 2;
        if (user.network === 'pending')
            return 3;
        return 4;
    };
    $scope.sortBusinesses = function (business) {
        if (business.following)
            return 1;
        return 2;
    };
    $scope.sortInteractions = function (interaction) {
        if (interaction.participated)
            return 1;
        return 2;
    };
    $scope.loadMore = function (section) {
        switch (section) {
            case 'individuals':
                $scope.view.limitIndividuals += loadSize;
                break;
            case 'businesses':
                $scope.view.limitBusinesses += loadSize;
                break;
            case 'interactions':
                $scope.view.limitInteractions += loadSizeInteractions;
                break;
        }
    }
    $scope.invite = function () {
       
    }
    $scope.inviteToNetwork = function (user) {
         if ( $scope.invitation.Id) {
            _networkService.InviteFriend({ ReceiverId: $scope.invitation.Id }).then(function () {
                var message = $rootScope
                    .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                    $scope.invitation.DisplayName;
                __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'), message, 'success');
                $scope.invitation = null;
            }, function (errors) {
                __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
            })
        } else {
            __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                $rootScope.translate('Please_Choose_Friend_Message'),
                'warning');
        }
    };
    $scope.followBusiness = function (business) {
        business.following = true;
    };
    $scope.unfollowBusiness = function (business) {
        business.following = false;
    };
    $scope.initIndividuals();
});

