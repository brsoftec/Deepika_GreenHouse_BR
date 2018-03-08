var _commonDirectives = angular.module('CommonDirectives', ['ngSanitize']);


_commonDirectives.directive('qdselect', function () {
    function link(scope, element, attrs) {
        element.wrap('<div class="select"/>');
    }
    return {
        restrict: 'A',
        scope: {},
        link: link
    };
});
_commonDirectives.directive('input', function () {
    function link(scope, element, attrs) {
        if (attrs['type'] === 'checkbox' || attrs['type'] === 'radio') {
            element.addClass('input');
        }
    }
    return {
        restrict: 'E',
        link: link
    };
});


_commonDirectives.directive('input-file', function () {
    function link(scope, element, attrs) {
        var input = element.get(0);
        var label = input.nextElementSibling,
            labelVal = label.innerHTML;
        input.addEventListener('change', function (e) {
            var fileName = '';
            if (this.files && this.files.length > 1)
                fileName = (this.getAttribute('data-multiple-caption') || '').replace('{count}', this.files.length);
            else
                fileName = e.target.value.split('\\').pop();
            if (fileName)
                label.innerHTML = fileName;
            else
                label.innerHTML = labelVal;
        });
    }
    return {
        restrict: 'AEC',
        scope: {},
        link: link
    };
});



_commonDirectives.directive('ngEnter', function () {
    return {
        link: function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                var shiftKey = event.shiftKey;
                if (typeof attrs.includeShiftKey != 'undefined') {
                    shiftKey = false;
                }
                if (event.which === 13 && !shiftKey) {
                    event.preventDefault();
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });
                }
            })
        }
    };
});

_commonDirectives.directive('ckeditor', function () {
    return {
        restrict: 'A',
        scope: {
            ckSyncher: '=ckSyncher'
        },
        require: '?ngModel',
        link: function (scope, elm, attr, ngModel) {
            var editor = CKEDITOR.replace(elm[0]);
            if (!ngModel) return;

            var updateModel = function () {
                return scope.$apply(function () {
                    return ngModel.$setViewValue(editor.getData());
                });
            };

            scope.ckSyncher = function () {
                ngModel.$setViewValue(editor.getData());
            }

            editor.on('instanceReady', function () {
                editor.on('change', updateModel);
                editor.on('dataReady', updateModel);
                editor.on('key', updateModel);
                editor.on('paste', updateModel);
                editor.on('selectionChange', updateModel);
                return editor.setData(ngModel.$viewValue);
            });

            ngModel.$render = function (value) {
                return editor.setData(ngModel.$viewValue);
            };
        }
    }
})

_commonDirectives.directive('photoupload', function () {
    return {
        restrict: 'E',
        replace: true,
        template: function (element, attrs) {
            var uploadBtn = element.find('.upload-btn');
            if (uploadBtn.length != 0) {
                uploadBtn = $($(uploadBtn.get(0))).clone().wrap('<p>').parent().html();
            } else {
                uploadBtn = '<a class="upload-btn">+ Add Photo</a>';
            }
            return '<div class="photo-uploader"><div class="photo-uploader-wrapper"><img /><div class="photo-uploader-tool"><div class="photo-uploader-tool-wrapper">' + uploadBtn + '<input type="file" /></div></div></div></div>';
        },
        scope: {
            onSelectFile: '&onSelectFile',
            imgSrc: '&imgSrc',
            clearImg: '=clearImg'
        },
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            var $element = $(element);
            var wrapper = $element.find('.photo-uploader-wrapper')[0];
            var $uploadBtn = $($element.find('.upload-btn')[0]);
            var $uploadCtrl = $($element.find('input[type=file]')[0]);
            var $img = $($element.find('img')[0]);

            var newLabel = attrs.newLabel;
            if (typeof newLabel == 'undefined') {
                newLabel = '+ Add Photo';
            } else {
                $uploadBtn.html(newLabel);
            }

            var changeLabel = attrs.changeLabel;
            if (!changeLabel) {
                changeLabel = '+ Change Photo';
            }

            var clicked = false;
            wrapper.onclick = function () {
                if (!clicked) {
                    clicked = true;
                    $uploadCtrl.click();
                    clicked = false;
                }
            }

            $uploadCtrl.change(function () {
                var upc = this;
                var files = this.files;

                if (!files.length) {
                    return;
                }

                var file = files[0];

                if (/^image\/.+$/.test(file.type)) {
                    if (typeof ngModel.$viewValue == 'undefined') {
                        ngModel.$viewValue = {};
                    }

                    ngModel.$viewValue.NewUploadPhoto = file;
                    ngModel.$commitViewValue();

                    $element.addClass('has-photo');
                    $uploadBtn.html(changeLabel);

                    var fileReader = new FileReader();
                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (e) {

                        $img.attr('src', e.target.result);

                        if (attrs.onSelectFile) {
                            scope.onSelectFile();
                        }
                    };
                } else {
                    alert("Please choose an image file.");
                }

            });

            if (attrs.clearImg) {
                scope.$watch(function () {
                    return scope.clearImg;
                }, function (val) {
                    if (val) {
                        $uploadCtrl.val('');
                        $img.removeAttr('src');
                        scope.clearImg = false;
                    }
                })
            }

            scope.$watch(function () {
                return scope.imgSrc();
            }, function (val) {
                if (val && val != '') {
                    $img.attr('src', val);
                    $element.addClass('has-photo');
                    $uploadBtn.html(changeLabel)
                }
            });
        }
    }
});

