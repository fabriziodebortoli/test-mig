/// <reference name="MicrosoftAjax.js"/>


Type.registerNamespace("TBWebFormControl");

TBWebFormControl.TBWebFormClient = function(element) {
	TBWebFormControl.TBWebFormClient.initializeBase(this, [element]);

}

TBWebFormControl.TBWebFormClient.prototype = {
	initialize: function() {
		TBWebFormControl.TBWebFormClient.callBaseMethod(this, 'initialize');
		execInitScript();
	},
	dispose: function() {

		//Add custom dispose actions here
		TBWebFormControl.TBWebFormClient.callBaseMethod(this, 'dispose');

	},

	saveClientState: function() {
		clientStateObject = new ClientState();
		Array.clear(clientStateObject.Forms);

		for (elementIdx in draggedObjects) {
			var element = $get(draggedObjects[elementIdx]);
			if (!element)
				continue;

			var location = Sys.UI.DomElement.getLocation(element);
			var w = parseInt(element.style.width, 10);
			var h = parseInt(element.style.height, 10);

			Array.add(clientStateObject.Forms, new FormPosition(element.id, location.x, location.y, w, h));

		}
		this._clientStateField.value = Sys.Serialization.JavaScriptSerializer.serialize(clientStateObject);
		return this._clientStateField.value;
	},
	loadClientState: function(clientState) {
		clientStateObject = Sys.Serialization.JavaScriptSerializer.deserialize(clientState);
	}
}

function ClientState() {
	this.Forms = new Array();
}

function FormPosition(id, x, y, w, h) {
	this.Id = id;
	this.X = x;
	this.Y = y;
	this.W = w;
	this.H = h;
}

function TbAction(targetId, args) {
	this.TargetId = targetId;
	this.Args = args;
}


TBWebFormControl.TBWebFormClient.registerClass('TBWebFormControl.TBWebFormClient', AjaxControlToolkit.ControlBase);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

function initWindow() {
	focusedElementId = '0';
	tbAction = null;
	tbRadioAction = null;
	pinging = false;
	processingRequest = false;
	timeoutTimer = null;

	if (Sys.Browser.agent != Sys.Browser.InternetExplorer) {
		document.captureEvents(Event.MOUSEMOVE | Event.MOUSEUP);
	}

	dragObject = null;
	resizeObject = null;
	resizeGridColumn = null;
	document.onmousemove = mouseMove;
	document.onmouseup = mouseUp;

	var form = document.forms[0];
	form.onsubmit = function() {
		return false;
	}
	draggedObjects = new Array();
	
}
function focusChild(form) {
	var child = FindFirstFocusableChild(form);
	if (child)
		child.focus();
}

function FindFirstFocusableChild(control) {
	if (!control || !(control.tagName)) {
		return null;
	}
	var tagName = control.tagName.toLowerCase();
	if (tagName == "undefined") {
		return null;
	}
	var children = control.childNodes;
	if (children) {
		for (var i = 0; i < children.length; i++) {
			try {
				if (CanFocus(children[i])) {
					return children[i];
				}
				else {
					var focused = FindFirstFocusableChild(children[i]);
					if (CanFocus(focused)) {
						return focused;
					}
				}
			} catch (e) {
			}
		}
	}
	return null;
}

function CanFocus(candidateFocusTarget)
{
	//aggiungo controllo su campi input readonly(usati da MagoWeb per gestire hyperlink, cssClass "Disabled"),
	//e sui vari bottoni che sono sempre abilitati (es. quello di chiusura, quello della toolbar)  (cssClass "NotFocusable") 
	if (WebForm_CanFocus(candidateFocusTarget))
	{
		if	(	
				candidateFocusTarget &&
				(
					(candidateFocusTarget.className.toLowerCase().indexOf("disabled") != -1)
					||
					(candidateFocusTarget.className.toLowerCase().indexOf("notfocusable") != -1)
				)
			)
			return false;

		return true;
	}
	else
		return false;
}


function alignParentZIndex(cellId) {
	var el = $get(cellId);
	if (!el)
		return;
	el.parentNode.style.zIndex = el.style.zIndex;
}
function Size(w, h) {
	this.W = w;
	this.H = h;
}
function getWindowSize() {
	var myWidth = 0, myHeight = 0;
	if (typeof (window.innerWidth) == 'number') {
		//Non-IE
		myWidth = window.innerWidth;
		myHeight = window.innerHeight;
	} else if (document.documentElement && (document.documentElement.clientWidth || document.documentElement.clientHeight)) {
		//IE 6+ in 'standards compliant mode'
		myWidth = document.documentElement.clientWidth;
		myHeight = document.documentElement.clientHeight;
	} else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
		//IE 4 compatible
		myWidth = document.body.clientWidth;
		myHeight = document.body.clientHeight;
	}

	return new Size(myWidth, myHeight);
}

function setWindowSize(width, height) {
	try {
		window.resizeTo(width, height);
	}
	catch (ex) {
	}
}

function initForm(formId) {
	var form = $get(formId);
	if (!form)
		return;
	Sys.UI.DomEvent.addHandler(form, "keyup", doAccelerator);

	if (!Array.contains(draggedObjects, formId)) {
		var size = getWindowSize();
		var formSize = Sys.UI.DomElement.getBounds(form);
		var x = Math.max(0, Math.round((size.W - formSize.width) / 2));
		var y = Math.max(0, Math.round((size.H - formSize.height) / 2));
		Sys.UI.DomElement.setLocation(form, x, y);
	}
	form.style.visibility = "visible";
}

function initMainForm(formId, threadID) {
	window.threadID = threadID;
	var form = $get(formId);
	if (!form)
		return;
	Sys.UI.DomEvent.addHandler(form, "keyup", doAccelerator);

	//se form piu grande di finestra browser, allargo qst'ultima
	var size = getWindowSize();
	var formSize = Sys.UI.DomElement.getBounds(form);
	if (formSize.width > size.W || formSize.height > size.H)
		setWindowSize(Math.max(formSize.width, size.W) + 60, Math.max(formSize.height, size.H) + 130);
}

function submitAction(targetId, commandId) {
	closePopupPanel();
	var args = new Array();
	args.push(commandId);
	for (var i = 2; i < arguments.length; i++) {
		args.push(arguments[i]);
	}

	//se c'e' azione in corso, metto da parte questa action
	//per eseguirla nell'end request
	if (processingRequest) {
		tbAction = new TbAction(targetId, args);
		return;
	}
	tbDoPostBack(targetId, Sys.Serialization.JavaScriptSerializer.serialize(args));
}

function tbDoPostBack(target, args) {
	if (!initRequest())
		return;
	
	var req = getHttpRequest();
	var form = document.forms[0];
	var url = form.action;
	req.open('POST', url, false);

	var body = '';

	form.__EVENTTARGET.value = target;
	form.__EVENTARGUMENT.value = args;

	var count = form.elements.length;
	for (i = 0; i < count; i++) {
		var element = form.elements[i];
		var name = element.name;
		if (typeof (name) === "undefined" || (name === null) || (name.length === 0) || (name === this._scriptManagerID)) {
			continue;
		}
		var tagName = element.tagName.toUpperCase();
		if (tagName === 'INPUT') {
			var type = element.type;
			if (type === 'hidden') {
				body += encodeURIComponent(name);
				body += '=';
				body += encodeURIComponent(element.value);
				body += '&';
			}
		}
	}

	req.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=utf-8');
	req.setRequestHeader('X-TBAjax', 'true');
	req.setRequestHeader('Cache-Control', 'no-cache');

	try {
		req.send(body);
		if (req.status == 200) //status == ok
			processResponse(req.responseText);
	}
	catch (ex) {
		alert('Exception caught!\nName: ' + ex.name + '\nMessage: ' + ex.message + '\nDescription: ' + ex.description);
	}
	
	window.processingRequest = false;

}

function processResponse(response) {
	var responseObject;
	try {
		responseObject = Sys.Serialization.JavaScriptSerializer.deserialize(response);
	}
	catch (e) {
		document.clear();
		document.write(response);
		return;
	}
	if (responseObject.E.length > 0) {
		var message = '';
		for (var i = 0; i < responseObject.E.length; i++) {
			var error = responseObject.E[i];
			message += error.M;
			message += '\n';
			message += error.S;
			message += '\n';
		}
		alert(message);
		return;
	}
	for (var i = 0; i < responseObject.P.length; i++) {
		var updatePanel = responseObject.P[i];
		var elToUpdate = $get(updatePanel.P);
		if (elToUpdate)
			elToUpdate.innerHTML = updatePanel.I;
		else
			alert('Element to update with Id: ' + updatePanel.P +' not found');
	}

	for (var i = 0; i < responseObject.S.length; i++) {
		var scriptUrl = responseObject.S[i];
		var head = document.getElementsByTagName('head')[0];
		var s = document.createElement('script');
		s.setAttribute("type", "text/javascript");
		s.setAttribute("src", scriptUrl);
		head.appendChild(s);
	}
	endRequest(responseObject.F);
}



