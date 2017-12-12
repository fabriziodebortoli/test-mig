function initControl() {
	processingRequest = false;
	if (typeof(Sys) == 'undefined')
		return;
	var prm = Sys.WebForms.PageRequestManager.getInstance();
	prm.add_initializeRequest(initRequest);
	prm.add_endRequest(endRequest);
}

function initRequest(sender, args) {
	var req = args.get_request();
	if (processingRequest) {
		args.set_cancel(true)
		return;
	}
	document.body.style.cursor = "wait";
	processingRequest = true;
}

function endRequest(sender, args) {
	processingRequest = false;
	document.body.style.cursor = "auto";

	if (typeof execInitScript == 'function')
		execInitScript();
	
	if (args.get_error() != undefined) {
		stopPing();
		handleError(args);
	}
}

var intervalID;

function ping(dummyBtnId, interval) {
	if (!processingRequest) {
		var button = document.getElementById(dummyBtnId);
		if (button != null) {
			button.click();
		}
	}
	window.clearTimeout(intervalID);
	if (interval < 4000)
		interval = interval + 600;

	intervalID = window.setTimeout('ping(\'' + dummyBtnId + '\',' + interval + ')', interval);
}

function stopPing() {
	window.clearInterval(intervalID);
}

initControl();