//require bootstrap modal (bootstrap js)
_commonDirectives.directive('bsModal', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            scope.$watch(function () {
                return scope.$eval(attrs.bsModalShow);
            }, function (value, old) {
                if (value == true) {
                    $(element).modal('show');
                } else if (value == false && value != old) {
                    $(element).modal('hide');
                }
            });

            $(element).on('show.bs.modal', function () {
                if (attrs.bsModalOnShow) {
                    scope.$evalAsync(attrs.bsModalOnShow);
                }
            });

            $(element).on('shown.bs.modal', function () {
                var focusTo = element.find('[focus-on-shown]');

                focusTo.focus();

                if (attrs.bsModalOnShown) {
                    scope.$evalAsync(attrs.bsModalOnShown);
                }
            });

            $(element).on('hide.bs.modal', function () {
                setTimeout(function () {
                    scope.$apply(function () {
                        if (scope.$eval(attrs.bsModalShow)) {
                            scope.$eval(attrs.bsModalShow + ' = false');
                        }
                        if (attrs.bsModalOnHide) {
                            scope.$eval(attrs.bsModalOnHide);
                        }
                    });
                }, 0);

            });

            $(element).on('hidden.bs.modal', function () {
                if (attrs.bsModalOnHidden) {
                    scope.$evalAsync(attrs.bsModalOnHidden);
                }
            });
        }
    }
});

//require bootstrap dropdown (bootstrap js)
_commonDirectives.directive('preventInsideClickClose', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $(element).click(function (event) {
                if (!$(event.target).closest($('.close-on-click')).length) {
                    event.stopPropagation();
                }
                if ($(event.target).closest($('.select-close-dropdown')).length) {
                    $(event.target).closest($('.select-close-dropdown')).parent().toggleClass('open');
                }
                if ($(event.target).closest($('[data-toggle=dropdown]')).length) {
                    $(event.target).closest($('[data-toggle=dropdown]')).parent().toggleClass('open');
                }
            })
        }
    }
});

_commonDirectives.directive('uploadControl', function () {
    return {
        restrict: 'A',
        scope: {
            __filesStore: '=filesStore',
            __onFileSelected: '&onFileSelected'
        },
        link: function (scope, element, attrs) {
            var input = $(element.find('input')[0]);

            var clicked = false;
            element.click(function (event) {
                if (!clicked) {
                    clicked = true;
                    input.click();
                    clicked = false;
                }
            });

            input.clearFile = function () {
                input.val('');
            }

            input.change(function (event) {
                scope.__filesStore.__control = input;

                var files = this.files;
                scope.$eval(function () {
                    scope.__filesStore.__files = files;

                })

                if (attrs.onFileSelected) {
                    scope.$evalAsync(function () {
                        scope.__onFileSelected();
                    })
                }

            })

        }
    }
})