function closePopupPanel() {
	var itemContainer = getPopupPanel();

	if (itemContainer.isMS != null && itemContainer.isMS) {
		itemContainer.isMS = false;
		closePopupPanelMS(itemContainer.attachedPopup);
	}	
	
	itemContainer.style.visibility = "hidden";
	itemContainer.style.display = "none";
	itemContainer.attachedPopup = "";
	itemContainer.isMS = false;
}

function getAccelerators(shiftKey, ctrlKey, altKey, formId) {
	if (shiftKey && ctrlKey && altKey) return eval("Array_" + formId + "_CTRL_ALT_SHIFT");
	if (shiftKey && ctrlKey) return eval("Array_" + formId + "_CTRL_SHIFT");
	if (shiftKey && altKey) return eval("Array_" + formId + "_CTRL_ALT");
	if (ctrlKey && altKey) return eval("Array_" + formId + "_CTRL_ALT");
	if (shiftKey) return eval("Array_" + formId + "_SHIFT");
	if (ctrlKey) return eval("Array_" + formId + "_CTRL");
	if (altKey) return eval("Array_" + formId + "_ALT");
	return eval("Array_" + formId + "_NONE");
}
function doAccelerator(event, formId) {
	if (!formId)
		formId = this.id;
	var accelerators = getAccelerators(event.shiftKey, event.ctrlKey, event.altKey, formId);
	for (elementIdx in accelerators) {
		var element = accelerators[elementIdx];
		if (element.ch == event.keyCode) {
			tbCmd(element.window, element.cmd);
			return true;
		}
	}
	if (event.shiftKey) //accelerator not found: perhaps shift is used to mask browser accelerator?
	{
		var accelerators = getAccelerators(false, event.ctrlKey, event.altKey, formId);
		for (elementIdx in accelerators) {
			var element = accelerators[elementIdx];
			if (element.ch == event.keyCode) {
				tbCmd(element.window, element.cmd);
				return true;
			}
		}
	}
	return false;
}

function ping() {
	if (processingRequest)
		return;

	var border = getResizeBorder();
	pinging = true;
	submitAction(border.id, "Ping");
}

function getFocusedControlInfo(controlId, obj) {
	if (focusedElementId == controlId) return;
	if (focusedElementId != null && focusedElementId != "") {
		var focusedElement = $get(focusedElementId);
		if (focusedElement) {
			obj.id = focusedElementId;
			obj.val = normalizeText(focusedElement.value);
		}
	}

	if ((obj.val == null) || (obj.val == undefined))
		obj.val = '';
	focusedElementId = controlId;
}

function FocusInfo() {
	this.id = '0';
	this.val = '';
}

function tbMove(control, targetId) {
	try {
		closePopupPanel();
		if (focusedElementId == control.id) {
			if (control && control.value)
				setCurSel(control, new CursorSel(0, control.value.length));
			return;
		}

		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);

		submitAction(targetId, "MoveTo", info.id, info.val);

	}
	catch (ex) {
	}

}

function tbClick(control) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(control.id, "Cmd", null, info.id, info.val);
	}
	catch (ex) {
	}
}

//click su radiobtn o checkbtn da anche fuoco al controllo
function tbRadioClick(control) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		//Metto da parte l'azione di click su Radio/check button e prima eseguo il cambio fuoco
		//se tb mi permette di cambiare fuoco, allora eseguiro' l'azione di click nell'endRequest
		var arguments = new Array();
		arguments.push("Cmd");
		arguments.push(null);
		arguments.push(info.id);
		arguments.push(info.val);

		tbRadioAction = new TbAction(control.id, arguments);
		
		//cambio fuoco
		submitAction(control.id, "MoveTo", info.id, info.val);
		
		
		return false;
	}
	catch (ex) {
	}
}

function initFileUploader(controlID, fileUploaderID, dragPanelID) {

	// Check for the various File API support.
	if (!(window.File && window.FileReader && window.FileList && window.Blob)) {
		alert('The File APIs are not fully supported in this browser.');
		return;
	}
	
	var fileUploader = $get(fileUploaderID);
	if (fileUploader == null)
		return;

	var dragPanel = $get(dragPanelID);

	if (dragPanel != null) {

		dragPanel.innerHTML = '<div style="display: inline-block">...or Drag here</div>';
		dragPanel.style.verticalAlign = "middle";
		dragPanel.style.textAlign = "center";

		dragPanel.addEventListener('dragover', handleDragOver, false);
		dragPanel.addEventListener('drop', function (event) {
			handleFileSelectDrop(event, event.dataTransfer.files, controlID);
		}, false);

		dragPanel.addEventListener('dragenter',  function () {
			preventDefaultEnter(event, dragPanel);
		}, false);

		dragPanel.addEventListener('dragleave', function () {
			preventDefaultLeave(event, dragPanel);
		}, false);
	}

	fileUploader.addEventListener('change', function (event) {
		handleFileSelect(event, event.target.files, controlID);
	}, false);
}


function preventDefaultLeave(e, dragPanel) {
	e.preventDefault();
	dragPanel.style.borderColor = "rgb(211,211,211)";
	return false;
}

function preventDefaultEnter(e, dragPanel) {
	e.preventDefault();
	dragPanel.style.borderColor = "rgb(0, 211, 0)";
	return false;
}

function handleDragOver(evt) {
	evt.stopPropagation();
	evt.preventDefault();
	evt.dataTransfer.dropEffect = 'move';
}

function handleFileSelect(event, files,  controlId) {
	
	var file = files[0];

	if (file == null) {
		return;
    }

    if (file.size > 5242880) {
        alert('File size is bigger than maximum allowed (5MB)');
        return;
    }

	try {
		var info = new FocusInfo();
		getFocusedControlInfo(controlId, info);

		var reader = new FileReader();

		// Closure to capture the file information.
		reader.onload = (function (fileToUpload) {
			return function (e) {
				submitAction(controlId, "UploadFile", null, info.id, info.val, fileToUpload.size, fileToUpload.name, e.target.result);
			};
		} (file))

		reader.readAsDataURL(file);
	}
	catch (ex) {
		alert(ex);
	}
}

function handleFileSelectDrop(evt, files, controlId) {
	
	handleFileSelect(evt, files, controlId);

	evt.preventDefault();
	return false;
}



function tbCmd(id, cmd) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(null, info);
		submitAction(id, "Cmd", cmd, info.id, info.val);
	}
	catch (ex) {
	}
}

function tbNewRow(control, nCol) {
	try {
		closePopupPanel();
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(control.id, "NewRow", info.id, info.val, nCol);
	}
	catch (ex) {
	}
}
function tbMoveBodyPage(controlId, next) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(controlId, info);
		submitAction(controlId, "BodyPage", info.id, info.val, next);
	}
	catch (ex) {
	}
}

function dragTbGridResizeCol(event, id, gridHeight, idGrid, gridWidth) {
	var obj = $get(id);
	if (obj) {
		resizeGridColumn = obj;
		if (!processingRequest)
			document.body.style.cursor = getEventSource(event).style.cursor;

		var location = Sys.UI.DomElement.getLocation(resizeGridColumn);
		resizeObject = getResizeBorder();

		resizeObject.style.width = "0px";
		resizeObject.style.height = gridHeight + "px";
		resizeObject.style.visibility = "visible";
		resizeObject.style.display = "block";
		Sys.UI.DomElement.setLocation(resizeObject, location.x + resizeGridColumn.offsetWidth, location.y);

		var grid = $get(idGrid);
		if (grid)
			resizeGridColumn.xMax = getAbsoluteXCoord(grid) + gridWidth;
		else
			resizeGridColumn.xMax = 2000;

		grabx = mousex;
		getMouseXY(event);
	}
}

function getAbsoluteXCoord(obj) {
	var x = 0;
	while (obj && obj.offsetLeft != undefined) {
		x += obj.offsetLeft;
		obj = obj.parentNode;
	}
	return x;
}

