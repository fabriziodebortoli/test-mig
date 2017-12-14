
function calltask() {
	return true;
}

function callcanvas() {
	return true;
}

function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var sec = wid + 'sec';
		var min = wid + 'min';
		var hour = wid + 'hour';
		var sHtml = "";
		IdClock = wid;

		sHtml += '<body>';
		sHtml += '<ul id="' + wid + '" class="clock">';
		sHtml += '<li id="' + sec + '"  class="sec" ></li>';
		sHtml += '<li id="' + hour + '" class="hour"></li>';
		sHtml += '<li id="' + min + '"  class="min" ></li>';
		sHtml += '</ul>';
		sHtml += '</body>';
		return sHtml;
	}

	this.start = function () {
		return true;
	}

	return this;
}

function widgetcanvas(id) 
{
	var wid = id;
	var sec = wid + 'sec';
	var min = wid + 'min';
	var hour = wid + 'hour';

	this.preCanvas = function () {
		jq$("#" + sec).hide();
		jq$("#" + min).hide();
		jq$("#" + hour).hide();
		return true;
	}

	this.postCanvas = function () {
		jq$("#" + sec).show();
		jq$("#" + min).show();
		jq$("#" + hour).show();
		return true;
	}

	return this;
}

function task(id) 
{
	var wid = id;

	function setHand(id, rotate) {
		jq$("#" + id).css({ "-moz-transform": rotate, "-webkit-transform": rotate, "-o-transform": rotate, "-ms-transform": rotate, "transform": rotate });
	}

	this.run = function () {
		var sec = wid + 'sec';
		var min = wid + 'min';
		var hour = wid + 'hour';

		var data = new Date();
		var seconds = data.getSeconds();
		var hours = data.getHours();
		var mins = data.getMinutes();
		// second
		var hdegree = seconds * 6;
		var rotate = "rotate(" + hdegree + "deg)";
		setHand(sec, rotate);
		// hour
		degree = hours * 30 + (mins / 2);
		rotate = "rotate(" + degree + "deg)";
		setHand(hour, rotate);
		// mins
		degree = mins * 6;
		rotate = "rotate(" + degree + "deg)";
		setHand(min, rotate);

		return true;
	}

	return this;
}