_commonDirectives.directive('switcher', function () {
    return {
        restrict: 'E',
        replace: true,
        template: '<div class="switcher switcher-off"><div class="switcher-background"><div class="switcher-on-off"></div><div class="switcher-text"></div></div></div>',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }

            var switcherOnClass = 'switcher-on';
            var switcherOffClass = 'switcher-off';
            var $element = $(element);

            var $switcherText = $($element.find('.switcher-text'));

            var switcherOnText = attrs.switcherOnText ? attrs.switcherOnText : '';
            var switcherOffText = attrs.switcherOffText ? attrs.switcherOffText : '';

            var refresh = function () {
                if (ngModel.$modelValue) {
                    $element.addClass(switcherOnClass);
                    $element.removeClass(switcherOffClass);
                    $switcherText.text(switcherOnText);
                } else {
                    $element.addClass(switcherOffClass);
                    $element.removeClass(switcherOnClass);
                    $switcherText.text(switcherOffText);
                }
            }

            $element.click(function () {
                ngModel.$setViewValue(!ngModel.$modelValue);
                ngModel.$render();
                if (attrs.onChanged) {
                    scope.$evalAsync(attrs.onChanged);
                }
            });

            ngModel.$render = function () {
                refresh();
            }

        }
    }
});

_commonDirectives.directive('switch', function () {
    return {
        restrict: 'E',
        replace: true,
        template: '<div class="switch switch-off"><div class="switch-background"><div class="switch-on-off"></div><div class="switch-text"></div></div></div>',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }

            var switcherOnClass = 'switch-on';
            var switcherOffClass = 'switch-off';
            var $element = $(element);

            var $switcherText = $($element.find('.switch-text'));

            var switcherOnText = attrs.switcherOnText ? attrs.switcherOnText : '';
            var switcherOffText = attrs.switcherOffText ? attrs.switcherOffText : '';

            var refresh = function () {
                if (ngModel.$modelValue) {
                    $element.addClass(switcherOnClass);
                    $element.removeClass(switcherOffClass);
                    $switcherText.text(switcherOnText);
                } else {
                    $element.addClass(switcherOffClass);
                    $element.removeClass(switcherOnClass);
                    $switcherText.text(switcherOffText);
                }
            }

            $element.click(function () {
                ngModel.$setViewValue(!ngModel.$modelValue);
                ngModel.$render();
                if (attrs.onChanged) {
                    scope.$evalAsync(attrs.onChanged);
                }
            });

            ngModel.$render = function () {
                refresh();
            }

        }
    }
});

_commonDirectives.directive('countrycity', function () {
   
    return {
        restrict: 'E',
        templateUrl: '/Areas/User/Views/Shared/Template/countryCity.html',
        controller: 'CountryCityController',
    }
});