function tbClose(control) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(control.id, "CloseForm", info.id, info.val);
	}
	catch (ex) {
	}
}

function tbClickHotLink(control, lower) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(control.id, "HotLink", info.id, info.val, lower);
	}
	catch (ex) {
	}
}

function tbClickSpinControl(control, lower) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(control.id, "Spin", info.id, info.val, lower);
	}
	catch (ex) {
	}
}

function initRequest(sender, args) {
	if (processingRequest) {
		return false;
	}

	document.body.style.cursor = "wait";
	processingRequest = !pinging;
	return true;
}

function execInitScript() {
	var border = getResizeBorder();
	var script = border.attributes['initScript'];
	if (script)
		eval(script.value);
}

function endRequest(controlIDToFocus) {
	processingRequest = false;
	pinging = false;
	var border = getResizeBorder();

	document.body.style.cursor = "auto";
	showImage(getWorkingImage(), border.attributes['working']);
	
	resetPingTimeout();

	if (tbRadioAction != null && controlIDToFocus == tbRadioAction.TargetId) {
		var localTbRadioAction = tbRadioAction;
		tbRadioAction = null;
		tbDoPostBack(localTbRadioAction.TargetId, Sys.Serialization.JavaScriptSerializer.serialize(localTbRadioAction.Args));
	}

	if (tbAction != null) {
		//esegue seconda azione solo se controllo ha il fuoco
		if (controlIDToFocus == tbAction.TargetId)
			tbDoPostBack(tbAction.TargetId, Sys.Serialization.JavaScriptSerializer.serialize(tbAction.Args));
		tbAction = null;
	}
	execInitScript();

	if (controlIDToFocus) {
		focusedElementId = controlIDToFocus;
		var focusTarget;
		var oldContentEditableSetting;
		if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
			var targetControl = $get(controlIDToFocus);
			focusTarget = targetControl;
			if (targetControl && (!WebForm_CanFocus(targetControl))) {
				focusTarget = FindFirstFocusableChild(targetControl);
			}
			if (focusTarget && (typeof (focusTarget.contentEditable) !== "undefined")) {
				oldContentEditableSetting = focusTarget.contentEditable;
				focusTarget.contentEditable = false;
			}
			else {
				focusTarget = null;
			}
		}
		WebForm_AutoFocus(controlIDToFocus);
		if (focusTarget) {
			focusTarget.contentEditable = oldContentEditableSetting;
		}
	}
}

function resetPingTimeout() {
	var border = getResizeBorder();
	if (border) {
		window.clearTimeout(timeoutTimer);
		timeoutTimer = window.setTimeout("ping()", border.attributes['pingInterval'].nodeValue);
	}

}

function showImage(image, show) {
	if (image != null) {
		if (show) {
			var offset = 30;
			image.style.visibility = "visible";
			image.style.display = "block";
			var size = getWindowSize();
			var imageSize = Sys.UI.DomElement.getBounds(image);
			var y = size.H - imageSize.height - offset;

			Sys.UI.DomElement.setLocation(image, offset, y);
		}
		else {
			image.style.visibility = "hidden";
			image.style.display = "none";
		}
	}
}

var mousex = 0;
var mousey = 0;
var grabx = 0;
var graby = 0;
var orix = 0;
var oriy = 0;
var resizeTop = false;
var resizeLeft = false;
var resizeBottom = false;
var resizeRight = false;

function dragTitleMouseDown(event, obj) {
	if (!processingRequest)
		document.body.style.cursor = getEventSource(event).style.cursor;

	dragObject = obj;
	if (!Array.contains(draggedObjects, dragObject.id))
		Array.add(draggedObjects, dragObject.id);

	grabx = mousex;
	graby = mousey;
	orix = dragObject.offsetLeft;
	oriy = dragObject.offsetTop;
	getMouseXY(event);

}

function dragBorderMouseDown(event, obj, top, left, bottom, right) {
	if (!processingRequest)
		document.body.style.cursor = getEventSource(event).style.cursor;

	var location = Sys.UI.DomElement.getLocation(obj);
	resizeObject = getResizeBorder();
	resizeObject.attachedForm = obj;
	if (!Array.contains(draggedObjects, obj.id))
		Array.add(draggedObjects, obj.id);

	resizeObject.style.width = obj.style.width;
	resizeObject.style.height = obj.style.height;
	resizeObject.style.visibility = "visible";
	resizeObject.style.display = "block";
	Sys.UI.DomElement.setLocation(resizeObject, location.x, location.y);

	resizeTop = top;
	resizeLeft = left;
	resizeBottom = bottom;
	resizeRight = right;

	grabx = mousex;
	graby = mousey;
	orix = obj.offsetLeft;
	oriy = obj.offsetTop;
	getMouseXY(event);

}
function getMouseXY(e) {
	if (!e) e = window.event; // works on IE
	if (e) {
		if (e.pageX || e.pageY) { //(Moz,Safari)
			mousex = e.pageX;
			mousey = e.pageY;
		}
		else if (e.clientX || e.clientY) { // IE,Moz,Opera7
			mousex = e.clientX + document.body.scrollLeft;
			mousey = e.clientY + document.body.scrollTop;
		}
	}
}
function getEventSource(e) {
	var targ;
	if (!e) {
		var e = window.event;
	}
	if (e.target) {
		targ = e.target;
	}
	else if (e.srcElement) {
		targ = e.srcElement;
	}
	if (targ.nodeType == 3) // defeat Safari bug
	{
		targ = targ.parentNode;
	}
	return targ;
}
function mouseMove(event) {
	try {
		if (dragObject) {
			var elex = orix + (mousex - grabx);
			var eley = oriy + (mousey - graby);
			Sys.UI.DomElement.setLocation(dragObject, elex, eley);
			getMouseXY(event);
			return false;
		}
		else if (resizeGridColumn) {
			getMouseXY(event);
			if ((parseInt(resizeGridColumn.offsetWidth + (mousex - grabx), 10) < 0)  //controllo limite sx del resize
	             ||
	            (mousex > resizeGridColumn.xMax)                                                      //controllo limite dx del resize
	            )
				return false;

			var location = Sys.UI.DomElement.getLocation(resizeObject);
			Sys.UI.DomElement.setLocation(resizeObject, mousex, location.y);
			return false;
		}
		else if (resizeObject) {
			var location = Sys.UI.DomElement.getLocation(resizeObject);
			if (resizeLeft) {
				location.x = orix + (mousex - grabx);
				var w = Math.max(parseInt(resizeObject.attachedForm.style.width, 10) - (mousex - grabx), 0);
				resizeObject.style.width = w + "px";
				Sys.UI.DomElement.setLocation(resizeObject, location.x, location.y);
			}
			if (resizeRight) {
				var w = Math.max(parseInt(resizeObject.attachedForm.style.width, 10) + (mousex - grabx), 0);
				resizeObject.style.width = w + "px";
			}

			if (resizeTop) {
				location.y = oriy + (mousey - graby);
				var h = Math.max(parseInt(resizeObject.attachedForm.style.height, 10) - (mousey - graby), 0);
				resizeObject.style.height = h + "px";
				Sys.UI.DomElement.setLocation(resizeObject, location.x, location.y);
			}
			if (resizeBottom) {
				var h = Math.max(parseInt(resizeObject.attachedForm.style.height, 10) + (mousey - graby), 0);
				resizeObject.style.height = h + "px";
			}
			getMouseXY(event);
			return false;
		}
		getMouseXY(event);
		return true;
	}
	catch (e) {
		dragObject == null;
		resizeObject = null;
		resizeGridColumn = null;
		return false;
	}
}

function mouseUp() {
	dragObject = null;
	if (resizeGridColumn) {
		var w = parseInt(resizeGridColumn.offsetWidth + (mousex - grabx), 10);
		resizeObject.style.visibility = "hidden";
		resizeObject.style.display = "none";
		submitAction(resizeGridColumn.id, "ResizeColumn", w);
		resizeGridColumn = null;
		resizeObject = null;
	}
	else if (resizeObject) {

		var w = parseInt(resizeObject.style.width, 10);
		var h = parseInt(resizeObject.style.height, 10);
		var location = Sys.UI.DomElement.getLocation(resizeObject);

		Sys.UI.DomElement.setLocation(resizeObject.attachedForm, location.x, location.y);
		resizeObject.attachedForm.style.width = resizeObject.style.width;
		resizeObject.attachedForm.style.height = resizeObject.style.height;

		resizeObject.style.visibility = "hidden";
		resizeObject.style.display = "none";
		submitAction(resizeObject.attachedForm.id, "Resize", w, h);

		resizeObject = null;
	}

	if (!processingRequest)
		document.body.style.cursor = "auto";
	resizeBottom = resizeTop = resizeLeft = resizeRight = false;
}

