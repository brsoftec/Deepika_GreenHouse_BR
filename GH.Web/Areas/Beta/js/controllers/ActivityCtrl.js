angular.module('regitMain').controller('ActivityCtrl', function ($scope) {

    $scope.activities = [
        {
            type: 'interaction',
            desc: 'You joined Big Opening Event',
            time: '5 minutes ago'
        },

        {
            type: 'vault-edit',
            desc: 'You modified vault',
            time: '30 minutes ago'
        },
        {
            type: 'delegation',
            desc: 'You received delegation from Jennifer Pham',
            time: '55 minutes ago'
        },
        {
            type: 'login',
            desc: 'You logged in Regit',
            time: '1 hour ago'
        },
        {
            type: 'logout',
            desc: 'You logged out Regit',
            time: 'Yesterday'
        }
    ];


});