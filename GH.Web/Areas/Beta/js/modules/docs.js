angular.module('docs', ['regit.ui'])

    .factory('docsService', function ($http, $q) {
        var docs = [
            {
                type: 'doc',
                name: 'Document 1',
                cat: 'Resume',
                uploaded: new Date(),
                url: 'resume.doc'
            }, {
                type: 'img',
                name: 'Photo 1',
                cat: 'Passport',
                uploaded: new Date(),
                expiry: new Date(),
                url: '/Areas/regitUI/img/avatars/2.jpg'
            }
        ];
        for (var i = 0; i < 10; i++) {
            docs.push(angular.extend({}, docs[0]));
            docs.push(angular.extend({}, docs[1]));
        }
        return {
            getDocs: function () {
                return docs;
            }
        };
    })
    .filter('docPager', function () {
        return function (arr, start, len) {
            return (arr || []).slice(start, start+len);
        };
    });



