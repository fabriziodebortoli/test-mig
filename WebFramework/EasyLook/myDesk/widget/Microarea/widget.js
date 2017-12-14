
function callcanvas() {
	return false;
}

function calltask() {
	return false;
}

function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		return '<iframe src="http://www.microarea.it" width="400" height="300">Contenuto alternativo per i browser che non leggono gli iframe.</iframe>';
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
		// var fr = jq$("#" + id)[0].contentWindow;
		// fr.setParentDesktop(desktop);
		return true;
	}

	return this;
}
