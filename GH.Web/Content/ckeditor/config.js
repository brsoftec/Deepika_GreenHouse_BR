/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
    config.extraPlugins = 'video';
    config.filebrowserImageBrowseUrl = '/Gallery/Manager?type=Images';
    config.filebrowserVideoBrowseUrl = '/Gallery/Manager?type=Videos';
    config.extraAllowedContent = 'video[*]{*};source[*]{*};h1{*};img{*}';

    config.resize_enabled = true;
    config.allowedContent = true;
};
