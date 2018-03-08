angular.module('windows', [])
    .service('windowService', function($rootScope,$compile,$document) {
        this.openedWindowIds = [];
        this.createWindow = function (id,attr,className,css,containerSelector,content) {
            if (id in this.openedWindowIds) return;
            var html = '<window wid="' + id + '"';
            if (attr && attr.heading) {
                html += ' heading="' + attr.heading + '"';
            }
            html += '>';
            if (content) {
                html += content;
            }
            html += '</window>';
            var win = $compile(html)($rootScope);
            if (className) {
                win.addClass(className);
            }
            if (css) {
                win.css(css);
            }

            var container = $document.find(containerSelector);
            container.append(win);
            this.openedWindowIds[id] = true;
        };

        this.removeWindow = function(id) {
            if (id in this.openedWindowIds) {
                delete this.openedWindowIds[id];
            }
        }
    })
    .directive('window', function ($document,windowService) {
        function link(scope, element, attr) {

            var startX = 0, startY = 0, x = 0, y = 0;

            var header = element.find('.window-header');

            header.on('mousedown', function (e) {
                // Prevent default dragging of selected content
                e.preventDefault();
                startX = e.clientX - element.position().left;
                startY = e.clientY - element.position().top;
                header.css('cursor', 'move');
                $document.on('mousemove', mousemove);
                $document.on('mouseup', mouseup);
            });

            function mousemove(e) {
                x = e.clientX - startX;
                y = e.clientY - startY;
                element.css({ top: y, left: x});
            }

            function mouseup(e) {
                e.preventDefault();
                $document.off('mousemove', mousemove);
                $document.off('mouseup', mouseup);
                header.css('cursor', '');
            }

        }

        return {
            restrict: 'EAC',
            link: link,
            transclude: true,
            scope: {
                wid: '@',
                heading: '@',
                type: '@'
            },
            template: function (el, attr) {
                var header = '<div class="window-header">';
                if (attr.heading) {
                    header += '<div class="window-heading">' + attr.heading + '</div>';
                } else if (attr.type === 'tabs') {
                    el.addClass('tabbed');
                    header += '<ul class="tabs-nav">';
                    header += '<li ng-repeat="pane in panes" ng-class="{active:pane.selected}">'
                        + '<a href="" ng-click="select(pane)">{{pane.heading}}</a></li>';
                    header += '</ul>';
                }
                header += '<a class="window-close" href="#" ng-click="windowClose($event,wid)"></a></div>';
                header += '<div class="window-container" ng-transclude></div>';
                return header;
            },
            controller: function windowController($scope, $element) {
                var panes = $scope.panes = [];

                $scope.select = function (pane) {
                    angular.forEach(panes, function (pane) {
                        pane.selected = false;
                    });
                    pane.selected = true;
                };

                this.addPane = function (pane) {
                    if (panes.length === 0) {
                        $scope.select(pane);
                    }
                    panes.push(pane);
                };
                $scope.windowClose = function (e) {
                    e.preventDefault();
                    $element.hide();
                    windowService.removeWindow($scope.wid);
                };

                $scope.onClose = null;
                this.addOnClose = function (handler) {
                    $scope.onClose = handler;
                    // console.log($scope.onClose);
                }

            }

        };
    })

    .directive('pane', function () {
        return {
            require: '^^window',
            restrict: 'E',
            transclude: true,
            scope: {
                heading: '@'
            },
            link: function (scope, element, attrs, tabsCtrl) {
                tabsCtrl.addPane(scope);
            },
            template: '<div class="tab-pane" ng-show="selected" ng-transclude>'
            + '</div>'
        };
    });


