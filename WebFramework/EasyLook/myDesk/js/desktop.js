 /**
 * My desktop Microarea
 */

/*********************************************************************************************************************/
winStartZOrder = 4;
winTopZOrder = 5;

BORDER_SENSIBILITY_X = 50;
BORDER_SENSIBILITY_Y = 50;
HIDE_SHOW_TIME = 500;

VIRTUALDESK_PREKEY = "VDDIC_";
VIRTUALDESK_CANVASPREKEY = "canvas_desk_"

var AngleActiveCallApp_TopLeft   = "";
var AngleActiveCallApp_TopRight  = "";
var AngleActiveCallApp_DownLeft  = "";
var AngleActiveCallApp_DownRight = "goExpose()";

/* Virtual Desktop modal version */
VDESK_COL = 2;  // n
VDESK_ROW = 2;  // n
VDESK_MAR = 20; // px

var vDeskScale = 1;

var vDeskBoxSizeX = 0;
var vDeskBoxSizeY = 0;
var vDeskBoxSizeW = 0;
var vDeskBoxSizeH = 0;

/*********************************************************************************************************************/
var jq$ = jQuery.noConflict();
var doc = jq$(document);
var desktop = null;
var tmp_authenticationtoken = "";
var isCtrl = false;
var taskStop = false;

var widgetClassObj = new Dictionary();

/*********************************************************************************************************************
jquery internal extension
**********************************************************************************************************************/
jq$.fn.outerHTML = function () {
	jq$t = jq$(this);
	if ("outerHTML" in jq$t[0])
	{ return jq$t[0].outerHTML; }
	else {
		var content = jq$t.wrap('<div></div>').parent().html();
		jq$t.unwrap();
		return content;
	}
}

/*********************************************************************************************************************
Cookie
**********************************************************************************************************************/
function createCookie(name, value, days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
		var expires = "; expires=" + date.toGMTString();
	}
	else var expires = "";

	var fixedName = '<%= Request["formName"] %>';
	name = fixedName + name;

	document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for (var i = 0; i < ca.length; i++) {
		var c = ca[i];
		while (c.charAt(0) == ' ') c = c.substring(1, c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
	}
	return null;
}

function eraseCookie(name) {
	createCookie(name, "", -1);
}

/*********************************************************************************************************************
refresh authenticationtoken
**********************************************************************************************************************/
function refreshAuthenticationtoken() {
	var url = "AttachToLogin.aspx?authenticationtoken=" + tmp_authenticationtoken + "&ReturnUrl=%2fDevelopment%2fEasyLook%2fmyDesk%2findex.html";
		jq$.get(url, function (data, status) {
	}).fail(function (msg) {
		alert("Error! refreshAuthenticationtoken: [" + tmp_authenticationtoken + "] " + msg);
	});	
}

/*********************************************************************************************************************
 jquery start
 **********************************************************************************************************************/
doc.ready(function () {
	// wallpaper image
	tmp_authenticationtoken = readCookie("authtoken");

	if (tmp_authenticationtoken == "") {
		alert("authentication token null!!");
		return;
	}

	desktop = new Desktop();
});

/*********************************************************************************************************************
 loadCss
 **********************************************************************************************************************/
loadCSS = function(href) {
	var cssLink = jq$("<link>");
	jq$("head").append(cssLink); //IE hack: append before setting href

	cssLink.attr({
		rel:  "stylesheet",
		type: "text/css",
		href: href
	});
};

/*********************************************************************************************************************
 dictionary
 **********************************************************************************************************************/
function Dictionary() {
	var _dictionary = {};
	var len = 0;

	this.insert = function (key, value) {
		if (!(key in _dictionary)) len++;

		_dictionary[key] = value;
	}

	this.existsKey = function (key){
		return key in _dictionary;
	}

	this.deleteEntry = function (key) {
		len--;
		delete _dictionary[key];
	}

	this.clear = function (key) {
		len = 0;
		_dictionary = {};
	}
	
	this.getDictionary = function () {
		return _dictionary;
    }

	this.getValues = function (key) {
		return _dictionary[key];
	}

	this.getLen = function () {
		return len;
	}

}

/*********************************************************************************************************************
Window class
**********************************************************************************************************************/
function WindowClass(IdWindow) {
	var idwin = IdWindow;
	var thumb = false;
	var widgetObj = null;
	
	var idClose = "close_" + IdWindow;
	var idContent = "content_" + IdWindow;
	var idThumbtack = "Thumbtack_" + IdWindow;

	this.getThumbtack = function () {
		return thumb;
	}

	this.setThumbtack = function (val) {
		thumb = val;
	}

	this.getID = function () {
		return idwin;
	}

	this.getIdClose = function () {
		return idClose;
	}

	this.getIdContent = function () {
		return idContent;
	}

	this.getIdThumbtack = function () {
		return idThumbtack;
	}

	this.newWindowDragAndDrop = function (innerHtml, res) {
		var hOut = "";
			hOut += "<div class='droppable' id='" + idwin + "'>"
				hOut += "<div class='window_top'>";
					hOut += '<span class="float_left">';
					hOut += '</span>';
					hOut += '<span class="float_right">';
						hOut += '<a href="#" class="window_close" id="' + idClose + '"></a>';
					hOut += '</span>';
				hOut += '</div>';
				hOut += "<div id='" + idContent + "'>";
					hOut += innerHtml;
				hOut += "</div>";
				// resizable window
				hOut += '<div class="window_bottom">'; /* +  IdWindow*/
				if (res == true)
					hOut += '<span class="abs ui-resizable-handle ui-resizable-se"></span>';
				hOut += '</div>';
			hOut += "</div>";
			hOut += "<div class='rightLimit' id='limit_" + idwin + "'></div>";

		return hOut;
	}

}

/*********************************************************************************************************************
 Windows class
 **********************************************************************************************************************/
