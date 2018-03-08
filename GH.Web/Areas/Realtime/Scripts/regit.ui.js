
//angular.module('regit.ui', [])

//    .directive('qdselect', function () {
//        function link(scope, element, attrs) {
//            element.wrap('<div class="select"/>');
//        }

//        return {
//            restrict: 'A',
//            link: link
//        };
//    })

//    .directive('input', function () {
//        function link(scope, element, attrs) {
//            if (attrs['type'] === 'checkbox' || attrs['type'] === 'radio') {
//                element.addClass('input');
//            }
//        }

//        return {
//            restrict: 'E',
//            link: link
//        };
//    })
//    .directive('searchbar', function () {
//        function link(scope, element, attrs) {
//        }

//        return {
//            restrict: 'EAC',
//            link: link,
//            templateUrl: 'searchbar.html'
//        };
//    })

//    .directive('input-file', function () {
//        function link(scope, element, attrs) {
//            var input = element.get(0);
//            var label = input.nextElementSibling,
//                labelVal = label.innerHTML;
//            input.addEventListener('change', function (e) {
//                var fileName = '';
//                if (this.files && this.files.length > 1)
//                    fileName = (this.getAttribute('data-multiple-caption') || '').replace('{count}', this.files.length);
//                else
//                    fileName = e.target.value.split('\\').pop();
//                if (fileName)
//                    label.innerHTML = fileName;
//                else
//                    label.innerHTML = labelVal;
//            });
//        }

//        return {
//            restrict: 'AEC',
//            scope: {},
//            link: link
//        };
//    });