//Context Menu

function onClickMenu(controlId, windowId, itemId) {
	var itemContainer = getPopupPanel();
	if (controlId == '')
		controlId = itemContainer.attachedPopup;

	submitAction(controlId, "ClickMenu", windowId, itemId);
}

function onShowSubMenu(subMenuId) {
	var subMenu = $get(subMenuId);
	if (subMenu) {
		var itemContainer = getPopupPanel();
		if (itemContainer.subMenu) {
			itemContainer.subMenu.style.visibility = "hidden";
			itemContainer.subMenu.style.display = "none";
		}
		subMenu.style.visibility = "visible";
		subMenu.style.display = "block";
		itemContainer.subMenu = subMenu;
	}
}

function DoLink(controlId, alias, row) { //LinkReport e LinkForm nei Report
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(controlId, info);
		submitAction(controlId, "DoLink", info.id, info.val, alias, row);
	}
	catch (ex) {
	}
}

function DoHyperLink(controlId) {   //HyperLink dei CParsedCtrl
	try {
		submitAction(controlId, "DoHyperLink");
	}
	catch (ex) {
	}
}

function SelectRow(controlId) {   //Selezione della riga del body edit quando body edit non editabile
	try {
		submitAction(controlId, "SelectRow");
	}
	catch (ex) {
	}
}

function ToggleExpandNode(controlId) {   //Expand/collapse del nodo body edit ad albero
	try {
		submitAction(controlId, "ToggleExpandNode");
	}
	catch (ex) {
	}
}


function tbDropdownButton(id, handle, cmd) {
	onContextMenu(null, id, handle, cmd);
}

function onContextMenu(event, id, handle, cmd) {
	var ownerObj = $get(id);
	if (!ownerObj) {
		closePopupPanel();
		return;
    }

    //IE8 lost event parameter if passed in setTimeout function. 
    var eventCopy = {};
    for (var i in event) {
        eventCopy[i] = event[i];
    }

	var itemContainer = getPopupPanel();
	if (itemContainer.attachedPopup == id) {
		closePopupPanel();
	}
	else {
		tbMove(ownerObj, ownerObj.id);
		window.setTimeout
		(function () { onContextMenuInternal(eventCopy, itemContainer, handle, id, cmd) },
			1
		);

	}
}
function onContextMenuInternal(event, itemContainer, handle, id, cmd) {
	if (window.processingRequest) {
		window.setTimeout
		(
			function() { onContextMenuInternal(event, itemContainer, handle, id, cmd) },
			1
		);
		return;
	}
	var ownerObj = $get(id);
	if (!ownerObj) {
		closePopupPanel();
		return;
	}


	var req = getHttpRequest();
	req.open('GET', 'TbAction.axd?a=2&id=' + encodeURIComponent(getDocSessionId()) + '&h=' + encodeURIComponent(handle) + '&c=' + cmd, false);
	req.send(null);
	if (req.status != 200) //status == ok
		return;

	var response = req.responseText;
	if (response == '')
		return;

	itemContainer.style.visibility = "visible";
	itemContainer.style.display = "block";
	if (event == null) {
		var location = Sys.UI.DomElement.getLocation(ownerObj);
		itemContainer.style.top = (location.y + parseInt(ownerObj.style.height)) + "px";
		itemContainer.style.left = location.x + "px";
	}
	else {
		getMouseXY(event);
		itemContainer.style.top = mousey + "px";
		itemContainer.style.left = mousex + "px";
	}
	itemContainer.attachedPopup = id;
	itemContainer.innerHTML = response;
	if (itemContainer.childNodes.length == 1) {
		var menu = itemContainer.childNodes[0];
		menu.attachedControl = ownerObj;
		var items = menu.getElementsByTagName('input');
		if (items.length > 0 && !(items[0].disabled))
			items[0].focus();
	}
}

function onTreeAdvContextMenu(event, treeId, nodeKey) {
var ownerObj = $get(nodeKey);
	if (!ownerObj) {
		closePopupPanel();
		return;
	}

	var itemContainer = getPopupPanel();
	if (itemContainer.attachedPopup == nodeKey) {
		closePopupPanel();
	}
	else {
		tbMove(ownerObj, ownerObj.id);
		window.setTimeout
		(function () { onTreeAdvContextMenuInternal(event, itemContainer, treeId, nodeKey) },
			1
		);

	}
}


function onTreeAdvContextMenuInternal(event, itemContainer, treeId, nodeKey) {
	if (window.processingRequest) {
		window.setTimeout
		(
			function () { onTreeAdvContextMenuInternal(event, itemContainer, treeId, nodeKey) },
			1
		);
		return;
	}
	var ownerObj = $get(nodeKey);
	if (!ownerObj) {
		closePopupPanel();
		return;
	}


	var req = getHttpRequest();
	req.open('GET', 'TbAction.axd?a=4&id=' + encodeURIComponent(getDocSessionId()) + '&ht=' + encodeURIComponent(treeId) + '&hn=' + encodeURIComponent(nodeKey), false);
	req.send(null);
	if (req.status != 200) //status == ok
		return;

	var response = req.responseText;
	if (response == '')
		return;

	itemContainer.style.visibility = "visible";
	itemContainer.style.display = "block";
	getMouseXY(event);
	itemContainer.style.top = mousey + "px";
	itemContainer.style.left = mousex + "px";
	itemContainer.attachedPopup = nodeKey;
	itemContainer.innerHTML = response;
	if (itemContainer.childNodes.length == 1) {
		var menu = itemContainer.childNodes[0];
		menu.attachedControl = ownerObj;
		var items = menu.getElementsByTagName('input');
		if (items.length > 0 && !(items[0].disabled))
			items[0].focus();
	}
}


function tbSelectIndex(id, index) {
	var info = new FocusInfo();
	getFocusedControlInfo(id, info);
	submitAction(id, "SelectIdx", index, info.id, info.val);
}

function tbSelectItemListCtrl(id, index) {
	var selectObj = $get(id);
	if (!selectObj) return;

	var info = new FocusInfo();
	getFocusedControlInfo(selectObj.id, info);
	submitAction(id, "SelectIdx", index, info.id, info.val);
}

//ListBox
function tbSelectItemListBox(ListBoxID) {
	var obj = $get(ListBoxID);
	if (!obj) return;
	tbSelectIndex(ListBoxID, obj.selectedIndex);
}


//treeview
function tbActionOnNode(control, nodeID, tbCommand) {
	try {
		var info = new FocusInfo();
		getFocusedControlInfo(control.id, info);
		submitAction(nodeID, tbCommand, info.id, info.val);
	}
	catch (ex) {
	}
}

function onCheckItem(id, index) {
	var selectObj = $get(id);
	if (!selectObj) return;

	var info = new FocusInfo();
	getFocusedControlInfo(selectObj.id, info);
	submitAction(selectObj.id, "SelectIdx", index, info.id, info.val);
}

function onDblClickItem(id, index) {
	var selectObj = $get(id);
	if (!selectObj) return;

	var info = new FocusInfo();
	getFocusedControlInfo(selectObj.id, info);
	submitAction(selectObj.id, "DblClickItem", index, info.id, info.val);
}

function tbRadarMoveRow(controlId, index) {
	timeoutId = setTimeout(function() { tbRadarMoveRowInternal(controlId, index) }, 400);
}

function tbRadarSelect(controlId, index) {
	if (timeoutId) {
		window.clearTimeout(timeoutId);
	}
	tbRadarSelectInternal(controlId, index);
}

function tbRadarSelectInternal(controlId, index) {
	try {
		var info = new FocusInfo();
		submitAction(controlId, "RadarSelect", info.id, info.val, index);
	}
	catch (ex) {
	}
}

function tbRadarMoveRowInternal(controlId, index) {
	try {
		var info = new FocusInfo();
		submitAction(controlId, "RadarMoveRow", info.id, info.val, index);
	}
	catch (ex) {
	}
}

function mouseOverButton(id) {
	var el = $get(id);
	var bounds = Sys.UI.DomElement.getBounds(el);
	el.style.width = (bounds.width + 2) + "px";
	el.style.height = (bounds.height + 2) + "px";
	el.style.zIndex++;
}