function WindowsClass() {

	var dicWin		 = new Dictionary();
	var dicWinMaster = new Dictionary();
	var tasklist     = new Dictionary(); // task 1s --> task_id = [widget, widgetid, function cache]
	var canvaslist   = new Dictionary(); // widget_id = [widget, widgetid, function cache]

	function InsertDicWin(winObj) {
		dicWin.insert(winObj.getID(), winObj);
		dicWin.insert(winObj.getIdClose(), winObj);
		dicWin.insert(winObj.getIdContent(), winObj);
		dicWin.insert(winObj.getIdThumbtack(), winObj);
		dicWinMaster.insert(winObj.getID(), winObj);
	}

	// task 1s
	setInterval(function () {

		if (taskStop)
			return;

		jq$.each(Object.keys(tasklist.getDictionary()), function (i, taskl) {
			var wid = tasklist.getValues(taskl);
			var rObj = wid[2];
			var ret = false;

			if (rObj == null) {
				jq$.getScript("widget/" + wid[1] + "/widget.js", function (data, textStatus, jqxhr) {
					funcCall = "new task('" + wid[0] + "')";
					rObj = eval(funcCall);
					ret = rObj.run();
					tasklist.insert(taskl, [wid[0], wid[1], rObj]);
					if (!ret) alert("Error in task" + wid[1]);

				});
			}
			else {
				ret = rObj.run();
				if (!ret) alert("Error in task" + wid[1]);
			}
		});
	}, 1000);

	this.preMakeDesktopCanvas = function () {
		jq$.each(Object.keys(canvaslist.getDictionary()), function (i, canvl) {
			var wid = canvaslist.getValues(canvl);
			var rObj = wid[2];
			var ret = false;

			if (rObj == null) {
				jq$.getScript("widget/" + wid[1] + "/widget.js", function (data, textStatus, jqxhr) {
					funcCall = "new widgetcanvas('" + wid[0] + "')";
					rObj = eval(funcCall);
					ret = rObj.preCanvas();
					canvaslist.insert(canvl, [wid[0], wid[1], rObj]);
					if (!ret) alert("Error in pre canvas" + wid[1]);
				});
			}
			else {
				ret = rObj.preCanvas();
				if (!ret) alert("Error in pre canvas" + wid[1]);
			}

		});
	}

	this.postMakeDesktopCanvas = function () {
		
		jq$.each(Object.keys(canvaslist.getDictionary()), function (i, canvl) {
			var wid = canvaslist.getValues(canvl);
			var rObj = wid[2];
			var ret = false;
			if (rObj != null) {
				ret = rObj.postCanvas();
				if (!ret) alert("Error in post canvas" + wid[1]);
			}
		});
	}


	this.getWindowsObj = function (IdWindow) {

		if (!dicWin.existsKey(IdWindow))
			return null;

		var id = dicWin.getValues(IdWindow).getID();
		return jq$("#" + id);
	}

	this.WinAutoOrganize = function () {
		var docH = doc.height();
		var docW = doc.width();
		var sp = 10;

		var winForLine = 3;
		var line = 2;

		var cLine = 0;
		var cWinLine = 0;

		var dic = dicWinMaster.getDictionary();
		var dicn = dicWinMaster.getLen();
		
		var boxSizeW = (docW / winForLine) - (sp * 2 * winForLine);
		var boxSizeH = (docH / line) - (sp * 2 * line);

		var boxPosTop = sp;
		var boxPosLeft = sp;

		win.windowsZorderClear();
		jq$.each(Object.keys(dic), function (i, IdDoc) {

			win.setWinPos(IdDoc, boxPosTop, boxPosLeft);
			win.setWinSize(IdDoc, boxSizeH, boxSizeW);

			if (dicn > (winForLine * line)) {
				boxPosLeft += sp;
				boxPosTop += sp;
			}
			else {
				cWinLine++;
				boxPosLeft += boxSizeW + sp * 5;
				if (cWinLine >= winForLine) {
					cWinLine = 0;
					boxPosLeft = sp;
					boxPosTop += boxSizeH + sp;
					cLine++;
				}
			}
		});

		return true;
	} 
    
	this.windowsZorderClear = function () {
		jq$.each(Object.keys(dicWinMaster.getDictionary()), function (i, IdDoc) {
			win.getWindowsObj(IdDoc).css('z-index', winStartZOrder);
        });
	}

	this.windowTop = function (IdWindow) {
		this.windowsZorderClear();
		var obj = win.getWindowsObj(IdWindow);
		if (obj != null)
			obj.css('z-index', winTopZOrder);
	}

	this.windowTopOver = function (IdWindow) {

	}

	this.SetThumbtack = function (IdWindow) {
		var obj = jq$("#" + dicWin.getValues(IdWindow).getIdThumbtack());
		if (obj.hasClass("on")) {
			dicWin.getValues(IdWindow).setThumbtack(false);
			obj.removeClass("on");
		}
		else {
			dicWin.getValues(IdWindow).setThumbtack(true);
			obj.addClass("on");
		}
	}

	this.isThumbtack = function (IdWindow) {
		return dicWin.getValues(IdWindow).getThumbtack();
	}

	this.close = function (IdWindow) {

		var winObj = dicWin.getValues(IdWindow);

		// rimove widget object
		if (widgetClassObj.existsKey(winObj.getID()))
			widgetClassObj.deleteEntry(winObj.getID());
		
		// find and remove task if exist
		if (tasklist.existsKey(winObj.getID()))
			tasklist.deleteEntry(winObj.getID());

		if (canvaslist.existsKey(winObj.getID()))
			canvaslist.deleteEntry(winObj.getID());

		dicWin.deleteEntry(winObj.getID());
		dicWin.deleteEntry(winObj.getIdClose());
		dicWin.deleteEntry(winObj.getIdContent());
		dicWin.deleteEntry(winObj.getIdThumbtack());
		dicWinMaster.deleteEntry(winObj.getID());
		jq$("#" + winObj.getID()).remove();
		jq$("#limit_" + winObj.getID()).remove();
		delete (winObj);
	}
		
	this.setWinSize = function (IdWindow, width, height) {
		jq$("#" + IdWindow).width(width);
		jq$("#" + IdWindow).height(height);
	}
	
	this.setWinPos = function (IdWindow, top, left) {
		jq$("#" + IdWindow).css({"top": top+"px", "left": left+"px"}); 
	}

	this.loadWidget = function (widget, IdWindow) {
		jq$.getScript("widget/" + widget + "/widget.js", function (data, textStatus, jqxhr) {
			//Call the function webpage
			var wigId = widget + "_" + IdWindow;
			
			funcCall = "new widgetClass('" + wigId + "')";
			wObj = eval(funcCall);
			jq$("#content_" + IdWindow).html(wObj.webpage());
			wObj.start();

			// add windget obj dictionary
			widgetClassObj.insert(IdWindow, wObj);

			//Call the function task
			funcCall = "calltask()";
			ret = eval(funcCall);
			if (ret)
				tasklist.insert(IdWindow, [wigId, widget, null]);


			//Call the function canvas
			funcCall = "callcanvas()";
			ret = eval(funcCall);
			if (ret)
				canvaslist.insert(IdWindow, [wigId, widget, null]);
		});
	}
	
	// IdWindow: id windows in page
	// innerHtml: html startup contenute
	// res: is resizable Treu / false
	this.startDragAndDrop = function (IdWindow, innerHtml, res) {
		var winObj = new WindowClass(IdWindow);
		InsertDicWin(winObj);
		if (innerHtml == null) innerHtml = '';
		return winObj.newWindowDragAndDrop(innerHtml, res);
	}
}

