// Returns the version of Internet Explorer or a -1 for other browsers.

function getInternetExplorerVersion() 
{
	var rv = -1;
	if(navigator.appName == 'Microsoft Internet Explorer')
	{
		var ua = navigator.userAgent;
		varre  = newRegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
		if(re.exec(ua) != null)
		rv = parseFloat( RegExp.$1 );
	}
	return rv;
}

function callcanvas() {
	return false;
}

function calltask() {
	return false;
}

function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var ua = navigator.userAgent;
		var sHtml = "";
		sHtml += '<body>';
		// sHtml += '<div style="width:400px;height:50px;background-color:#FFFFFF">';
		sHtml += '<div class="infoCss" id="infoDebug">';
		sHtml += 'Browser:';
		sHtml += ua;
		sHtml += '<Br/>';
		sHtml += '</div>';
		sHtml += '</body>';

		return sHtml;
	}

	this.start = function () {
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