function mouseOutButton(id) {
	var el = $get(id);
	if (el == null)
		return;

	var bounds = Sys.UI.DomElement.getBounds(el);
	el.style.width = (bounds.width - 2) + "px";
	el.style.height = (bounds.height - 2) + "px";
	el.style.zIndex--;
}

function CursorSel(start, end) {
	this.Start = start;
	this.End = end;
}

function browserKey(keyCode, shiftKey, ctrlKey, altKey) {

	switch (keyCode) {
		case 0: return true;
		case 9:     //tab
			return !ctrlKey && !altKey;
		case 16:    //SHIFT
		case 17:    //CTRL
		case 18:    //ALT
			return true;
		case 67:    //CTRL-C  copy
		case 86:    //CTRL-V  paste
		case 88:    //CTRL-X  cut
			{
				if (ctrlKey && !shiftKey && !altKey)
					return true;
			}
	}

	if (shiftKey || ctrlKey || altKey)
		return false;

	switch (keyCode) {
		case 9:     //tab
		case 20:    //caps lock
		case 35:    //end
		case 36:    //home
		case 37:    // <- left arrow
		case 38:    //    up arrow
		case 39:    // -> right arrow
		case 40:    //    down arrow
		case 93:    //   context menu key    
			return true;
		default:
			return false;
	}
}

function isCommandKey(keyCode, shiftKey, ctrlKey, altKey) {
	
	//per mac -> intercetta la chiocciola su tastiera italiana
	if (!ctrlKey && altKey && keyCode == 186)
		return false;

	//an.17881 ->la pressione dei tasti ctrl e alt corrisponde al tasto altGr, consideriamo caratteri e non comandi le 
	//combinazioni con tasti ctrl+alt (perche servono per digitare tutti i caratteri diacritici nelle varie lingue)
	//La scelta e' dovuta al fatto che e' meglio sacrificare un acceleratore(wa utilizzo del mouse) che perdere la possibilita di digitare
	//un carattere
	if (ctrlKey && altKey) {
		return false;
	}

	if (ctrlKey || altKey)
		return true;

	switch (keyCode) {
		case 27:    //esc
		case 112:    //F1
		case 113:    //F2
		case 114:    //F3
		case 115:    //F4
		case 116:    //F5
		case 117:    //F6
		case 118:    //F7
		case 119:    //F8
		case 120:    //F9
		case 121:    //F10
		case 122:    //F11
		case 123:    //F12

			return true;
		default:
			return false;
	}
	return false;
}

function isCharKey(keyCode) {
	switch (keyCode) {
		case 8:     //del
		case 9:     //tab
		case 16:    //SHIFT
		case 17:    //CTRL
		case 18:    //ALT
		case 19:    //pause/break
		case 20:    //caps lock
		case 27:    //esc
		case 33: //pgup
		case 34: //pgdown
		case 35: //end
		case 36: //home
		case 37: //arrow
		case 38: //arrow
		case 39: //arrow
		case 40: //arrow
		case 41: //SELECT 
		case 42: 	//PRINT SCREEN 
		case 43: 	//EXECUTE 
		case 44: 	//SNAPSHOT 
		case 45:    //INS
		case 46:    //canc
		case 93:    //context menu
		case 112:    //F1
		case 113:    //F2
		case 114:    //F3
		case 115:    //F4
		case 116:    //F5
		case 117:    //F6
		case 118:    //F7
		case 119:    //F8
		case 120:    //F9
		case 121:    //F10
		case 122:    //F11
		case 123:    //F12
		case 144:    //numlock
		case 145:    //scrolllock
			return false;
	}
	return true;
}

function getKeyCode(e) {
	if (e.keyCode != 0) // IE
	{
		return e.keyCode;
	}
	else if (e.which != 0) // Netscape/Firefox/Opera
	{
		return e.which;
	}
	return 0;

}

function textKeyDown(e, element, handle, targetId, formId) {
	var keyCode = getKeyCode(e);
	var shift = e.shiftKey;
	var ctrl = e.ctrlKey;
	var alt = e.altKey;

	window.keyCode = keyCode;
	if (browserKey(keyCode, shift, ctrl, alt))
		return true;

	if (isCommandKey(keyCode, shift, ctrl, alt)) {
	    window.setTimeout(function () { doKeyCommand(keyCode, shift, ctrl, alt, element, handle, targetId) }, 1);
	    cancelEvent(e);
		return false;
	}
    else if (doAccelerator(e, formId)) {
		return false;
	}
	else if (!isCharKey(keyCode)) {
		formatInput(keyCode, shift, ctrl, alt, element, handle, false);
		return false;
	}

	return true;
}

function cancelEvent(e) {
    try {
        e.keyCode = 0;
    }
    catch (e) {
        e.keyCode = null 
    }
    e.cancelBubble = true
    if (e.stopPropagation)
         e.stopPropagation();
 
    e.returnValue = false
}





function textKeyPress(e, element, handle, targetId) {
	var keyCode = getKeyCode(e);
	var shift = e.shiftKey;
	var ctrl = e.ctrlKey;
	var alt = e.altKey;

	if (window.keyCode == undefined)  /*An. 17983 */
		window.keyCode = keyCode;

	if (isCharKey(window.keyCode) && !e.ctrlKey /*An.18250*/) {
		formatInput(keyCode, shift, ctrl, alt, element, handle, true);
		return false;
	}
	if (isCommandKey(window.keyCode, shift, ctrl, alt)) {
		window.setTimeout(function () { doKeyCommand(window.keyCode, shift, ctrl, alt, element, handle, targetId) }, 1);
		cancelEvent(e);
		return false;
	}
	return browserKey(window.keyCode, shift, ctrl, alt);
}


function formatInput(keyCode, shift, ctrl, alt, element, handle, keyIsChar) {

	if (window.processingKey)
		return false;

	window.processingKey = true;

	try {
		resetPingTimeout();
		try {
			var cursel = getCurSel(element);
			var req = getHttpRequest();
			var params = 'a=' + encodeURIComponent(getDocSessionId()) +
						'&b=' + encodeURIComponent(handle) +
						'&c=' + encodeURIComponent(keyCode) +
						'&d=' + encodeURIComponent(cursel.Start) +
						'&e=' + encodeURIComponent(cursel.End) +
						'&f=' + encodeURIComponent(normalizeText(element.value)) +
						'&g=' + shift +
						'&h=' + ctrl +
						'&i=' + alt +
						'&l=' + keyIsChar;
			req.open('POST', 'TbFormatter.axd', false);
			req.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
			req.send(params);
			if (req.status == 200) //status == ok
			{
				var response = req.responseText;
				var nStart = 0; nEnd = 0;
				var responseLength = req.responseText.length;

				var indexFirstSemicolon = response.indexOf(";");
				nStart = parseInt(response.substring(0, indexFirstSemicolon), 10);
				var indexSecondSemicolon = response.substr(indexFirstSemicolon + 1, responseLength - 1).indexOf(";") + indexFirstSemicolon + 1;
				nEnd = parseInt(response.substring(indexFirstSemicolon + 1, indexSecondSemicolon), 10);

				var newValue = response.substring(indexSecondSemicolon + 1, responseLength);
				var newSel = new CursorSel(nStart, nEnd);
				element.value = newValue;
				setCurSel(element, newSel);
				window.processingKey = false;

				return false;
			}

			window.processingKey = false;
			return true;
		}
		catch (e) {
			alert(e);
			window.processingKey = false;
			return true;
		}

	}
	catch (e) {
		window.processingKey = false;
		return true;
	}
}

function normalizeText(text) {
	if (text == undefined || text == null)
		return undefined;

	return text.replace(/\r/gm, "").replace(/\n/gm, "\r\n");
}

function doKeyCommand(keyCode, shift, ctrl, alt, element, handle, targetId) {
	if (window.processingKey)
		return false;
	window.processingKey = true;

	try {
		resetPingTimeout();
		var cursel = getCurSel(element);
		var windowText = normalizeText(element.value);

		submitAction(targetId, "DoKeyDown", keyCode, shift, ctrl, alt, cursel.Start, cursel.End, windowText);
		window.processingKey = false;
		return false;

		window.processingKey = false;
		return true;
	}
	catch (e) {
		window.processingKey = false;
		return false;
	}
}

function getDocSessionId() {
	var request = document.forms[0].action;
	var startIndex = request.indexOf("DocSessionId") + 13;  // 12 di docsessionId + 1 per =
	return request.substring(startIndex, request.length);
}

