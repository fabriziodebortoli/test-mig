
function callcanvas() {
	return false;
}

function calltask() {
	return false;
}


function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var sHtml = "";
		sHtml += '<body>';
		sHtml += '<div id="' + wid + '">';
		sHtml += '</div>';
		sHtml += '</body>';
		 return sHtml;
	}

		this.start = function () {
			jq$("#WidgetCalendar").attr("id", wid);
			jq$('#' + wid).datepicker();	
			return true;
		}

	return this;
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