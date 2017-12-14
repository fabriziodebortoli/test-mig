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
		sHtml += '<textarea name="comments" id="comments" style="width:200px;height:200px;background-color:#D0F18F;color:#53760D;font:24px/30px cursive;">';
		sHtml += '</textarea>';
		sHtml += '';
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