function getHttpRequest() {

	try {
		return new XMLHttpRequest();
	}
	catch (ex) {
	}

	var progIDs = ['Msxml2.XMLHTTP.3.0', 'Msxml2.XMLHTTP'];
	for (var i = 0, l = progIDs.length; i < l; i++) {
		try {
			return new ActiveXObject(progIDs[i]);
		}
		catch (ex) {
		}
	}

	return null;
}


function setCurSel(input, selection) {
	try {
		if (Sys.Browser.agent != Sys.Browser.Opera)
			selection = normalizeRNtoN(input.value, selection.Start, selection.End);

		input.focus();
		if (typeof (input.selectionStart) == "number") { //altri browsers
			input.setSelectionRange(selection.Start, selection.End);
		}
		else { //Explorer

			var range = input.createTextRange();
			range.collapse(true);
			range.moveStart('character', selection.Start);
			range.moveEnd('character', selection.End - selection.Start);
			range.select();
		}
	}
	catch (ex) {
	}
}

function getCurSel(input) {
	var start = 0, end = 0;
	input.focus();
	if (typeof (input.selectionStart) == "number") { //altri browsers
		if (Sys.Browser.agent == Sys.Browser.Opera) {
			start = input.selectionStart;
			end = input.selectionEnd;
		}
		else {
			var selection = normalizeNtoRN(input.value, input.selectionStart, input.selectionEnd);
			start = selection.Start;
			end = selection.End;
		}
	}
	else { //Explorer
		if (input.type == "textarea") {
			return cursorPositionForIETextarea(input);
		}
		else {
			var range = document.selection.createRange();
			var stored_range = range.duplicate();
			stored_range.moveEnd('character', input.value.length)
			if (stored_range.text == '')
				start = input.value.length
			else
				start = input.value.lastIndexOf(stored_range.text)
			stored_range = document.selection.createRange().duplicate()
			stored_range.moveStart('character', -input.value.length)
			end = stored_range.text.length;
		}
	}

	return new CursorSel(start, end);
}

function cursorPositionForIETextarea(textarea) {
	// get selection in firefox, opera, …

	var selection_range = document.selection.createRange().duplicate();

	if (selection_range.parentElement() == textarea) { // Check that the selection is actually in our textarea
		// Create three ranges, one containing all the text before the selection,
		// one containing all the text in the selection (this already exists), and one containing all
		// the text after the selection.
		var before_range = document.body.createTextRange();
		before_range.moveToElementText(textarea); // Selects all the text
		before_range.setEndPoint("EndToStart", selection_range); // Moves the end where we need it

		var after_range = document.body.createTextRange();
		after_range.moveToElementText(textarea); // Selects all the text
		after_range.setEndPoint("StartToEnd", selection_range); // Moves the start where we need it

		var before_finished = false, selection_finished = false, after_finished = false;
		var before_text, untrimmed_before_text, selection_text, untrimmed_selection_text, after_text, untrimmed_after_text;

		// Load the text values we need to compare
		before_text = untrimmed_before_text = before_range.text;
		selection_text = untrimmed_selection_text = selection_range.text;
		after_text = untrimmed_after_text = after_range.text;

		// Check each range for trimmed newlines by shrinking the range by 1 character and seeing
		// if the text property has changed. If it has not changed then we know that IE has trimmed
		// a \r\n from the end.
		do {
			if (!before_finished) {
				if (before_range.compareEndPoints("StartToEnd", before_range) == 0) {
					before_finished = true;
				} else {
					before_range.moveEnd("character", -1)
					if (before_range.text == before_text) {
						untrimmed_before_text += "\r\n";
					} else {
						before_finished = true;
					}
				}
			}
			if (!selection_finished) {
				if (selection_range.compareEndPoints("StartToEnd", selection_range) == 0) {
					selection_finished = true;
				} else {
					selection_range.moveEnd("character", -1)
					if (selection_range.text == selection_text) {
						untrimmed_selection_text += "\r\n";
					} else {
						selection_finished = true;
					}
				}
			}
			if (!after_finished) {
				if (after_range.compareEndPoints("StartToEnd", after_range) == 0) {
					after_finished = true;
				} else {
					after_range.moveEnd("character", -1)
					if (after_range.text == after_text) {
						untrimmed_after_text += "\r\n";
					} else {
						after_finished = true;
					}
				}
			}

		} while ((!before_finished || !selection_finished || !after_finished));

		// Untrimmed success test to make sure our results match what is actually in the textarea
		// This can be removed once you’re confident it’s working correctly
		var untrimmed_text = untrimmed_before_text + untrimmed_selection_text + untrimmed_after_text;
		var untrimmed_successful = false;
		if (textarea.value == untrimmed_text) {
			untrimmed_successful = true;
		}
		// ** END Untrimmed success test

		var startPoint = untrimmed_before_text.length;
		var endPoint = startPoint + untrimmed_selection_text.length;
		return new CursorSel(startPoint, endPoint);
	}
}
function normalizeNtoRN(text, start, end) {
	var stringBeforeStart = text.substr(0, start);
	start += countOccurenceString(stringBeforeStart, "\n");

	var stringBeforeEnd = text.substr(0, end);
	end += countOccurenceString(stringBeforeEnd, "\n");

	return new CursorSel(start, end);
}


function normalizeRNtoN(text, start, end) {
	var newStart = 0;
	var newEnd = 0;
	var s = new String(normalizeText(text));
	for (var i = 0; i < s.length; i++) {
		if (s.charAt(i) == '\r')
			continue;

		if (i < start)
			newStart++;

		if (i < end)
			newEnd++;
		else
			break;
	}

	return new CursorSel(newStart, newEnd);
}

function countOccurenceString(mainString, searchString) {
	return mainString.split(searchString).length - 1
}

function CloseMessageControl(closeBtnId) {
	var closeBtn = $get(closeBtnId);
	if (closeBtn)
		submitAction(closeBtnId, "CloseMessage");
	return false;
}

function formatCalendarDate(calendarExtender, textBoxID, textBoxHandle) {
	var textObj = $get(textBoxID);
	if (textObj) {
		var dateText = calendarExtender.get_selectedDate().format("MM/dd/yyyy");
		var req = getHttpRequest();
		req.open('GET', 'TbAction.axd?a=3&id=' + encodeURIComponent(getDocSessionId()) + '&h=' + encodeURIComponent(textBoxHandle) + '&d=' + encodeURIComponent(dateText), false);
		req.send(null);
		if (req.status == 200) //status == ok
		{
			calendarExtender._textbox.set_Value(req.responseText);
			calendarExtender._textbox.get_element().focus();
		}
	}
}

initWindow();



//override del metodo getLocation che aveva dei bug su SAfari e Mozilla
//il codice e' preso dal toolkit 30930 sept. 2009. DA RIMUOVERE quando si aggiorna il toolkit.
//(non aggiorno ora perche il nuovo tabber mostra sempre l'header delle tab su piu righe senza bottoni scorrimento)

