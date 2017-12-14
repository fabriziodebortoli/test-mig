// ====================================================================
function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var ua = navigator.userAgent;
		var sHtml = "";
		sHtml += '<div class="tagcloud">';
		sHtml += '<div id="tagCloudIw">';
		sHtml += '</div>';
		sHtml += '</div>';
		return sHtml;
	}

	this.start = function () {
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