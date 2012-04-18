var sendingData = false;
var initValue = 140;
var connected = false;

//Init HTML5
$(document).ready(function() {
	$('#nick').slider({min:-1, max:256, value: initValue});
	$('#roll').slider({min:-1, max:256, value: initValue});
});


$(function() {  

	$('#nick').bind( "slide", function(event, ui) {
		$('#nick_val').html('Nick: ' + $('#nick').slider("value"));
		sendData();		
	});
	
	$('#roll').bind( "slide", function(event, ui) {
		$('#roll_val').html('Roll: ' + $('#roll').slider("value"));
		sendData();
	});
	

	$('#sendRequest').click(function() {
		if(sendingData == false)
		{
			startSendingData();
		}
		else
		{
			stopSendingData();
		}
	});
	
	$('#connector').click(function() {
		if(connected == false)
		{
			connectToService();
		}
		else
		{		
			stopSendingData();
			window.setTimeout("disconnectFromService()", 1000);
		}
	});
	
	return true;
});

function startSendingData()
{
	$.ajax({  
		type: "POST",  
		url: "js/startexternalcontrol.php",
		success: function(data) {
			sendingData = true;
			sendData();
			$('#sendRequest').addClass('stop_sending');
			}
		});	
}

function stopSendingData()
{	
	$.ajax({  
	  type: "POST",  
	  url: "js/closeexternalcontrol.php",
	  success: function(data) {
		  sendingData = false;
		  
			$('#sendRequest').removeClass('stop_sending');
			$('#sendRequest').addClass('senddata');
		  }
	});	
}

function connectToService()
{
	$.ajax({  
	  type: "POST",  
	  url: "js/connect.php", 
	  success: function(data) {  
		connected = true;
		
		$('#sendRequest').removeClass('disable');
		$('#connector').addClass('disconnect');
		$('#sendRequest').addClass('senddata');
		
	  }
	});
}

function disconnectFromService()
{
	$.ajax({  
	  type: "POST",  
	  url: "js/disconnect.php",
	  success: function(data) {
		  connected = false;
			$('#sendRequest').addClass('disable');
			$('#connector').removeClass('disconnect');
			$('#sendRequest').removeClass('senddata');
		  }
	});
}

//add jquery functions for dynamic content
function sendData()
{  
	if(sendingData)
	{
		var nick = $('#nick').slider("value");
		var roll = $('#roll').slider("value");
	
		$.ajax({  
		  type: "POST",  
		  url: "js/senddata.php", 
		  data: "nick=" + nick + "&roll=" + roll
		});
	}
}
	
	