if (document.documentElement.getBoundingClientRect) {
	Sys.UI.DomElement.getLocation = function(element) {
		if (element.self || element.nodeType === 9 || // window?
            (element === document.documentElement) || // documentElement?
            (element.parentNode === element.ownerDocument.documentElement)) { // body?
			return new Sys.UI.Point(0, 0);
		}
		var clientRect = element.getBoundingClientRect();
		if (!clientRect) {
			return new Sys.UI.Point(0, 0);
		}
		var ex, documentElement = element.ownerDocument.documentElement,
            offsetX = Math.round(clientRect.left) + documentElement.scrollLeft,
            offsetY = Math.round(clientRect.top) + documentElement.scrollTop;
		if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
			try {
				var f = element.ownerDocument.parentWindow.frameElement || null;
				if (f) {
					// frameBorder has a default of "1" so undefined must map to 0, and "0" and "no" to 2.
					var offset = (f.frameBorder === "0" || f.frameBorder === "no") ? 2 : 0;
					offsetX += offset;
					offsetY += offset;
				}
			}
			catch (ex) {
			}
			if (Sys.Browser.version === 7 && !document.documentMode) {
				var body = document.body,
                    rect = body.getBoundingClientRect(),
                    zoom = (rect.right - rect.left) / body.clientWidth;
				zoom = Math.round(zoom * 100);
				zoom = (zoom - zoom % 5) / 100;
				if (!isNaN(zoom) && (zoom !== 1)) {
					offsetX = Math.round(offsetX / zoom);
					offsetY = Math.round(offsetY / zoom);
				}
			}
			if ((document.documentMode || 0) < 8) {
				offsetX -= documentElement.clientLeft;
				offsetY -= documentElement.clientTop;
			}
		}
		return new Sys.UI.Point(offsetX, offsetY);
	}
}
else if (Sys.Browser.agent === Sys.Browser.Safari) {
	Sys.UI.DomElement.getLocation = function(element) {
		if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0, 0);

		var offsetX = 0, offsetY = 0,
            parent,
            previous = null,
            previousStyle = null,
            currentStyle;
		for (parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
			currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
			var tagName = parent.tagName ? parent.tagName.toUpperCase() : null;

			if ((parent.offsetLeft || parent.offsetTop) &&
                ((tagName !== "BODY") || (!previousStyle || previousStyle.position !== "absolute"))) {
				offsetX += parent.offsetLeft;
				offsetY += parent.offsetTop;
			}

			if (previous && Sys.Browser.version >= 3) {
				offsetX += parseInt(currentStyle.borderLeftWidth);
				offsetY += parseInt(currentStyle.borderTopWidth);
			}
		}

		currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
		var elementPosition = currentStyle ? currentStyle.position : null;
		// If an element is absolutely positioned, its parent's scroll should not be subtracted
		if (!elementPosition || (elementPosition !== "absolute")) {
			for (parent = element.parentNode; parent; parent = parent.parentNode) {
				tagName = parent.tagName ? parent.tagName.toUpperCase() : null;

				if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {
					offsetX -= (parent.scrollLeft || 0);
					offsetY -= (parent.scrollTop || 0);
				}
				currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
				var parentPosition = currentStyle ? currentStyle.position : null;

				if (parentPosition && (parentPosition === "absolute")) break;
			}
		}
		return new Sys.UI.Point(offsetX, offsetY);
	}
}
else {
	Sys.UI.DomElement.getLocation = function(element) {
		if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0, 0);

		var offsetX = 0, offsetY = 0,
            parent,
            previous = null,
            previousStyle = null,
            currentStyle = null;
		for (parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
			var tagName = parent.tagName ? parent.tagName.toUpperCase() : null;
			currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);

			if ((parent.offsetLeft || parent.offsetTop) &&
                !((tagName === "BODY") &&
                (!previousStyle || previousStyle.position !== "absolute"))) {

				offsetX += parent.offsetLeft;
				offsetY += parent.offsetTop;
			}

			if (previous !== null && currentStyle) {
				if ((tagName !== "TABLE") && (tagName !== "TD") && (tagName !== "HTML")) {
					offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
					offsetY += parseInt(currentStyle.borderTopWidth) || 0;
				}
				if (tagName === "TABLE" &&
                    (currentStyle.position === "relative" || currentStyle.position === "absolute")) {
					offsetX += parseInt(currentStyle.marginLeft) || 0;
					offsetY += parseInt(currentStyle.marginTop) || 0;
				}
			}
		}

		currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
		var elementPosition = currentStyle ? currentStyle.position : null;
		// If an element is absolutely positioned, its parent's scroll should not be subtracted, except on Opera.
		if (!elementPosition || (elementPosition !== "absolute")) {
			for (parent = element.parentNode; parent; parent = parent.parentNode) {
				tagName = parent.tagName ? parent.tagName.toUpperCase() : null;

				if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {

					offsetX -= (parent.scrollLeft || 0);
					offsetY -= (parent.scrollTop || 0);

					currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
					if (currentStyle) {
						offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
						offsetY += parseInt(currentStyle.borderTopWidth) || 0;
					}
				}
			}
		}
		return new Sys.UI.Point(offsetX, offsetY);
	}
}

//**********************************************
//Script per TABBER
//**********************************************

function onNavigate(isMoveNext, id) {
	var tabHeader = $get(id);
	var tabs = getTabs(tabHeader);

	if (tabs.length <= 0)
		return;

	var firstVisibleTabIndex = getFirstVisibleTabIndex(tabHeader);

	if (firstVisibleTabIndex == -1)
		return;

	if (isMoveNext) {
		aTab = tabs[firstVisibleTabIndex];
		aTab.style.visibility = "hidden";
		aTab.style.display = "none";
		setFirstVisibleTabIndex(tabHeader, ++firstVisibleTabIndex);
	}
	else {
		if (firstVisibleTabIndex > 0) {
			aTab = tabs[firstVisibleTabIndex - 1];
			aTab.style.visibility = "visible";
			aTab.style.display = "inline-block";
			setFirstVisibleTabIndex(tabHeader, --firstVisibleTabIndex);
		}
	}

	updateMoveButtons(tabHeader);
}
function IndexMap(id, index) {
	this.id = id;
	this.index = index;
}
function getFirstVisibleTabIndex(tabHeader) {
	if (!window.tabIndexMap)
		window.tabIndexMap = new Array();

	var map = window.tabIndexMap;
	for (var i in map) {
		var el = map[i];
		if (el.id == tabHeader.id)
			return el.index;
	}

	map.push(new IndexMap(tabHeader.id, 0));
	return 0;
}
function setFirstVisibleTabIndex(tabHeader, index) {
	if (!window.tabIndexMap)
		window.tabIndexMap = new Array();

	var map = window.tabIndexMap;
	for (var i in map) {
		var el = map[i];
		if (el.id == tabHeader.id) {
			el.index = index;
			return;
		}
	}

	map.push(new IndexMap(tabHeader.id, index));
}


function isLastTabVisible(tabHeader) {
	var tabs = getTabs(tabHeader);
	var lastTab = tabs[tabs.length - 1];
	if (!lastTab)
		return true;
	return (lastTab.offsetLeft + lastTab.offsetWidth < parseInt(getButtonContainer(tabHeader).style["left"], 10));

}

function initTabsBtn(tabId) {
	var tabHeader = $get(tabId + '_h');
	if (!tabHeader)
		return;
	var tabs = getTabs(tabHeader);

	for (var i = 0; i < tabs.length; i++) {
		var t = tabs[i];
		t.onmouseover = function(e) { Sys.UI.DomElement.addCssClass(this, "tb__tab_hover"); };
		t.onmouseout = function(e) { Sys.UI.DomElement.removeCssClass(this, "tb__tab_hover"); };
		if (t.getAttribute("Active") === "true")
			Sys.UI.DomElement.addCssClass(t, "tb__tab_active");
		else
			Sys.UI.DomElement.removeCssClass(t, "tb__tab_active");
		t.tIndex = i;
		t.onclick = function() {
			if (this.getAttribute("Active") === "true")
				return;

			tabChanged(tabId, this.tIndex);
		};
	}
	//aTab.get_headerTab().parentNode.parentNode per accedere al contenitore esterno della linguetta e riuscire a nascondere/visualizzare tutta 
	//la linguetta (modifica dovuta ad aggiornamento toolkit)
	for (var i = 0; i < getFirstVisibleTabIndex(tabHeader); i++) {
		aTab = tabs[i];
		if (!aTab)
			continue;
		aTab.style.visibility = "hidden";
		aTab.style.display = "none";
	}
	updateMoveButtons(tabHeader);
}
function getPrevButton(tabHeader) {
	return $get('btnPrev_' + tabHeader.id);
}

function getNextButton(tabHeader) {
	return $get('btnNext_' + tabHeader.id);
}

function getButtonContainer(tabHeader) {
	return $get('spanBtns_' + tabHeader.id);
}

function getTabs(tabHeader) {
	var tabs = new Array();
	for (var i in tabHeader.childNodes) {
		var t = tabHeader.childNodes[i];
		if (t.nodeName && t.nodeName.toUpperCase() == "SPAN")
			tabs.push(t);
	}
	return tabs;
}

function updateMoveButtons(tabHeader) {

	if (!tabHeader)
		return;

	var totalNumOfTabs = getTabs(tabHeader).length;

	var lastVisible = isLastTabVisible(tabHeader);
	getNextButton(tabHeader).disabled = lastVisible;

	var firstVisible = (getFirstVisibleTabIndex(tabHeader) == 0);
	getPrevButton(tabHeader).disabled = firstVisible;

	var spanBtns = getButtonContainer(tabHeader);

	var bShow = !lastVisible || !firstVisible;
	if (bShow) {
		spanBtns.style.visibility = "visible";
		spanBtns.style.display = "inline-block";
	}
	else {
		spanBtns.style.visibility = "hidden";
		spanBtns.style.display = "none";
	}
}

