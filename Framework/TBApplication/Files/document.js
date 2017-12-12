

/// <reference path="jquery.min.js" />
/// <reference path="jquery-ui.css" />
/// <reference path="jquery-ui.min.js" />
/// <reference path="renderengine.js" />
/// <reference path="extjs-5.1.0/ext-all-debug.js" />

//////////////////////////////
/// States whether the message
/// has to be shown.
//////////////////////////////
gbShowMessage = true;

//////////////////////////////
/// States whether the message
/// has to be shown in the console 
/// or in an alert window.
//////////////////////////////
gbUseConsole = true;

//////////////////////////////
/// log levels
/// 0 verbose => print all
/// TODO: define other levels.
//////////////////////////////
giLogLevel = 0;

//utility method to test if elements are in array
Array.prototype.contains = function (obj) {
	var i = this.length;
	while (i--) {
		if (this[i] === obj) {
			return true;
		}
	}
	return false;
}


function isLeaf(type) {
	switch (type) {
		case WndObjType_Undefined:
		case WndObjType_Label:
		case WndObjType_Button:
		case WndObjType_PdfButton:
		case WndObjType_BeButton:
		case WndObjType_BeButtonRight:
		case WndObjType_SaveFileButton:
		case WndObjType_Image:
		case WndObjType_Radio:
		case WndObjType_Check:
		case WndObjType_Combo:
		case WndObjType_Edit:
		case WndObjType_ToolbarButton:
		case WndObjType_BodyEdit:
		case WndObjType_HotLink:
		case WndObjType_ColTitle:
		case WndObjType_Cell:
		case WndObjType_MenuItem:
		case WndObjType_TreeNode:
		case WndObjType_ListCtrlItem:
		case WndObjType_ListCtrlDetails:
		case WndObjType_Spin:
		case WndObjType_Title:
		case WndObjType_GenericWndObj:
		case WndObjType_BETreeCell:
		case WndObjType_BtnImageAndText:
		case WndObjType_MultiChart:
		case WndObjType_TreeAdv:
		case WndObjType_TreeNodeAdv:
		case WndObjType_MailAddressEdit:
		case WndObjType_WebLinkEdit:
		case WndObjType_AddressEdit:
		case WndObjType_FieldReport:
		case WndObjType_TableReport:
		case WndObjType_MSCombo:
		case WndObjType_UploadFileButton:
			return true;

		case WndObjType_View:
		case WndObjType_Toolbar:
		case WndObjType_Tabber:
		case WndObjType_Tab:
		case WndObjType_Radar:
		case WndObjType_Table:
		case WndObjType_CheckList:
		case WndObjType_Tree:
		case WndObjType_Menu:
		case WndObjType_ListCtrl:
		case WndObjType_Report:
		case WndObjType_StatusBar:
		case WndObjType_SbPane:
		case WndObjType_MainMenu:
		case WndObjType_AuxRadarToolbar:
		case WndObjType_Frame:
		case WndObjType_RadarFrame:
		case WndObjType_PrintDialog:
		case WndObjType_Dialog:
		case WndObjType_PropertyDialog:
		case WndObjType_RadarHeader:
		case WndObjType_FileDialog:
		case WndObjType_EasyBuilderToolbar:
		case WndObjType_FloatingToolbar:
		case WndObjType_Thread:
		case WndObjType_Panel:
		case WndObjType_Group:
			return false;

		default:
			return true;
	}
}
//////////////////////////////
/// Logs the given message [sMessage] according
/// to the given log level [iLogLevel] and 
/// global page settings [gbShowMessage], [gbUseConsole]
/// and [giLogLevel].
//////////////////////////////
function safeAlert(sMessage, iLogLevel) {
	if (!iLogLevel) {
		iLogLevel = 0;
	}
	if (iLogLevel <= giLogLevel) {
		// message can be printed
		if (gbShowMessage) {
			if (gbUseConsole) {
				console.log(sMessage);
			}
			else {
				alert(sMessage);
			}
		}
	}
}

