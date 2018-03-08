﻿angular.module('windows', [])
    .service('windowService', function ($rootScope, $compile, $document) {
        this.openedWindowIds = [];
        this.createWindow = function (wincls, id, attr, className, css, containerSelector, content) {
            if (id in this.openedWindowIds) return;
            var html = '<window wid="' + id + '"';
            html += ' wincls="' + wincls + '"';
            if (attr && attr.heading) {
                html += ' heading="' + attr.heading + '"';
            }
            if (attr && attr.settings) {
                html += ' settings="' + attr.settings + '"';
            }
            html += 'onclick="bringToFront()">';
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

        this.removeWindow = function (id) {
            if (id in this.openedWindowIds) {
                $('window[wid=' + id + ']').remove();
                delete this.openedWindowIds[id];
            }
        };

        this.setWindowHeading = function (wid, headingHtml) {
            var heading = $('window[wid=' + wid + '] .window-heading');
            heading.html(headingHtml);
        };
        this.setWindowHeadingIconClass = function (wid, iconClass) {
            var heading = $('window[wid=' + wid + '] .window-heading-icon');
            heading.eq(0).addClass(iconClass);
        };

        this.scroolbarend = function (wid) {
            var divcontainer = $('window[wid=' + wid + '] .window-container msg-conversation .msg-conversation');
            if (divcontainer.length > 0) {
                $(divcontainer).stop().animate({
                    scrollTop: $(divcontainer)[0].scrollHeight
                }, 500);
            }
        };

    })

    .directive('window', function ($document, windowService, msgService) {

        function link(scope, element, attr) {

            var msgbox = element.find('.msg-box');
            var header = element.find('.window-header');
            var offsetX = 0, offsetY = 0;

            var posX = 0, posY = 0, footY = 0;

            function mousedown(e) {
                e.preventDefault();
                posX = element.offset().left;
                posY = element.offset().top;
                var el = $(e.target);
                offsetX = e.clientX;
                offsetY = e.clientY;

                startX = offsetX - element.position().left;
                startY = offsetY - element.position().top;
                
                header.css('cursor', 'move');
                $document.on('mousemove', mousemove);
                $document.on('mouseup', mouseup);

                footY = $('.foot-nav').offset().top;
            }

            header.on('mousedown', mousedown);

            function mousemove(e) {

                var el = $(e.target);
                var offset = element.offset();

                x = e.clientX - startX;
                y = e.clientY - startY;
                var minX = -startX + offsetX - posX + 20;
                var minY = -startY + offsetY - posY + 70;
                var maxY = footY - 440;
                if (x < minX) {
                    x = minX;
                }
                if (y < minY) {
                    y = minY;
                } else if (y > maxY) {
                    y = maxY;
                }
                element.css({
                    top: y,
                    left: x 
                });
            }

            function cancelMouseMove() {
                $document.off('mousemove', mousemove);
                $document.off('mouseup', mouseup);
                header.css('cursor', 'default');
            }

            function mouseup(e) {
                e.preventDefault();
                cancelMouseMove();
            }

            if (attr['wincls'] === 'MsgList') {
                scope.settingsMenu = msgService.getSettingsMenu('MsgList');
            } else if (attr['wincls'] === 'MsgBox') {
                scope.settingsMenu = msgService.getSettingsMenu('MsgBox');
            } else if (attr['wincls'] === 'MsgBoxGroup') {
                scope.settingsMenu = msgService.getSettingsMenu('MsgBoxGroup');
            }

        }

        return {
            restrict: 'EAC',
            link: link,
            transclude: true,
            scope: {
                wid: '@',
                type: '@',
                heading: '@'
            },
            template: function (el, attr) {
                var header = '<div class="window-header">';
                header += '<div class="window-heading-icon"></div>';
                header += '<div class="window-heading" ng-bind="heading"></div>';
                if (attr.type === 'tabs') {
                    el.addClass('tabbed');
                    header += '<ul class="tabs-nav">';
                    header += '<li ng-repeat="pane in panes" ng-class="{active:pane.selected}">'
                        + '<a href="" ng-click="select(pane)">{{pane.heading}}</a></li>';
                    header += '</ul>';
                }
                // header += '<button class="btn-settings window-heading-button"' +
                //     ' uib-popover-template="\'/Areas/regitUI/templates/settings-menu.html\'" popover-placement="bottom-right" popover-is-open="isOpenSettings" popover-trigger="\'outsideClick\'"></button>';
                if (attr['wincls'] !== 'MsgList') {
                    // Removed close button of chat friends list
                    header += '<button class="window-close window-heading-button" ng-click="windowClose()"></button></div>';
                }
                header += '<div class="window-container" ng-transclude></div>';
                return header;
            },
            controller: function windowController($scope, $element, msgService) {
                var panes = $scope.panes = [];

                this.addSettingsCommand = function (cmd, handler, scope) {
                    if (angular.isFunction(handler)) {
                        handler.call(null);
                    }
                };

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
                    $element.remove();
                    windowService.removeWindow($scope.wid);
                    $scope.$broadcast('window.windowclose');
                    return false;
                };

                $scope.popoverSettingsHtml = function () {
                    var html = '<ul class="rgu-menu rgu-dropdown">';
                    angular.forEach($scope.settingsMenu.items, function (item) {
                        html += '<li><a href="#">' + item.label + '</a></li>';
                    });
                    html += '</ul>';
                    return html;
                };
                $scope.isOpenSettings = false;

                $scope.onSettingsMenuItemClick = function (e, item) {
                    e.preventDefault();
                    $scope.$broadcast('window.selectSettingsMenuItem', item);
                    $scope.isOpenSettings = false;
                    return false;
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


