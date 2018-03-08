$(document).ready(function() {
    $('select').wrap('<div class="select"></div>');
    var following = $('.btn-following');
    following.mouseover(function(e) {
        $(this).text('Unfollow').addClass('btn-unfollow');
    }).mouseout(function(e) {
        $(this).text('Following').removeClass('btn-unfollow');
    });
    var edit = $('.btn-edit');
    edit.click(function(e) {
        e.preventDefault();
        var btnEdit = $(this);
        btnEdit.hide();
        var form = btnEdit.closest('fieldset');
        var staticRows = form.find('.static-row');
        var editingRows = form.find('.editing-row');
        staticRows.hide();
        editingRows.show();
        var editingButtons = form.find('.editing-button');
        editingButtons.show().click( function(e) {
            e.preventDefault();
            btnEdit.show();
            editingButtons.hide();
            staticRows.show();
            editingRows.hide();
        });
    });

    function insertDeleteButtons() {
        var gridLastFields = $('.lastfield');
        var html = '<a class="btn-delete row-button" href="#">Delete</a>';
        gridLastFields.append(html);
        gridLastFields.find('.btn-delete').click(removeFamilyMember.bind(this));
    }
    insertDeleteButtons();

    function removeFamilyMember(e) {
        var a = $(e.target);
        var row = a.closest('.ui-grid-row');
       row.hide();
        //  Action to remove family member
    }

    var inputs = document.querySelectorAll( '.input-file' );
    Array.prototype.forEach.call( inputs, function( input )
    {
        var label	 = input.nextElementSibling,
            labelVal = label.innerHTML;

        input.addEventListener( 'change', function( e )
        {
            var fileName = '';
            if( this.files && this.files.length > 1 )
                fileName = ( this.getAttribute( 'data-multiple-caption' ) || '' ).replace( '{count}', this.files.length );
            else
                fileName = e.target.value.split( '\\' ).pop();

            if( fileName )
                label.innerHTML = fileName;
            else
                label.innerHTML = labelVal;
        });
    });
});
