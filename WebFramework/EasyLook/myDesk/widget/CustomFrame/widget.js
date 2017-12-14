// ====================================================================
function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var ua = navigator.userAgent;
		var sHtml = "";
		sHtml += '<div class="customFrame" id="customFrame_' + wid + '">';

		sHtml += '<div class="customFrameRequest" id="customFrameRequest_"' + wid + '">';
		sHtml += '<h1>';
		sHtml += '<span class="customFrameGo" id="customFrameGo_' + wid + '"> GO! </span>';		
		sHtml += 'url: <input type="text" id="customFrameUrl_' + wid + '"/> ';
		sHtml += '</h1>';
		sHtml += '</div>';
		sHtml += '<iframe src="" id="customFrameIFrame_'+ wid+ '" width="100%" height="100%"> </iframe>';

		// sHtml += 
		sHtml += '</div>';
		return sHtml;
	}

	this.start = function () {

		function goUrl() {
			var url = "http://" + jq$("#customFrameUrl_" + wid).val();

			jq$("#customFrameIFrame_" + wid).attr('src', url);

		}

		jq$('#customFrame_' + wid).keydown(function (e) {

			if (e.keyCode == 13)
				goUrl();
		});


		doc.on('click', '#customFrameGo_' + wid, function () {
			goUrl();
		});

	}
	
	return this;
}
// ====================================================================

function callcanvas() {
	return false;
}

function calltask() {
	return false;
}

function widgetcanvas(id) {
	var wid = id;

	this.preCanvas = function () {

		return true;
	}

	this.postCanvas = function () {

		return true;
	}

	return this;
}

function task(id) {
	var wid = id;

	this.run = function () {
		return true;
	}

	return this;
}