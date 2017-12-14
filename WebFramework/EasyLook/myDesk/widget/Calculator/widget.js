// ====================================================================
function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var ua = navigator.userAgent;
		var sHtml = "";
		sHtml += '<div class="calculator" id="calculator_' + wid + '">';
		sHtml += '	<div class="screen" id="screen_' + wid + '"></div>';
		sHtml += '	<input type="hidden" class="outcome" id="outcome_' + wid + '" value="" />';
		sHtml += '	<ul class="buttons">';
		sHtml += ' 		<li><a class="clear" data-parameter1="' + wid + '" id="clear_' + wid + '">C</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="-">&plusmn;</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="/">&divide;</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="*">&times;</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="7">7</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="8">8</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="9">9</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="-">-</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="4">4</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="5">5</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="6">6</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="+">+</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="1">1</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="2">2</a></li>';
		sHtml += ' 		<li><a class="val" data-parameter1="' + wid + '" href="3">3</a></li>';
		sHtml += ' 		<li><a class="equal tall" data-parameter1="' + wid + '">=</a></li>';
		sHtml += ' 		<li><a class="val wide shift" data-parameter1="' + wid + '" href="0">0</a></li>';
		sHtml += ' 		<li><a class="val shift" data-parameter1="' + wid + '" href=".">.</a></li>';
		sHtml += '	</ul>';
		sHtml += '</div>';
		
		return sHtml;
	}

	this.start = function () {

		// when a value is clicked
		jq$(".val").click(function (e) {

			if (jq$(this).attr("data-parameter1") != wid)
				return;

			// prevent the link from acting like a link
			e.preventDefault();
			//grab this link's href value
			var a = jq$(this).attr("href");
			// append said value to the screen
			jq$("#screen_" + wid).append(a);
			// append same value to a hidden input
			jq$("#outcome_" + wid).val(jq$("#outcome_" + wid).val() + a);

		});

		// when equals is clicked
		jq$(".equal").click(function () {
			if (jq$(this).attr("data-parameter1") != wid)
				return;
			try {
				// solve equation and put in hidden field
				jq$("#outcome_" + wid).val(eval(jq$("#outcome_" + wid).val()));
				// take hidden field's value & put it on screen
				jq$("#screen_" + wid).html(eval(jq$("#outcome_" + wid).val()));
			} catch (e) {
				// value not valid.!
			}
		});

		// clear field
		jq$(".clear").click(function () {
			if (jq$(this).attr("data-parameter1") != wid)
				return;

			jq$("#outcome_" + wid).val("");
			jq$("#screen_" + wid).html("");
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