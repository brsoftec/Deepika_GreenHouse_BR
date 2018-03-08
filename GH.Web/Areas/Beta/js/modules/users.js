angular.module('users', [])
.factory('userService', function() {
   return {
       getCurrentUser: function() {
           return {
               id: '00',
               name: 'Son Nguyen',
               avatar: '/Areas/regitUI/img/avatars/0.jpg',
               online: true
           }
       },
       getUsers: function(network) {
            // Get users from network
           //   See fetchUsers() from msgService
       }
   }
});