
angular.module('regitPublic', ['oitozero.ngSweetAlert', 'ngCookies',
    'TranslationModule', 'UserModule', 'DataModule', 'ngSanitize', 'regit.ui'])
.config(function() {
});

$('body').append('<ajaxLoader></ajaxLoader>');