function Desktop() {
	var win = new WindowsClass();
	var idWidget = 0;
	var onemenuarea_lastopen = null;
	var deskIsFull = false;

	// widget
	var widgetEventListId = new Array();
	var internalWidget = new Dictionary(); // [key] = [multi, div id]
	var tagCloudMenu = new Dictionary();
	var reportPageCache = new Dictionary();

	// virtual desktop
	var vDesk = new Dictionary();
	var vDeskCount = 0;
	var vDeskActive = VIRTUALDESK_PREKEY + "0";

	// Tb Menu -  cache fast load menu.
	var MenuAreaCache = new Dictionary();
	var MenuReportCache = new Dictionary();
	var lastIdTreeLabel = "";

	var lastCanvasSizeW = 0;
	var lastCanvasSizeH = 0;

	//	Open TB url
	function openTBUrl(url) {
		var goUrl = "TB://" + url;

		// get argoments
		var cRep = reportPageCache.getValues(url);
		if (cRep[2] != "")
			goUrl += "?arguments=" + cRep[2];
		// open url
		jq$(location).attr('href', goUrl);
	}

	function getWidGetId(x) 
	{
		idWidget++;
		return  x + "_" + idWidget;
	}

	function vdesk_container_center() {
		// center vdesk_container_center
		var le = ((jq$("#desktop").width() / 2) - (jq$('#vdesk_container').width() / 2)) + "px";
		jq$('#vdesk_container').css("left", le );
	}

	function removeDesktopFull() 
	{
		if (jq$("#desktopFull").length != 0)
			jq$("#desktopFull").remove();
	}

	function desktopIsFull() 
	{
		removeDesktopFull();
		// append to execute the test if not present
		if (jq$("#desktopFull").length == 0)
			jq$("#desktop").append('<div class="droppable" id="desktopFull"></div>');
		// the desktop is full ?
		var bottomLimit = jq$(".mac-container").position().top;
		var winPos = jq$("#desktopFull").position().top;
		var winHeight = winPos + jq$("#desktopFull").height();
		removeDesktopFull();
		if ((winPos + 30>= bottomLimit)) {
			return true;
		}
		return false;
	}

	function setDesktopEnable() {
		if (jq$("#vd_" + vDeskActive).length != 0) 
		{
			jq$("#vd_" + vDeskActive).removeClass("vdesk_off");
			jq$("#vd_" + vDeskActive).addClass("vdesk_on");
		}
	}

	function appendDesktopIcon() {
		if (jq$("#vd_" + vDeskActive).length == 0) 
		{
			jq$("#vdesk_container").append('<span class="vdesk vdesk_off" id="vd_' + vDeskActive + '" data-parameter1="' + vDeskActive + '" ></span>');
		}
		else 
		{
			jq$("#vd_" + vDeskActive).removeClass("vdesk_on");
			jq$("#vd_" + vDeskActive).addClass("vdesk_off");
		}
	}

	function draggable_remove() {
		if (jq$('#draggable').size() != 0)
			jq$('#draggable').remove();
	}


	function showDesktopCanvas() {
		jq$("#showVirtualDesk").show("slow");

		jq$("#canvas_container").find('canvas').each(function (i, elem) {
			var canvas = document.getElementById(this.id);
			var ctx = canvas.getContext("2d");
			ctx.save();
			ctx.clearRect(0, lastCanvasSizeH, lastCanvasSizeW, lastCanvasSizeH + 120);
			ctx.scale(vDeskScale, vDeskScale);
			ctx.fillStyle = "blue";
			ctx.font = "bold 50pt Arial";
			ctx.fillText("Desktop " + (i + 1), 10, lastCanvasSizeH + 60)
			ctx.restore();
		});
	}

	function makeDesktopCanvas() {

		if (jq$("div.messageboxModal").is(':visible'))
			return;

		win.preMakeDesktopCanvas();

		// vDeskActive can cange when render html2canvas 
		var vd = vDeskActive;
		var canvasId = VIRTUALDESK_CANVASPREKEY + vd;
		
		jq$("#vdesk_container").hide(0);

		if (jq$("#" + canvasId).length == 0) {

			var delClass = "vdesk_canvasDel";
			if (vd == VIRTUALDESK_PREKEY + "0")
				delClass = "vdesk_canvasCommandSpace";
			
			jq$("#canvas_container").append("<div class='canvas-wrap' id='canvas_div_" + vd + "'>" +
												"<a href='#' class='" + delClass  + "' data-parameter1='" + vd + "'></a>" +
												"<canvas class='vdesk_canvas' id='" + canvasId + "' width=" + vDeskBoxSizeW +
												" height=" + vDeskBoxSizeH + " data-parameter1='" + vd + "'></canvas>" +
											"</div>");
		}

		canvasRecord = jq$('body').html2canvas({
			onrendered: function (canvas) {
				var c = document.getElementById(canvasId);
				var ctx = c.getContext("2d");
				ctx.save();
				ctx.clearRect(0, 0, canvas.width, canvas.height);
				ctx.scale(vDeskScale, vDeskScale);
				ctx.drawImage(canvas, 0, 0);
				ctx.rect(0, 0, canvas.width, canvas.height);
				ctx.lineWidth = 7;
				ctx.strokeStyle = 'black';
				ctx.stroke();

				lastCanvasSizeW = canvas.width;
				lastCanvasSizeH = canvas.height;
				
				ctx.restore();
				win.postMakeDesktopCanvas();

				var docW = doc.width();
			}
		});
	}

	function saveDesktopSate() {
		draggable_remove();
		//removeDesktopFull();
		vDesk.insert(vDeskActive, [jq$("#desktop").html(), widgetEventListId.slice()]);
		makeDesktopCanvas();
		appendDesktopIcon();
		widgetEventListId.length = 0;
	}

	function desktopDelete(id) {
		
		jq$("#vd_" + id).remove();
		jq$("#canvas_div_" + id).remove();
		vDesk.deleteEntry(id);

		function desktopDeleteCallBack() {
			var vdeskWidget = vDesk.getValues(vDeskActive)
			jq$("#desktop").html(vdeskWidget[0]);
			// show desktop
			jq$("#desktop").show();
			// enable all widget event
			jq$.each(vdeskWidget[1], function (i, item) {
				startWidgetEvent(item);
			});

			setDesktopEnable();
		}

		if (vDeskActive == id) 
		{
			vDeskActive = VIRTUALDESK_PREKEY + "0";
		} 
		else 
		{
			// save active desktop state
			saveDesktopSate();
		}

		// hide old desktop and call callback
		jq$("#desktop").hide("drop", { direction: "left" }, HIDE_SHOW_TIME, desktopDeleteCallBack);
		jq$("div.messageboxModal").hide(1500);
		
	}

	function desktopChange(id) {
		function deskChangeCallBack() {
			var vdeskWidget = vDesk.getValues(vDeskActive)
			jq$("#desktop").html(vdeskWidget[0]);
			// show desktop
			jq$("#desktop").show();
			// enable all widget event
			jq$.each(vdeskWidget[1], function (i, item) {
				startWidgetEvent(item);
			});
			setDesktopEnable();
		}

		vDeskN = id;
		if (vDeskActive == vDeskN)
			return;

		// save active desktop state
		saveDesktopSate();
		// get desktop to active
		vDeskActive = vDeskN;
		// hide old desktop and call callback
		jq$("#desktop").hide("drop", { direction: "left" }, HIDE_SHOW_TIME, deskChangeCallBack);
	}

	// ==========================================================================================================
	// new desktop
	// ==========================================================================================================

	// new desktop clear
	function newDeskPageClear() {
		function winHideCallBack() {
			// new desktop append
			vDeskCount = vDeskCount + 1;
			vDeskActive = VIRTUALDESK_PREKEY + vDeskCount;
			// clear desktop area
			jq$("#desktop").html("");
			// show desktop
			jq$("#desktop").show();
			appendDesktopIcon();
			setDesktopEnable();
		}

		// save desktop structure
		saveDesktopSate()
		// hide old desktop and call callback
		jq$("#desktop").hide("drop", { direction: "left" }, HIDE_SHOW_TIME, winHideCallBack);
	}

	// new desktop and move widget
	function newDeskPageWidMove(widHtml, widId) {
		function winHideCallBack() {
			// new desktop append
			vDeskCount = vDeskCount + 1;
			vDeskActive = VIRTUALDESK_PREKEY + vDeskCount;
			// clear desktop area
			jq$("#desktop").html(widHtml);

			if (jq$("#limit_"+ widId).length == 0)
				jq$("#desktop").append("<div class='rightLimit' id='limit_" + widId + "'></div>");

			// show desktop
			jq$("#desktop").show();
			appendDesktopIcon();
			setDesktopEnable();
			startWidgetEvent(widId);
		}

		// save desktop structure
		saveDesktopSate()
		// hide old desktop and call callback
		jq$("#desktop").hide("drop", { direction: "left" }, HIDE_SHOW_TIME, winHideCallBack);
	}

	// new desktop and new widget
	function newDeskPage(dragId, onlyWindows, res) {
		function winHideCallBack() {
			// new desktop append
			vDeskCount = vDeskCount + 1;
			vDeskActive = VIRTUALDESK_PREKEY + vDeskCount;
			// clear desktop area
			jq$("#desktop").html("");
			// start widget
			startNewWidgetDragAndDrop(dragId, onlyWindows, res);
			// show desktop
			jq$("#desktop").show();
			appendDesktopIcon();
			setDesktopEnable();
		}

		// save desktop structure
		saveDesktopSate()
		// hide old desktop and call callback
		jq$("#desktop").hide("drop", { direction: "left" }, HIDE_SHOW_TIME, winHideCallBack);
	}

	// ==========================================================================================================
	function startWidgetEvent(dragId) {

		// the widget not exist.! (remuved)
		if (jq$("#" + dragId).length == 0) {
			return;
		}

		// if not present widget not start.!
		if (widgetClassObj.existsKey(dragId))
		{
			var widObj = widgetClassObj.getValues(dragId);
			widObj.start();
		}
		
		// array event list
		widgetEventListId.push(dragId);

		// contextmenu widget
		jq$("#" + dragId).bind("contextmenu", function (e) {
			jq$('#WidgetContextMenu').html("<center>Move to</center>");

			var wigId = this.id;

			jq$('#WidgetContextMenu').append("<div class='WidgetContextMenuText' data-parameter1='new' data-parameter2='" + wigId + "'>New Desktop </div>");

			jq$("#canvas_container").find('canvas').each(function (i, elem) {
				var canvas = document.getElementById(this.id);
				var vd = this.id.substring(VIRTUALDESK_CANVASPREKEY.length); //VIRTUALDESK_PREKEY + i;

				if (vDeskActive != vd)
					jq$('#WidgetContextMenu').append("<div class='WidgetContextMenuText' data-parameter1='" + vd + "' data-parameter2='" + wigId + "'>Desktop " + (i + 1) + "</div>");

			});

			jq$('#WidgetContextMenu').css({
				top: e.pageY + 'px',
				left: e.pageX + 'px'
			}).show();
			return false;
		});

		jq$("#" + dragId).draggable({
			handle: 'div.window_top',
			revert: "invalid"
		});

		var lastValidWidth = 0;
		var lastValidHeight = 0;

		jq$("#" + dragId).resizable({
			handle: 'span.ui-resizable-se',
			containment: 'parent',
			minWidth: 200,
			minHeight: 200,

			resize: function (event, ui) {
				if (!desktopIsFull()) {
					lastValidWidth = ui.size.width;
					lastValidHeight = ui.size.height - 45; // todo: -45 correction on mouse pointer 
				}

				var obj = jq$("#content_" + jq$(ui.element).attr('id')).children();
				obj.width(lastValidWidth);
				obj.height(lastValidHeight);
				jq$(ui.element).css('width', '');
				jq$(ui.element).css('height', '');
				jq$(ui.element).css('left', '');
				jq$(ui.element).css('top', '');
				jq$(ui.element).css('position', 'relative');
			},

			start: function (event, ui) {
				lastValidWidth = ui.originalSize.width;
				lastValidHeight = ui.originalSize.height;
			}

		});

		jq$("#" + dragId).droppable({
			accept: function (ui) {
				//if (ui.hasClass('widget'))
				//	return false;
				return true;
			},

			over: function (event, ui) {
				if (jq$('#draggable').size() == 0)
					jq$(this).before("<div id='draggable'></div>");
				else
					jq$(this).before(jq$('#draggable'));

				jq$('#draggable').droppable({
					drop: function (event, ui) {
						jq$(this).before(jq$(ui.draggable));
						jq$(ui.draggable).css('left', '');
						jq$(ui.draggable).css('top', '');
						draggable_remove();
					}
				});
			},

			drop: function (event, ui) {
				if (jq$(ui.draggable).hasClass('widget')) {
					return;
				}
				// From = jq$(ui.draggable) - To = jq$(this)
				// move the widget in desktop
				jq$(this).before(jq$(ui.draggable));
				// remove left & top Style
				jq$(ui.draggable).css('left', '');
				jq$(ui.draggable).css('top', '');
				draggable_remove();
				jq$(ui.draggable).after(jq$('#limit_' + jq$(ui.draggable).attr('id')));

			},

			deactivate: function (event, ui) {
				draggable_remove();
			}
		});
	}

	function startNewWidgetDragAndDrop(dragId, onlyWindows, res) 
	{
		var _id = "";

		if (desktopIsFull()) {
			newDeskPage(dragId, onlyWindows, res);
			return;
		}

		if (!onlyWindows) 
		{
			// internal widget have the tag multi! 
			// is a multi task widget ?
			if (internalWidget.existsKey(dragId)) {
				var val = internalWidget.getValues(dragId);
				if (val[0] == false && (val[1] != "")) 
				{	// exist the windows id ?
					if (jq$('#' + val[1]).size() != 0)
						// exist
						return;
				}
			}

			_id = getWidGetId(dragId);
			jq$("#desktop").append(win.startDragAndDrop(_id, "", res));
			win.loadWidget(dragId, _id);
			// is internal widget ? update
			if (internalWidget.existsKey(dragId)) { // yes!
				internalWidget.insert(dragId, [internalWidget.getValues(dragId)[0], _id]);

				setInterval(function () {
						tagCloudUpdateWidget()
					},
					400);
			}
		}
		else
		{
			_id = dragId;
			jq$("#desktop").append(win.startDragAndDrop(_id, "", res));
		}

		desktopIsFull();
		// enable event drag & drop
		startWidgetEvent(_id);

		return _id;
	}

	// windows resize
	function goResizeWindowsEvent() {

		// set dock background positions
		var wMenuIco = jq$("#tbmenu_icons").width() + 30;
		var wSize = jq$("#" + widgetObj[0].wobj).width();

		var dockStartPos = jq$("#" + widgetObj[0].wobj).position().left - wSize + wMenuIco;
		var dockStopPos = jq$("#" + widgetObj[widgetObj.length - 1].wobj).position().left + wSize * 2 + wMenuIco;

		var canvas_border = 50;

		jq$('.mac-background-bar-in').offset({ left: dockStartPos })
		jq$('.mac-background-bar-in').width(dockStopPos - dockStartPos);
		jq$('.mac-background-bar-sx').offset({ left: dockStartPos - 17 })
		jq$('.mac-background-bar-dx').offset({ left: dockStopPos })

		// virtual desktop Size
		vDeskBoxSizeW = (doc.width() / VDESK_COL - (VDESK_MAR * (VDESK_COL + 1))) - canvas_border;
		vDeskBoxSizeH = (doc.height() / VDESK_ROW - (VDESK_MAR * (VDESK_ROW + 1))) - canvas_border;
		vDeskScale = (vDeskBoxSizeW / doc.width()) - 0.1;
	}

	function LoadReportCache(NameSpace, Title) {
		jq$.ajax({
		    url: 'MenuItemsPage.aspx',
			data: { "NameSpace": NameSpace, "Title": Title },
			cache: false,
			dataType: "json",
			success: function (data) {
				MenuReportCache.insert(NameSpace + Title, data);
			},
			error: function (e, exception) {
				alert("LoadReportCache load error! -  " + e.status + " - " + exception);
			}
		});
	}

	function LoadMenuAreaCache(NameSpace) {
		jq$.ajax({
			url: 'tbMenuArea.aspx',
			data: { "NameSpace": NameSpace },
			cache: false,
			dataType: "json",
			success: function (data) {
				// Cache menu 
				MenuAreaCache.insert(NameSpace, data.menudata);
				function recMenuArea(dataAr) {
					jq$.each(dataAr, function (i, obj) {
						if (obj.Child) {
							recMenuArea(obj.ChildNodes);
						}
						else {
							LoadReportCache(obj.NameSpace, obj.TitleValue);
						}
					});
				}
				recMenuArea(data.menudata);
			},
			error: function (e, exception) {
				alert("LoadMenuAreaCache load error! -  " + e.status + " - " + exception);
			}
		});
	}

	function loadAppPageMenu() {
		// load tb menu
		jq$.ajax({
			url: 'tbAppPage.aspx',
			cache: false,
			dataType: "json",
			success: function (data) {
				var rHtml = "";
				var icoHtml = "";
				jq$.each(data.menudata, function (i, obj) {

					rHtml += "<div class='appGroupMenuLabel'>";
					rHtml += "<div class='AppMenuLabel'>";
					rHtml += obj.applicationTitle;
					rHtml += "</div>";
					rHtml += "</div>";

					icoHtml += "<div class='appGroupMenuIcon'>";
					icoHtml += "<img src='../" + obj.Image + "' alt=''>";
					icoHtml += "</div>";


					jq$.each(obj.Items, function (i, objItem) {

						LoadMenuAreaCache(objItem.appTitleMenuLabel);

						rHtml += "<div class='appTitleMenuLabel' id='" + objItem.appTitleMenuLabel + "'>";
						rHtml += "<div class='AppMenuLabel'>";
						rHtml += objItem.Url;
						rHtml += "</div>";
						rHtml += "</div>";

						icoHtml += "<div class='appTitleMenuIcon' id='" + objItem.appTitleMenuLabel + "'>";
						icoHtml += "<img src='../" + objItem.Image + "' alt=''>";
						icoHtml += "</div>";
					});


				});

				jq$("#tbmenu_icons").html(icoHtml);
				jq$("#tbmenu_main").html(rHtml);
			},
			error: function (e, exception) {
				alert("tbAppPage.aspx load error! -  " + e.status + " - " + exception);
			}
		});
	}

	// show virtual desktop expose
	function goExpose() 
	{
		makeDesktopCanvas();
		showDesktopCanvas();
	}

	// refesh authentication Token to load page.
	refreshAuthenticationtoken();

	// load app list
	loadAppPageMenu()

	// read json data
	jq$.ajax({
		url: 'widget/widget.aspx',
		cache: false,
		dataType: "json",
		success: function (data) {
			// iterate widget
			jq$.each(data.widget, function (i, obj) {
				// load widget css
				var cssSrc = "widget/" + obj.wobj + "/widget.css";
				loadCSS(cssSrc);
				//load widget bar (bar_bottom)
				jq$(".mac-dock").append('<img src= "widget/' + obj.ico +
    			'" id="' + obj.wobj +
    			'" title="' + obj.title +
				'" resizable="' + obj.resizable +
				'" class="widget" >');
			});

			// load internal widget
			jq$.each(data.internalWidget, function (i, obj) {
				var cssSrc = "widget/" + obj.wobj + "/widget.css";
				loadCSS(cssSrc);

				internalWidget.insert(obj.wobj, [obj.multi, ""]);

				jq$(".mac-dock").append('<img src= "widget/' + obj.ico +
    			'" id="' + obj.wobj +
    			'" title="' + obj.title +
				'" resizable="' + obj.resizable +
				'" class="widget" >');
			});

			widgetObj = data.widget;
			// widget bar osx style
			jq$('.mac-dock img').resizeOnApproach();

			goResizeWindowsEvent();


			jq$(".widget").draggable({
				helper: 'clone',
				containment: 'desktop'
			});

			jq$("#desktop").droppable({
				drop: function (event, ui) {
					var dragId = jq$(ui.draggable).attr('id');
					var res = (jq$(ui.draggable).attr('resizable') == "true");
					jq$(ui.draggable).css('left', '');
					jq$(ui.draggable).css('top', '');

					if (jq$('#draggable').size() != 0) {
						if (jq$(ui.draggable).hasClass('widget')) {
							var newid = startNewWidgetDragAndDrop(dragId, false, res);
							// drag & drop widget on Windows
							jq$('#draggable').before(jq$('#' + newid));
							jq$(jq$('#' + newid)).after(jq$('#limit_' + newid));
							draggable_remove();
							return;
						}
						jq$('#draggable').before(jq$(ui.draggable));
						jq$(ui.draggable).after(jq$('#limit_' + jq$(ui.draggable).attr('id')));
						draggable_remove();
					}

					if (!jq$(ui.draggable).hasClass('widget')) {
						return false;
					}

					startNewWidgetDragAndDrop(dragId, false, res);
				}
			});

			// load startup widget    	
			jq$.each(data.startup, function (i, obj) {
				startNewWidgetDragAndDrop(obj.wobj, false, jq$('#' + obj.wobj).attr('resizable') == "true");
			});

		},
		error: function (e, exception) {
			alert("Widget load error! -  " + e.status + " - " + exception);
		}
	});
	
	// init desktop
	jq$("#vdesk_container").append('<span class="vdesk_add"></span>');
	vdesk_container_center();

	// set event
	doc.mousemove(function (event) {
		var msg = "Handler for .mousemove() called at ";
		msg += event.pageX + ", " + event.pageY;

		var deskW = doc.width();
		var deskH = doc.height();
		var mouseX = event.pageX;
		var mouseY = event.pageY;

		// angle active
		// top  -  left
		if (mouseY < BORDER_SENSIBILITY_Y && mouseX < BORDER_SENSIBILITY_X) {
			eval(AngleActiveCallApp_TopLeft);
			return;
		}
		// top  -  right
		if (mouseY < BORDER_SENSIBILITY_Y && mouseX > (deskW - BORDER_SENSIBILITY_X)) {
			eval(AngleActiveCallApp_TopRight);
			return;
		}
		// down -  left
		if (mouseY > (deskH - BORDER_SENSIBILITY_Y) && mouseX < BORDER_SENSIBILITY_X) {
			eval(AngleActiveCallApp_DownLeft);
			return;
		}
		// down -  right
		if (mouseY > (deskH - BORDER_SENSIBILITY_Y) && mouseX > (deskW - BORDER_SENSIBILITY_X)) {
			eval(AngleActiveCallApp_DownRight);
			return;
		}

		// border top
		if (mouseY < BORDER_SENSIBILITY_Y) {
			jq$("#vdesk_container").show("drop", { direction: "up" }, 500, vdesk_container_center);
		}
		else
			jq$("#vdesk_container").hide("drop", { direction: "up" }, 500);

	});

	doc.keyup(function (e) {
		if (e.keyCode == 17)
			isCtrl = false;
	}).keydown(function (e) {
		if (e.keyCode == 17)
			isCtrl = true;

		if (e.keyCode == 27)
			jq$("div.messageboxModal").hide(1500);

		if (e.keyCode == 112 && isCtrl == true) {
			// Ctrl + F1 expose.!
			goExpose();
			e.preventDefault();
		}

	});

	// context menu diasble default IE menu
	doc.bind("contextmenu", function (e) {
		//return false;
		jq$('#WidgetContextMenu').hide();
	});

	doc.on('click', '#desktop', function () {
		jq$('#WidgetContextMenu').hide();
	});

	doc.on('click', 'div.WidgetContextMenuText', function () {
		jq$('#WidgetContextMenu').hide();
		var toDeskop = jq$(this).attr('data-parameter1');
		var widgetId = jq$(this).attr('data-parameter2');
		var widHtml = jq$("#" + widgetId).outerHTML();
		jq$("#" + widgetId).remove();
		jq$("#limit_" + widgetId).remove();
		if (toDeskop == 'new') {
			newDeskPageWidMove(widHtml, widgetId);
			return;
		}
		var vdeskWidget = vDesk.getValues(toDeskop)
		vdeskWidget[0] = vdeskWidget[0] + widHtml + "<div class='rightLimit' id='limit_" + widgetId + "'></div>";
		vdeskWidget[1].push(widgetId);
		vDesk.insert(vDeskActive, vdeskWidget);
		desktopChange(toDeskop);
	});

	doc.on('click', 'canvas.vdesk_canvas', function () {
		jq$("div.messageboxModal").hide(1500);
		desktopChange(jq$(this).attr('data-parameter1'));
	});

	// Remove desktop
	doc.on('click', 'a.vdesk_canvasDel', function () {
		desktopDelete(jq$(this).attr('data-parameter1'));
	});

	doc.on('click', 'div.arrow', function () {
		if (jq$(this).attr('id') == "arrow_up") {
			jq$('#canvas_container').scrollTop(jq$('#canvas_container').scrollTop() - 100);
		}
		else {
			jq$('#canvas_container').scrollTop(jq$('#canvas_container').scrollTop() + 100);
		}
	});

	doc.on('click', 'div.vdesk_close', function () {
		jq$("div.messageboxModal").hide(1500);
	});

	doc.on('click', 'span.vdesk_add', function () {
		newDeskPageClear();
	});

	doc.on('click', 'span.vdesk', function () {
		desktopChange(jq$(this).attr('data-parameter1'));
	});

	jq$(window).resize(function () {
		goResizeWindowsEvent();
	});

	// ================================================================================
	// menu fadeIn / fadeOut


	function closeAllTbMenu() 
	{
		jq$("#tbmenu_report").fadeOut(100);
		jq$("#tbmenu_area").fadeOut(100);
		jq$("#tbmenu_main").fadeOut(100);
	}

	jq$("#desktop").hover(function () {
		closeAllTbMenu();
	});

	doc.on('mouseenter', 'div.appTitleMenuIcon', function () {
		// alert(jq$(this).attr('id'));
		jq$("#tbmenu_main").fadeIn(500);
	});

	doc.on('mouseenter', 'div.AppMenuLabel', function () {

	});

	doc.on('mouseenter', 'div.appGroupMenuLabel', function () {
		jq$("#tbmenu_area").fadeOut(100);
	});
	
	// ================================================================================

	doc.on('click', 'div.window', function () {
		win.windowTop(jq$(this).attr('id'));
	});
		
	// Close the window.
	doc.on('click', 'a.window_close', function () {
		win.close(jq$(this).attr('id'));
	});

    // ico click
    doc.on('click', 'a.icon', function () {
    	var idAtt = jq$(this).attr('id');
		ret = eval(idAtt + "()");
    });
	 
	function getIcoReportPage(NameSpaceType) 
	{
		var rHtml = "";

		if (NameSpaceType == "report")
			rHtml = "<img alt='' src='images/RunReport.gif' class='commandNodeImage'>";
		else if (NameSpaceType == "fuction")
			rHtml = "<img alt='' src='images/RunDocumentFunction.png' class='commandNodeImage'>";
		else if (NameSpaceType == "document")
			rHtml = "<img alt='' src='images/RunDocument.gif' class='commandNodeImage'>";
		else if (NameSpaceType == "runbatch")
			rHtml = "<img alt='' src='images/RunBatch.png' class='commandNodeImage'>";
		else if (NameSpaceType == "application")
			rHtml = "<img alt='' src='images/Application.png' class='commandNodeImage'>";
		else if (NameSpaceType == "WordDocument" ||
				 NameSpaceType == "WordDocument2007" ||
				 NameSpaceType == "WordTemplate" ||
				 NameSpaceType == "WordTemplate2007")
			rHtml = "<img alt='' src='images/Word.png' class='commandNodeImage'>";
		else if (NameSpaceType == "ExcelDocument" ||
				 NameSpaceType == "ExcelDocument2007" ||
				 NameSpaceType == "ExcelTemplate" ||
				 NameSpaceType == "ExcelTemplate2007")
			rHtml = "<img alt='' src='images/Excel.png' class='commandNodeImage'>";

		else if (NameSpaceType == "text")
			rHtml = "<img alt='' src='images/RunText.png' class='commandNodeImage'>";

		return rHtml;
	}

	function newWidgetMenu(NameSpace, Title) {
		var _id = NameSpace + Title;
		_id = _id.replace("\\\\", "_").replace("\\", "_");

		if (jq$("#" + _id).length != 0) {
			return;
		}

		jq$.ajax({
		    url: 'MenuItemsPage.aspx',
			data: { "NameSpace": NameSpace, "Title": Title },
			cache: false,
			dataType: "json",
			success: function (data) {
				var rHtml = "";
				jq$.each(data.menudata, function (i, obj) {
					rHtml += "<div class='commandNodeReport' data-parameter1='" + obj.Url + "'>";
					rHtml += getIcoReportPage(obj.NameSpaceType);
					var arg = "";
					if (obj.arguments && obj.arguments != "") arg = obj.arguments;
					rHtml += obj.Title;
					rHtml += "</div>";
					// report page cache insert 
					reportPageCache.insert(obj.Url, [obj.Title, obj.NameSpaceType, arg]);
				});

				rHtml = '<div class="reportPage" id="reportPage_' + _id + '">' + rHtml + '<div>';

				if (desktopIsFull()) {
					// new desktop and insert the menu
					rHtml = win.startDragAndDrop(_id, rHtml, true);
					newDeskPageWidMove(rHtml, _id);
				}
				else {
					startNewWidgetDragAndDrop(_id, true, true);
					jq$("#content_" + _id).append(rHtml);
				}

				// Todo: DP-da vedere.
				jq$("#reportPage_" + _id).height(300);
			},
			error: function (e, exception) {
			    alert("MenuItemsPage.aspx load error! -  " + e.status + " - " + exception);
			}
		});
		
		return _id;
	}

	// upate tag cloud internal widget
	function tagCloudUpdateWidget() 
	{
		// widget tag cloud in use ?
		if (jq$("#tagCloudIw").length == 0)
			return;

		var rTagTopHtml = new Dictionary();
		var rHtml = "";
		// order for load and show tag cloud
		jq$.each(Object.keys(tagCloudMenu.getDictionary()), function (i, u) {
			var rPage = reportPageCache.getValues(u);

			rHtml = "";
			rHtml += "<div class='commandNodeReport' data-parameter1='" + u + "'>";
			rHtml += getIcoReportPage(rPage[1]);
			rHtml += rPage[0];
			rHtml += "</div>";

			load = tagCloudMenu.getValues(u);
			var arHtml = [];
			if (rTagTopHtml.existsKey(load))
				arHtml = rTagTopHtml.getValues(load);
			arHtml.push(rHtml);
			rTagTopHtml.insert(load, arHtml);
		});
		rHtml = "";
		var l = 0;
		var key = Object.keys(rTagTopHtml.getDictionary());
		// sort and reverse the list and make the html code
		jq$.each(key.sort().reverse(), function (i, k) {
			jq$.each(rTagTopHtml.getValues(k), function (i, html) {
				var fontClass = "tagcloud_font_top";
				if (l == 1)
					fontClass = "tagcloud_font_mid";
				if (l > 1)
					fontClass = "tagcloud_font";

				rHtml += "<div class='" + fontClass + "'>";
				rHtml += html;
				rHtml += "</div>";

				l++;
			});
		});

		jq$("#tagCloudIw").html(rHtml);	
	}

	// Mouse Enter in link
	doc.on('mouseenter', 'div.appTitleMenuLabel', function () {
		var NameSpace = jq$(this).attr('id');
		if (jq$("#onemenuarea").length != 0)
			jq$("#onemenuarea").remove();

		if (onemenuarea_lastopen == this) {
			onemenuarea_lastopen = null;
			return;
		}

		// ==========================
		refreshAuthenticationtoken();

		// Request use for reset cache!
		onemenuarea_lastopen = this;


		var rHtml = "";
		var treeN = 0;

		function recMenuArea(dataAr) {
			jq$.each(dataAr, function (i, obj) {
				if (obj.Child) {
					treeN++;
					rHtml += "<div class='menuareaTree' id='treeNode_" + treeN + "_ico'>";

					rHtml += "<li><span class='menuareaTreeLabel' data-parameter1='treeNode_" + treeN + "'>";
					rHtml += obj.Title;
					rHtml += "</span></li>";

					rHtml += "<div class='treeNode' id='treeNode_" + treeN + "'>";
					recMenuArea(obj.ChildNodes);
					rHtml += "</div>";

					rHtml += "</div>";
				}
				else {
					rHtml += "<div class='menuareaTitle' id='" + obj.NameSpace + "' data-parameter1='" + obj.TitleValue + "'>";
					rHtml += "<li><span class='menuareaTitleLabel'>";
					rHtml += obj.Title;
					rHtml += "</span></li>";
					rHtml += "</div>";
				}
			});
		}

		recMenuArea(MenuAreaCache.getValues(NameSpace));

		jq$("#tbmenu_area").html(rHtml);
		jq$("#tbmenu_area").fadeIn(200);
		jq$("#tbmenu_report").fadeOut(100);
	});

	doc.on('mouseenter', 'span.menuareaTreeLabel', function () {

		var idTree = jq$(this).attr('data-parameter1');


		if (lastIdTreeLabel == idTree)
			return;

		if (lastIdTreeLabel != "") {
			if (jq$('#' + lastIdTreeLabel + '_ico').hasClass("open")) {
				jq$('#' + lastIdTreeLabel).toggle();
				jq$('#' + lastIdTreeLabel + '_ico').removeClass("open");
			}
		}

		lastIdTreeLabel = idTree;
		jq$('#' + idTree).toggle();
		jq$('#' + idTree + '_ico').addClass("open");
	});

	doc.on('dblclick', 'div.menuareaTitle', function (event) {
		var NameSpace = jq$(this).attr('id');
		var Title = jq$(this).attr('data-parameter1');
		event.stopPropagation();
		if (Title == "") return;
		newWidgetMenu(NameSpace, Title);
		closeAllTbMenu();
	});

	doc.on('mouseenter', 'div.menuareaTitle', function (event) {
		var NameSpace = jq$(this).attr('id');
		var Title = jq$(this).attr('data-parameter1');
		if (Title == "") return;
		
		event.stopPropagation();


		var rHtml = "";
		jq$.each(MenuReportCache.getValues(NameSpace + Title).menudata, function (i, obj) {
			// make html menu
			rHtml += "<div class='commandNodeReport' data-parameter1='" + obj.Url + "'>";
			rHtml += getIcoReportPage(obj.NameSpaceType);
			var arg = "";
			if (obj.arguments && obj.arguments != "") arg = obj.arguments;
			rHtml += obj.Title;
			rHtml += "</div>";
			// report page cache insert 
			reportPageCache.insert(obj.Url, [obj.Title, obj.NameSpaceType, arg]);
		});
		jq$("#tbmenu_report").html(rHtml);
		jq$("#tbmenu_report").fadeIn(500);
	});

	doc.on('click', 'div.commandNodeReport', function () {
		// tag Cloud for statistics
		var url = jq$(this).attr('data-parameter1');

		// load TB document todo:
		openTBUrl(url);

		// close all tb menu
		closeAllTbMenu();

		// tag cloud load
		var n = 1
		if (tagCloudMenu.existsKey(url))
			n = tagCloudMenu.getValues(url) + 1;
		tagCloudMenu.insert(url, n);

		tagCloudUpdateWidget();
		// refresh authentication token
		refreshAuthenticationtoken();
	});

	
}