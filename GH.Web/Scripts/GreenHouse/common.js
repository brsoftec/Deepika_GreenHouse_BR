var __common = {
    //replace source properties's value by corresponding destination properties'value
    //ignore undefined property
    MergeObject: function (source, destination) {
        for (var key in destination) {
            if (typeof source[key] != 'undefined') {
                if ((source[key] != null && source[key].constructor == String) || (destination[key] != null && destination[key].constructor == String)) {
                    source[key] = decodeURIComponent(destination[key]);
                } else {
                    source[key] = destination[key];
                }
            }
        }
    },
    //Replace vietnamese letter with english letter
    ToNoSign: function (string) {
        if (string.constructor == String) {

            string = string.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ  |ặ|ẳ|ẵ/g, "a");
            string = string.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ  |Ặ|Ẳ|Ẵ/g, "A");
            string = string.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
            string = string.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
            string = string.replace(/ì|í|ị|ỉ|ĩ/g, "i");
            string = string.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
            string = string.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ  |ợ|ở|ỡ/g, "o");
            string = string.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ  |Ợ|Ở|Ỡ/g, "O");
            string = string.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
            string = string.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
            string = string.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
            string = string.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
            string = string.replace(/đ/g, "d");
            string = string.replace(/Đ/g, "D");

        }
        return string;
    },

    GetNewLineCharInHtml: function (string) {
        if (string && string.constructor == String) {
            string = string.replace(/(?:\r\n|\r|\n)/g, '<br />')
        }
        return string;
    },
    swal: function (_sweetAlertService, title, text, type) {
        if (type == 'success') {
            _sweetAlertService.swal({
                title: title,
                text: text,
                type: type,
                timer: 1000,
                showConfirmButton: false
            });
        }
        else {

            _sweetAlertService.swal({
                title: title,
                text: text,
                type: type,
                showConfirmButton: true
            });
        }
    }
}

var __promiseHandler = {
    Error: function (errors, status, deferer) {
        deferer.reject({
            Status: status,
            Errors: errors
        })
    },

}

var __errorHandler = {
    ProcessErrors: function (errorObj) {
        if (errorObj.Status == 400) {
            if (errorObj.Errors.constructor == Array) {

                var messages = [];
                var codes = [];
                var exceptions = [];
                errorObj.Errors.forEach(function (error) {
                    messages.push(error.Message);
                    codes.push(error.Error);
                    if (error.Exception) {
                        exceptions.push(error.Exception);
                    }
                });

                return {
                    Messages: messages,
                    Codes: codes,
                    Status: errorObj.Status,
                    Exception: exceptions[0]
                }
            } else {
                return {
                    Messages: [],
                    Codes: [],
                    Status: errorObj.Status,
                    Exception: errorObj.Errors
                }
            }
        } else {
            return {
                Messages: [],
                Codes: [],
                Status: errorObj.Status,
                Exception: errorObj.Errors
            }
        }
    },

    Swal: function (errorObj, _sweetAlertService) {
        var message = '';
        if (errorObj.Messages.length) {
            message = '<p>' + errorObj.Messages.join('</p><p>') + '</p>';
        }

        if (errorObj.Exception) {
            message += '<div><div><b>Exception</b></div><textarea onclick="copyException(this)" class="form-control" rows="5">' + 'Exception Type: ' + errorObj.Exception.ClassName + '\nMessage: ' + errorObj.Exception.Message + '\nStack Trace: ' + errorObj.Exception.StackTraceString + '</textarea></div>';
        }

        if (message.length) {
            _sweetAlertService.swal({
                // swal({
                title: 'Error',
                type: 'error',
                html: message
                // html: true
            })
        }
    }
}

String.prototype.isEmpty = function () {
    if (!this || this.length === 0)
        return true;
    else
        return false;
};

//This prototype function allows you to remove even array from array
Array.prototype.remove = function (x) {
    var i;
    for (i in this) {
        if (this[i] == x) {
            this.splice(i, 1)
        }
    }
}

//This prototype function allows you to find item in array with prop = property name
Array.prototype.findItem = function (prop, value) {
    if (this == null || this === undefined)
        return undefined;
    for (var i = 0, len = this.length; i < len; i++)
        if (this[i][prop] == value) return this[i];
    return undefined;
}

function copyException(container) {
    container.setSelectionRange(0, container.innerHTML.length);
}