function tabChanged(tabId, index) {
	var info = new FocusInfo();
	getFocusedControlInfo(tabId, info);
	submitAction(tabId, "ChangeTab", index, info.id, info.val);

}


//**********************************************
//Script per DROPDOWNLIST (COMBO)
//**********************************************

function tbDropDown(textId, handle) {
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	var itemContainer = getPopupPanel();
	if (itemContainer.attachedPopup == textId) {
		closePopupPanel();
	}
	else {
		tbMove(textControl, textControl.parentNode.id);
		window.setTimeout
		(function() { tbDropDownInternal(itemContainer, handle, textId) },
			1
		);

	}
}

function tbDropDownInternal(itemContainer, handle, textId) {
	if (window.processingRequest) {
		window.setTimeout
		(
			function() { tbDropDownInternal(itemContainer, handle, textId) },
			1
		);
		return;
	}
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	itemContainer.style.visibility = "visible";
	itemContainer.style.display = "block";
	var location = Sys.UI.DomElement.getLocation(textControl);
	itemContainer.style.top = (location.y + parseInt(textControl.style.height)) + "px";
	itemContainer.style.left = location.x + "px";
	itemContainer.attachedPopup = textId;
	itemContainer.isMS = false;

	var req = getHttpRequest();
	req.open('GET', 'TbAction.axd?a=1&id=' + encodeURIComponent(getDocSessionId()) + '&h=' + encodeURIComponent(handle), false);
	req.send(null);
	if (req.status == 200) //status == ok
	{
		var response = req.responseText;
		itemContainer.innerHTML = response;
		if (itemContainer.childNodes.length == 1) {
			var list = itemContainer.childNodes[0];
			if (list.offsetWidth < textControl.offsetWidth) {
				itemContainer.style.width = textControl.style.width;
				list.style.width = textControl.style.width;

			}
			else
				itemContainer.style.width = list.style.width;
			
			list.attachedControl = textControl;
			if (list.selectedIndex == -1 && textControl.value != '') {
				for (var i = 0; i < list.length; i++) {
					var option = list.item(i);
					if (option != null && option != undefined && startsWith(option.text, textControl.value)) {
						list.selectedIndex = i;
						break;
					}
				}
			}
			//scroll verticale per visualizzare la voce selezionata(16 altezza dell'item)
			if (list.selectedIndex > 2) {
				var yScroll = (list.selectedIndex - 1) * 16;
				list.scrollTop = yScroll;
			}
			
			list.focus();
		}
	}
}

function startsWith(strTarget, str) {
	var lengthToCompare = Math.min(strTarget.length, str.length);
	return (strTarget.substr(0, lengthToCompare).toUpperCase() == str.substr(0, lengthToCompare).toUpperCase());
}

function tbSelectedItem(selectObj) {
	closePopupPanel();
	tbSelectIndex(selectObj.attachedControl.id, selectObj.selectedIndex);
}

//*******code for ipad/iphone  ***************
function tbDropDownTableSelectedItem(idCombo, selectedIndex) {
	tbSelectIndex(idCombo, selectedIndex);
}


function tbDropDownTable(textId, handle) {
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	var itemContainer = getPopupPanel();
	if (itemContainer.attachedPopup == textId) {
		closePopupPanel();
	}
	else {
		tbMove(textControl, textControl.parentNode.id);
		window.setTimeout
		(function () { tbDropDownTableInternal(itemContainer, handle, textId) },
			1
		);

	}
}

// **********************************************
// Script per MSDROPDOWNLIST (MSCOMBO)
// **********************************************

function tbMSDropDown(textId, handle) {
	
	// tbDropDown(textId, handle);
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	var itemContainer = getPopupPanel();
	if (itemContainer.attachedPopup == textId) {
		closePopupPanel();
	}
	else {
		tbMove(textControl, textControl.parentNode.id);
		window.setTimeout
		(function () {

				tbMSDropDownInternal(itemContainer, handle, textId) 
			
			},1);
	}
}

function tbMSDropDownInternal(itemContainer, handle, textId) {
	if (window.processingRequest) {
		window.setTimeout
		(
			function () { tbDropDownInternal(itemContainer, handle, textId) },
			1
		);
		return;
	}
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	itemContainer.style.visibility = "visible";
	itemContainer.style.display = "block";
	var location = Sys.UI.DomElement.getLocation(textControl);
	itemContainer.style.top = (location.y + parseInt(textControl.style.height)) + "px";
	itemContainer.style.left = location.x + "px";
	itemContainer.attachedPopup = textId;
	itemContainer.isMS = true;

	var req = getHttpRequest();
	req.open('GET', 'TbAction.axd?a=5&id=' + encodeURIComponent(getDocSessionId()) + '&h=' + encodeURIComponent(handle) + '&idTxtBox=' + textId + '&txtBox=' + textControl.value, false);
	req.send(null);
	if (req.status == 200) //status == ok
	{
		var response = req.responseText;
		itemContainer.innerHTML = response;
		
		if (itemContainer.childNodes.length == 1) {
			
			var list = itemContainer.childNodes[0];
			if (list.offsetWidth < textControl.offsetWidth) {
				itemContainer.style.width = textControl.style.width;
				list.style.width = textControl.style.width;
			}
			else
				itemContainer.style.width = list.style.width;
		}
		
	}
}

function tbMSSelectedItem(idTxtBox, key) 
{
	var textControl = $get(idTxtBox);
	if (textControl.value == "")
	{
		textControl.value = key;
		return;
	}
	
	var sta = textControl.value.search(key);
	if (sta < 0) 
	{
		textControl.value = textControl.value + '; ' + key;
	}
	else {
		var sto = textControl.value.indexOf(";", sta);
		var subst = textControl.value.substr(sta)
		if (sto >= 0) 
			subst = textControl.value.substr(sta, sto - sta);

		if (key == subst) 
		{
			if (sto >= 0)
				textControl.value = textControl.value.substr(0, sta - 1) + textControl.value.substr(sto + 1);
			else
				textControl.value = textControl.value.substr(0, sta - 2);
		}
		else
			textControl.value = textControl.value + '; ' + key;

		textControl.value = textControl.value.trim();
	}
}

function closePopupPanelMS(id) {
	console.log(id);
	submitAction(id, "SelectIdx", -1, id, $get(id).value);
}

function tbDropDownTableInternal(itemContainer, handle, textId) {
	if (window.processingRequest) {
		window.setTimeout
		(
			function () { tbDropDownTableInternal(itemContainer, handle, textId) },
			1
		);
		return;
	}
	var textControl = $get(textId);
	if (!textControl) {
		closePopupPanel();
		return;
	}
	itemContainer.style.visibility = "visible";
	itemContainer.style.display = "block";
	var location = Sys.UI.DomElement.getLocation(textControl);
	itemContainer.style.top = (location.y + parseInt(textControl.style.height) + 3) + "px";
	itemContainer.style.left = location.x + "px";
	itemContainer.attachedPopup = textId;
	itemContainer.style.height = "400px";
	itemContainer.style.overflow = "scroll";
	itemContainer.style.backgroundColor = "white";

	var req = getHttpRequest();
	req.open('GET', 'TbAction.axd?a=1&id=' + encodeURIComponent(getDocSessionId()) + '&h=' + encodeURIComponent(handle), false);
	req.send(null);
	if (req.status == 200) //status == ok
	{
		var response = req.responseText;
		itemContainer.innerHTML = response;
	}
}

//*****end ipad code

function getKeyCode(e) {
	if (e.keyCode != 0) // IE
	{
		return e.keyCode;
	}
	else if (e.which != 0) // Netscape/Firefox/Opera
	{
		return e.which;
	}
	return 0;

}

function tbComboEditKeyDown(e, element, handle, targetId, formId, textId) {
	if (getKeyCode(event) == 40) //down arrow
		tbDropDown(textId, handle);
	else
		textKeyDown(e, element, handle, targetId, formId);
}

function tbDropdownKeydown(event, selectObj) {
	if (getKeyCode(event) == 13)  //enter
		tbSelectedItem(selectObj);
}

function tbChangeStyle(comboStyle) {
	setCssRef(comboStyle.value);
	var today = new Date();
	var expire = new Date();
	expire.setTime(today.getTime() + 3600000 * 24 * 100);
	document.cookie = "cssStyle=" + escape(comboStyle.value) + ";expires=" + expire.toGMTString();
}

function setCssRef(value) {
	document.getElementById('CustomCssID').href = 'TBWebFormResource.axd?css=' + value;
}