//require bootstrap dropdown (bootstrap js)
_commonDirectives.directive('bsSelect', ['$parse', '$compile', function ($parse, $compile) {

    var controller = ['$scope', function ($scope) {
        var ctrl = this;
        $scope.bsSelectItem = function (item) {
            $scope.$_selectedItem = item;
            $scope.$_oldItem = ctrl.$ngModel.$modelValue;
            ctrl.$ngModel.$setViewValue(item);
            if (ctrl.$callback) {
                $scope.$eval(ctrl.$callback);
            }
        }
    }];

    var repeatParser = function ($parse) {
        return {
            Parse: function (expression) {
                var match = expression.match(/^\s*(?:([\s\S]+?)\s+as\s+)?(?:([\$\w][\$\w]*)|(?:\(\s*([\$\w][\$\w]*)\s*,\s*([\$\w][\$\w]*)\s*\)))\s+in\s+([\w]+)\s*(|\s*[\s\S]+?)?(?:\s+track\s+by\s+([\s\S]+?))?\s*$/);

                if (!match) {
                    return;
                }

                return {
                    itemName: match[4] || match[2], // (lhs) Left-hand side,
                    keyName: match[3], //for (key, value) syntax
                    source: $parse(!match[3] ? match[5] + (match[6] || '') : match[5]), //concat source with filters if its an array
                    sourceName: match[5],
                    filters: match[6],
                    trackByExp: match[7],
                    modelMapper: $parse(match[1] || match[4] || match[2]),
                    repeatExpression: function (grouped) {
                        var expression = this.itemName + ' in ' + (grouped ? '$group.items' : '$select.items');
                        if (this.trackByExp) {
                            expression += ' track by ' + this.trackByExp;
                        }
                        return expression;
                    }
                };
            },
            GetGroupNgRepeatExpression: function () {
                return '$group in $select.groups';
            }
        };
    };

    return {
        restrict: 'A',
        scope: true,
        controller: controller,
        controllerAs: '$select',
        require: ['bsSelect', 'ngModel'],
        compile: function (tElement, tAttrs) {
            return function (scope, element, attrs, ctrls) {
                var $select = ctrls[0];
                var ngModel = ctrls[1];

                var $element = $(element);

                var direction = attrs.bsSelectDirection;

                if (!direction) {
                    direction = 'down';
                }

                $element.addClass('drop' + direction);

                var $trigger = $($element.find('.bs-select-placeholder-wrapper')[0]);
                $trigger.attr('data-toggle', 'dropdown');
                $trigger.append($('<span class="dl-caret"></span>'));

                var $choice = $($element.find('[bs-select-choice]')[0]);

                var expr = $choice.attr('bs-select-choice');
                var item = repeatParser($parse).Parse(expr);

                $choice.attr('ng-repeat', expr);
                $choice.attr('ng-click', 'bsSelectItem(' + item.itemName + ')');
                $choice.attr('ng-class', '{"active": ' + attrs.ngModel + '==' + item.itemName + '}');

                var $display = $($choice.find('[bs-select-choice-display]')[0]);
                $display.attr('ng-bind-html', $display.attr('bs-select-choice-display'));

                $element.removeAttr('bs-select');

                $compile($element)(scope);

                $select.$ngModel = ngModel;

                var callback = attrs.bsSelectCallback;
                $select.$callback = callback;
            }
        }
    }
}]);

//only using for country/city
_commonDirectives.filter('propsFilter', function () {
    return function (items, props, limit, code) {
        var out = [];
        var array = [];
        if (!limit || limit == 0)
            limit = 500;
        if (angular.isArray(items)) {
            items.forEach(function (item) {
                var itemMatches = false;

                var keys = Object.keys(props);
                for (var i = 0; i < keys.length; i++) {
                    var prop = keys[i];
                    var text = props[prop].toLowerCase();
                    if (item[prop] != undefined && item[prop].toString().toLowerCase().indexOf(text) !== -1) {
                        itemMatches = true;
                        break;
                    }
                }

                if (itemMatches) {
                    out.push(item);
                }
            });
        } else {
            // Let the output be the input untouched
            out = items;
        }

        if (out != undefined) {
            array = out.slice(0, limit);
            var item = array.findItem('Name', code);
            if (!item) {
                item = items.findItem('Name', code);
                if (item)
                    array.push(item);
            }
        }

        return array;
    };
});

_commonDirectives.filter('questionFilter', [function () {
    return function (questions, anotherQuestions) {
        if (!angular.isUndefined(questions) &&
            !angular.isUndefined(anotherQuestions) &&
            anotherQuestions.length > 0) {

            var tempQuestions = [];
            angular.forEach(questions,
                function (q) {
                    var isValid = true;
                    angular.forEach(anotherQuestions,
                        function (a) {
                            if (a.QuestionId === q.Id) {
                                isValid = false;
                            }
                        });
                    if (isValid) {
                        tempQuestions.push(q);
                    }
                });
            return tempQuestions;
        }
        return questions;
    };
}]);

_commonDirectives.directive("scrollModal", function ($timeout, $q, $http) {
    return function (scope, elm, attr) {
        angular.element(elm).bind("scroll", function () {
            var a = this.scrollTop;
            var b = this.scrollHeight - this.clientHeight;
            if (b - a === 0 && !scope.isLoadding && !scope.isAll) {
                scope.parameter.Length = scope.parameter.Length + 10;
                scope.isLoadding = true;
                $http.get('/Api/AccountSettings/GetFollowTransactions', { params: scope.parameter })
                    .then(function (response) {
                        scope.isLoadding = false;
                        scope.transactions = response.data.Transactions;
                        if (response.data.Total === response.data.Transactions.length) {
                            scope.isAll = true;
                        }
                    });

            }
            scope.$apply();
        });
    }
});