///////////////////////////////
/// Retrieves query parameters.
///////////////////////////////
function queryObj() {
	var result = {}, keyValuePairs = location.search.slice(1).split('&');

	keyValuePairs.forEach(function (keyValuePair) {
		keyValuePair = keyValuePair.split('=');
		result[keyValuePair[0]] = keyValuePair[1] || '';
	});

	return result;
}

function showMessage(msg, type, onOk) {
	Ext.MessageBox.show({
		title: 'Error',
		msg: msg,
		buttons: Ext.MessageBox.OK,
		fn: onOk
	});
}
window.onunload = function () {
	if (connection)
		connection.close();
}

function goInEditMode(file) {
	window.documentEvents.isEditing = true;

	// change of editor wide font
	// Ext.util.CSS.createStyleSheet("* {    font-family: \"Courier New\", \"Times New Roman\", Times, serif !important;}", "fontStyle");

	//prepare the container for edited objects
	safeRender([{
		"id": "EDITING_THREAD",
		"descState": DescState_ADDED,
		"type": WndObjType_Thread,
		"command": "",
		"width": 0,
		"height": 0,
		"enabled": true
        ,
		"items": [{
			"id": "EDITING_FRAME",
			"region": 'center',
			"descState": DescState_ADDED,
			"type": WndObjType_Panel,
			"bkgColor": 'lightgray',
			"width": '100%',
			"height": '100%',
			"scrollable": true,
			"enabled": true,
			"items": []
		}
		]
	}]);
	//"file" has to be url encoded
	// var sEncoded = encodeURIComponent(file);
	var sEncoded = file;
	// get scaling factors.
	// getBaseUnits
	$.post("getBaseUnits/", {}, function (data) {

		// set th current scaling factor.
		SpatialPropertyHelper.setScalingFactor(data.x, data.y);
		loadFile(sEncoded);

	}).fail(function () {
		safeAlert("Unable to retrieve the current scaling factor, use default one.");
		loadFile(sEncoded);

	});
}

function loadFile(sFile) {
	//retrieve the object to edit	    
	$.post("getServerFile/", {
		file: sFile
	}, function (data) {
		//transform unit to px
		SpatialPropertyHelper.transformUnitToPx(data);

		data.parentId = "EDITING_FRAME";

		// TODO: move into accWin and make more robust the 
		// accelerator saving step
		if (data.accelerators) {
			// we have accelerator modifiers to be converted
			for (var iCount = 0; iCount < data.accelerators.length; iCount++) {
				var currAcc = data.accelerators[iCount];
				// convert modifiers
				AcceleratorHelper.deserializeAccelerator(currAcc);
			}
		}
		window.rootObject = data;

		document.title = data.name;
		window.renderer.editForm();

	});
}

function generateHJson(oObj) {
	var sResult = "";
	if (oObj) {
		sResult += "#define	";
		if (isIDD(oObj)) {
			sResult += oObj.name + "\tGET_IDD(" + oObj.name + ")\n";
		} else {
			sResult += oObj.name + "\tGET_IDC(" + oObj.name + ")\n";
		}
		if (oObj.items) {
			for (var i = 0; i < oObj.items.length; i++) {
				sResult += generateHJson(oObj.items[i]);
			}
		}
	}
	return sResult;
}

function isIDD(oObj) {

	if (oObj) {
		switch (oObj.wndObjType || oObj.type) {
			case WndObjType_View:
			case WndObjType_Tab:
			case WndObjType_Tile:
				return true;
			default:
				return false;
		}
	}
	return false;
}
function saveEditedObject(file) {
	var newObject = jQuery.extend(true, {}, window.rootObject);
	purgeProperties(newObject);
	//transform px to unit
	SpatialPropertyHelper.transformPxToUnit(newObject);

	var hJson = generateHJson(newObject);

	// TODO: convert accelerators.
	var aoAccelerators = [];
	if (newObject.accelerators) {
		for (var iCount = 0; iCount < newObject.accelerators.length; iCount++) {
			var oCurrAcc = newObject.accelerators[iCount];

			AcceleratorHelper.serializeAccelerator(oCurrAcc);

			// stringify the modifiers.
			aoAccelerators.push(newObject.accelerators[iCount]);
		}
		// setting back the accelerators with stringified modifiers.
		newObject.accelerators = aoAccelerators;
	}
	var aImages = [];
	extractImageNames(newObject, aImages);
	$.post("saveServerFile/", {
		file: file,
		jsonObject: JSON.stringify(newObject, null, "\t"),
		hjson: hJson,
		images: JSON.stringify(aImages, null, "\t"),
	}, function (data) {
		showMessage("File saved");
	});

	function purgeProperties(obj) {
		for (var prop in obj) {
			if (!DefaultConfigHelper.isUserProperty(prop) && (prop != 'items' && prop != 'type' && prop != 'id' && prop != 'accelerators')) {
				delete obj[prop];
			} else if (DefaultConfigHelper.isDefaultValueProp(obj, prop)) {
				// remove properties with default values, in order to keep the json smaller.
				delete obj[prop];
			}
		}
		//delete obj.parentId;
		//delete obj.descState;
		if (obj.items)
			for (var i = 0; i < obj.items.length; i++)
				purgeProperties(obj.items[i]);
	}
}

