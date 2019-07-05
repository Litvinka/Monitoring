$(function() {
    var dialog, form,
      mess = $( "#message" );
 
    function checkLength( o ) {
      if ( o.length > 2500 || o.length < 6 ) {
      	if ( !($('#message').hasClass('error')) ) {
      		$('#message').addClass('error');
      		$('#message').after("<p class='error-inf'>Заполните поле!</p>");
      	}      	
        return false;
      } else {
        return true;
      }
    }

 
    dialog = $( "#dialog-form" ).dialog({
      autoOpen: false,
      height: 400,
      width: 600,
      modal: true,
      close: function() {
        form[ 0 ].reset();
        mess.removeClass( "ui-state-error" );
      }
    });

    
 
    $( "#to-answer-rw" ).button().on( "click", function() {
      dialog.dialog( "open" );
    });
});