function extractImageNames(oObj, aImages) {
	if (!oObj) {
		return aImages;
	}
	if (oObj.type == WndObjType_Image) {
		aImages.push(oObj.icon);
	}
	if (oObj.items) {
		for (var iCount = 0; iCount < oObj.items.length; iCount++) {
			extractImageNames(oObj.items[iCount], aImages)
		}
	}
	return aImages;
}

Ext.Loader.setPath('Ext.ux', 'extjs-5.1.0/packages/ext-ux/build');


Ext.require([
    'Ext.ux.form.MultiSelect'
    , 'Ext.ux.form.ItemSelector'
]);

Ext.onReady(function () {
	// enable tooltip engine.
	Ext.QuickTips.init();
	// enable the focus manager.
	// Ext.FocusManager.enable(true);
	window.renderer = new RenderEngine();
	window.documentEvents = window.renderer.getEvents();
	var edit = queryObj()["edit"];
	if (edit) {
		goInEditMode(edit);
		return;
	}
	window.session = queryObj()["session"];
	connection = new TBWebSocket(window.session);

	connection.on("DeltaReady", function (data) {
		try {
			var oData = jQuery.parseJSON(data);
			
			processJSONResponse(oData);
		}
		catch (oEx) {
			showMessage(oEx);
			return;
		}
	});
	connection.on("Message", function (data) {
		try {
			var oData = jQuery.parseJSON(data);
			showMessage(oData.message.text, oData.message.type);
		}
		catch (oEx) {
			showMessage(oEx);
			return;
		}
	});

	connection.on("error", function (e) {
		showMessage(e.data);
	});

	connection.on("open", function () {
		loadDocument();
	});

});

function safeRender(items) {
	if (items != null) {
		try {
			return window.renderer.applyDelta(items)
		}
		catch (oEx) {
			console.log('documentready::safeRender: ' + oEx);
			console.log(oEx);
			showMessage(oEx.stack);
		}
	} else {
		safeAlert("safeRender::items is null");
	}
}
function sendRequest(sUrl, data, callback) {
	$.ajax({
		url: sUrl,
		dataType: 'json',
		type: 'POST',
		data: data,
		traditional: true,
		success: callback,
		error: function (errorData) {
			safeAlert("sendRequest::error data is " + errorData);
		}
	});
}
function processJSONResponse(jsonObject) {
	if (!jsonObject) {
		$('#wait').hide();
		showMessage("Connection error!");
		return;
	}
	if (jsonObject.message) {
		$('#wait').hide();
		showMessage(jsonObject.message.text, jsonObject.message.type, function () { window.close(); });
	}
	else if (jsonObject.items) {
		$('#wait').hide();
		safeRender(jsonObject.items);
	}
	else if (typeof jsonObject.descState !== "undefined") {
		$('#wait').hide();
		var items = [];
		items.push(jsonObject);
		safeRender(items);
	}
}

function loadDocument() {
	var url = "runObject/" + document.location.search;
	$('#wait').show();
	sendRequest(url, null, processJSONResponse);
}

function closeDocument() {
	var url = "closeDocument/" + document.location.search;
	sendRequest(url, null, processJSONResponse);
}
