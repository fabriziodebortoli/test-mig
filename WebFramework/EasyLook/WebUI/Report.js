/// <reference path="Persistence.js" />

/// <reference path="jquery.min.js" />

/// <reference path="jquery-ui.min.js" />
/// <reference path="Widget.js" />
/// <reference path="/CommonFunctions.js" />
/// <reference path="/DbCatalog.js />

/* Simple JavaScript Inheritance
* By John Resig http://ejohn.org/
* MIT Licensed.
*/
// Inspired by base2 and Prototype
(function () {
    var initializing = false, fnTest = /xyz/.test(function () { xyz; }) ? /\b_super\b/ : /.*/;
    // The base Class implementation (does nothing)
    this.Class = function () { };

    // Create a new Class that inherits from this class
    Class.extend = function (prop) {
        var _super = this.prototype;

        // Instantiate a base class (but only create the instance,
        // don't run the init constructor)
        initializing = true;
        var prototype = new this();
        initializing = false;

        // Copy the properties over onto the new prototype
        for (var name in prop) {
            // Check if we're overwriting an existing function
            prototype[name] = typeof prop[name] == "function" &&
        typeof _super[name] == "function" && fnTest.test(prop[name]) ?
        (function (name, fn) {
            return function () {
                var tmp = this._super;

                // Add a new ._super() method that is the same method
                // but on the super-class
                this._super = _super[name];

                // The method only need to be bound temporarily, so we
                // remove it when we're done executing
                var ret = fn.apply(this, arguments);
                this._super = tmp;

                return ret;
            };
        })(name, prop[name]) :
        prop[name];
        }

        // The dummy class constructor
        function Class() {
            // All construction is actually done in the init method
            if (!initializing && this.init)
                this.init.apply(this, arguments);
        }

        // Populate our constructed prototype object
        Class.prototype = prototype;

        // Enforce the constructor to be what we expect
        Class.prototype.constructor = Class;

        // And make this class extendable
        Class.extend = arguments.callee;

        return Class;
    };
})();


var tb$ = jQuery.noConflict();



//*******************<ALIGNMANAGER CODE>*******************
//Gestore allineamento testo negli elementi contenitore del report
function AlignManager() {
	//Singleton pattern
	if (arguments.callee.instance)
		return arguments.callee.instance;
	arguments.callee.instance = this;

    //esempio di utilizzo <istanza align manager>.ALIGNCONST.get('MY_CONST')); 
    this.ALIGNCONST = (function () {
        var private = {
            'DT_TOP': 0x00000000,
            'DT_LEFT': 0x00000000,
            'DT_CENTER': 0x00000001,
            'DT_RIGHT': 0x00000002,
            'DT_VCENTER': 0x00000004,
            'DT_BOTTOM': 0x00000008,
            'DT_WORDBREAK': 0x00000010,
            'DT_SINGLELINE': 0x00000020,
            'DT_EXPANDTABS': 0x00000040,
            'DT_TABSTOP': 0x00000080,
            'DT_NOCLIP': 0x00000100,
            'DT_EXTERNALLEADING': 0x00000200,
            'DT_CALCRECT': 0x00000400,
            'DT_NOPREFIX': 0x00000800,
            'DT_INTERNAL': 0x00001000,

            'DT_EX_VCENTER_LABEL': 0x00010000,
            'DT_EX_FIELD_SET': 0x00020000,
            'DT_EX_90': 0x00040000,
            'DT_EX_270': 0x00080000
        };

        return {
            get: function (name) { return private[name]; }
        };

    })();

    this.Align = function (textContainer, rect, align) {
        //centratura orizzontale
        //default left
        if ((align & this.ALIGNCONST.get('DT_RIGHT')) == this.ALIGNCONST.get('DT_RIGHT'))
            textContainer.css("text-align", "right");
        else if ((align & this.ALIGNCONST.get('DT_CENTER')) == this.ALIGNCONST.get('DT_CENTER'))
            textContainer.css("text-align", "center");
        else
            textContainer.css("text-align", "left");

        //centratura verticale
        //default top
        if (align & this.ALIGNCONST.get('DT_VCENTER')) {
            /*
		
            */
        }
        if (align & this.ALIGNCONST.get('DT_BOTTOM')) {
            /*
		
            */
        }
    }
}

AlignManager.getInstance = function () {
	var _alignManagerInstance = new AlignManager();
	return _alignManagerInstance;
}
//*******************</ALIGNMANAGER CODE>*******************


//*******************<UNDO MANAGER CODE>*******************
function UndoManager() {

	//Singleton pattern
	if (arguments.callee.instance)
		return arguments.callee.instance;
	arguments.callee.instance = this;

	// private
	var commandStack = [],
		index = -1,
		undoManagerContext = false,
		callback;

	function execute(command) {
		if (!command) {
			return;
		}
		undoManagerContext = true;
		command.f.apply(command.o, command.p);
		undoManagerContext = false;
	}

	function createCommand(undoObj, undoFunc, undoParamsList, undoMsg, redoObj, redoFunc, redoParamsList, redoMsg) {
		return {
			undo: { o: undoObj, f: undoFunc, p: undoParamsList, m: undoMsg },
			redo: { o: redoObj, f: redoFunc, p: redoParamsList, m: redoMsg }
		};
	}

	// public
	return {

		/*
		Registers an undo and redo command. Both commands are passed as parameters and turned into command objects.
		param undoObj: caller of the undo function
		param undoFunc: function to be called at myUndoManager.undo
		param undoParamsList: (array) parameter list
		param undoMsg: message to be used
		*/
		register: function (undoObj, undoFunc, undoParamsList, undoMsg, redoObj, redoFunc, redoParamsList, redoMsg) {
			if (undoManagerContext) {
				return;
			}

			// if we are here after having called undo,
			// invalidate items higher on the stack
			commandStack.splice(index + 1, commandStack.length - index);

			commandStack.push(createCommand(undoObj, undoFunc, undoParamsList, undoMsg, redoObj, redoFunc, redoParamsList, redoMsg));

			// set the current index to the end
			index = commandStack.length - 1;
			if (callback) {
				callback();
			}
		},

		/*
		Pass a function to be called on undo and redo actions.
		*/
		setCallback: function (callbackFunc) {
			callback = callbackFunc;
		},

		undo: function () {
			var command = commandStack[index];
			if (!command) {
				return;
			}
			execute(command.undo);
			index -= 1;
			if (callback) {
				callback();
			}
		},

		redo: function () {
			var command = commandStack[index + 1];
			if (!command) {
				return;
			}
			execute(command.redo);
			index += 1;
			if (callback) {
				callback();
			}
		},

		/*
		Clears the memory, losing all stored states.
		*/
		clear: function () {
			var prev_size = commandStack.length;

			commandStack = [];
			index = -1;

			if (callback && (prev_size > 0)) {
				callback();
			}
		},

		hasUndo: function () {
			return index !== -1;
		},

		hasRedo: function () {
			return index < (commandStack.length - 1);
		}
	};
};

UndoManager.getInstance = function () {
	var _undoManagerInstance = new UndoManager();
	return _undoManagerInstance;
}

var undoManagerInstance = UndoManager.getInstance();
//*******************</UNDO MANAGER CODE>*******************


//*******************<SHORTCUT KEYS MANAGEMENT CODE>*******************
tb$(document).ready(
function () {
	var ctrlDown = false;
	var zKey = 90, yKey = 89;

	tb$(document).keydown(function (e) {

		if (e.ctrlKey == true) {
			if (e.keyCode == zKey)
				undoManagerInstance.undo();
			if (e.keyCode == yKey)
				undoManagerInstance.redo();
		}
	});
});
//*******************</SHORTCUT KEYS MANAGEMENT CODE>*******************


//*******************<SELECTION MANAGER CODE>*******************
//Gestore selezioni con mouse e drag&drop
function SelectionManager() {

	//Singleton pattern
	if (arguments.callee.instance)
		return arguments.callee.instance;
	arguments.callee.instance = this;

	this.addSelectableObject = function (object) {
		var _htmlobject = object._htmlObject;
		var _data = object._data;

		//temp var to store original values
		var _horizontalSelectionLines = null;
		var _verticalSelectionLines = null;
		var _originalPosition = null;

		//funzione per undo/redo degli spostamenti
		function moveObject(object, position, width, height, baseRect) {
			object.css("top", position.top);
			object.css("left", position.left);

			if (width != undefined && baseRect != undefined)
			{
				object.css("width", width);
				baseRect['width'] = width;
			}
			if (height != undefined && baseRect != undefined)
			{
				object.css("height", height);
				baseRect['height'] = height;
			}
			
			if (baseRect != undefined)
			{
				baseRect['x'] = position.left;
				baseRect['y'] = position.top;
			
				fillPropertyGrid(baseRect.objectRoot, true);
				baseRect.objectRoot.updateDataServerSide();
			}
		}

		var resizeOpts = {
			handles: 'all',
			autoHide: true
		};

		_htmlobject.bind(	'resizestart',
							function (event, info) {
								info.helper.addClass("objectSelected");
								info.helper.css("z-index", "1000");

								_originalPosition = info.helper.position();
								_originalWidth = info.helper.position();
								_originalHeight = info.helper.height();
								_originalWidth = info.helper.width();
							});



		_htmlobject.bind(	'resizestop',
							{ ownerObj: _data },
							function (event, info) {
								info.helper.removeClass("objectSelected").css("z-index", "auto");
								var newPos = info.helper.position();
								var newHeight = info.helper.height();
								var newWidth = info.helper.width();

								undoManagerInstance.register(this, moveObject, [tb$(this), _originalPosition, _originalWidth, _originalHeight, event.data.ownerObj.BaseRect], 'Prova undo', this, moveObject, [tb$(this), newPos, newWidth, newHeight, event.data.ownerObj.BaseRect], 'Prova redo');

								moveObject(tb$(this), newPos, newWidth, newHeight, event.data.ownerObj.BaseRect);
							});



		_htmlobject.draggable({ containment: "parent" }).resizable(resizeOpts);

		_htmlobject.bind('dragstart',
						function (event, info) {
							info.helper.addClass("objectSelected");
							info.helper.removeClass("objectSelectedSlave");
							_originalPosition = info.helper.position();
							info.helper.css("z-index", "1000");

							posTopArray = [];
							posLeftArray = [];
							tb$(".objectSelectedSlave").each(function (i) {
								thiscsstop = tb$(this).css('top');
								thiscssleft = tb$(this).css('left');
								posTopArray[i] = parseInt(thiscsstop);
								posLeftArray[i] = parseInt(thiscssleft);
							});


							begintop = tb$(this).offset().top;
							beginleft = tb$(this).offset().left;
						}
					);

		_htmlobject.bind('drag',
						{ ownerObj: _data },
						function (event, info) {

							if (_horizontalSelectionLines != null)
								_horizontalSelectionLines.remove();
							if (_verticalSelectionLines != null)
								_verticalSelectionLines.remove();

							var top = info.helper.position().top;
							var left = info.helper.position().left;
							var h = info.helper.height();
							var w = info.helper.width();

							_horizontalSelectionLines = tb$("<div style='top:" + top + "px; height: " + h + "px;'></div>");
							_horizontalSelectionLines.addClass("horizontalSelectionLines");
							info.helper.parent().append(_horizontalSelectionLines);

							_verticalSelectionLines = tb$("<div style='left:" + left + "px; width: " + w + "px;'></div>");
							_verticalSelectionLines.addClass("verticalSelectionLines");
							info.helper.parent().append(_verticalSelectionLines);


							var topdiff = tb$(this).offset().top - begintop;
							var leftdiff = tb$(this).offset().left - beginleft;


							tb$(".objectSelectedSlave").each(function (i) {
								tb$(this).css('top', posTopArray[i] + topdiff);
								tb$(this).css('left', posLeftArray[i] + leftdiff);
							});
						}
					);


		_htmlobject.bind('dragstop',
						{ ownerObj: _data },
						function (event, info) {

							if (_horizontalSelectionLines != null)
								_horizontalSelectionLines.remove();
							if (_verticalSelectionLines != null)
								_verticalSelectionLines.remove();

							try {
								var topdiff = tb$(this).offset().top - begintop;
								var leftdiff = tb$(this).offset().left - beginleft;

								tb$(".objectSelectedSlave").each(function (i) {
									var _originalPos = { left: posLeftArray[i], top: posTopArray[i] };
									var _newPos = { left: posLeftArray[i] + leftdiff, top: posTopArray[i] + topdiff };

									undoManagerInstance.register(this, moveObject, [tb$(this), _originalPos, undefined, undefined, undefined], 'Prova undo', this, moveObject, [tb$(this), _newPos, undefined, undefined, undefined], 'Prova redo');
								});

								moveObject(info.helper, info.position, undefined, undefined, event.data.ownerObj.BaseRect);
								undoManagerInstance.register(info.helper, moveObject, [info.helper, _originalPosition, undefined, undefined, event.data.ownerObj.BaseRect], 'Prova undo', info.helper, moveObject, [info.helper, info.position, undefined, undefined, event.data.ownerObj.BaseRect], 'Prova redo');
							}
							catch (err) {
								alert("Error updating property grid and data on server!!")
							}

							tb$(".objectSelected").each(function (i) {
								tb$(this).removeClass("objectSelected").css("z-index", "auto");
							});

							tb$(".objectSelectedSlave").each(function (i) {
								tb$(this).removeClass("objectSelectedSlave").css("z-index", "auto");
							});
						}
					);
	}


		this.addToSelection = function (object) {
			var _htmlobject = object._htmlObject;
			_htmlobject.addClass("objectSelectedSlave");
		}
}

SelectionManager.getInstance = function () {
	var selectionManagerInstance = new SelectionManager();
	return selectionManagerInstance;
}

//*******************<SELECTION MANAGER CODE>*******************	

//Funzioni legate ai bottoni delle toolbar

function saveServerSide() {
    tb$.get('WoormHandler.axd/SaveReport', window.sessionData, function (result) {
        var report = tb$("<div id='dialog' title='Report script'></div>").html(result.replace(new RegExp("\\n", "g"), "<br>")).dialog({ resizable: true, draggable: true, width: 900 });
    });
}

function scrollPrevPage() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/ScrollPrevPage', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getReportData(createReport) }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server: ' + data.message);
            else
                createReport(data.reportObjects, data.paperLength, data.paperWidth);
        }
    })
	.error(function () { alert("Error calling the server to get report json data!"); });
}

function scrollNextPage() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/ScrollNextPage', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getReportData(createReport) }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server: ' + data.message);
            else
                createReport(data.reportObjects, data.paperLength, data.paperWidth);
        }
    })
	.error(function () { alert("Error calling the server to get report json data!"); });
}

function scrollFirstPage() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/ScrollFirstPage', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getReportData(createReport) }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server: ' + data.message);
            else
                createReport(data.reportObjects, data.paperLength, data.paperWidth);
        }
    })
	.error(function () { alert("Error calling the server to get report json data!"); });
}

function scrollLastPage() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/ScrollLastPage', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getReportData(createReport) }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server: ' + data.message);
            else
                createReport(data.reportObjects, data.paperLength, data.paperWidth);
        }
    })
	.error(function () { alert("Error calling the server to get report json data!"); });
}


function closeWindow() {
	window.close();
}



function getReportEngine() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/GetReportEngine', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getReportEngine() }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server');
            else {
                showVariables(data.report);
                showRules(data.rules);
            }
        }
    })
	.error(function () { alert("Error calling the server to get report engine json data!"); });
}

function showVariables(report) {
    var reportVars = tb$("<div id='dialog' title='Report variables'></div>")

    var tableFields = tb$("<table></table>");
    for (var i = 0; i < report.symtable.fields.symbols.length; i++) {
        var obj = report.symtable.fields.symbols[i];
        tableFields.append("<tr><td>" + obj.key + "</td></tr>");
    }

    reportVars.append(tableFields);
    reportVars.dialog({ resizable: true, draggable: true, width: 400, height: 500 });
}

function showRules(rules) {
    var reportRules = tb$("<div id='dialog1' title='Report rules'></div>")

    var tableRules = tb$("<table></table>");
    for (var i = 0; i < rules.length; i++) {
        tableRules.append("<tr><td>" + rules[i].replace(new RegExp("\\n", "g"), "<br>") + "</td></tr>");
    }

    reportRules.append(tableRules);
    reportRules.dialog({ resizable: true, draggable: true, width: 350, height: 500 });
}

function undo() {
	undoManagerInstance.undo(); 
}

function redo() {
	undoManagerInstance.redo();
}

function getDbObjects() {
    var jqxhr = tb$.getJSON('WoormHandler.axd/GetDBObjects', window.sessionData, function (data, textStatus, jqXHR) {
        if (!data.ready)
            setTimeout(function () { getDbObjects() }, 1000);
        else {
            if (data.error)
                alert('An error occurred on the server');
            else {
                catalog = new Catalog(data.catalog);
                catalog.show();
            }
        }
    })
	.error(function () { alert("Error calling the server to get DB objects json data!"); });
}




function Report(reportObjects, paperLength, paperWidth) {
    var _objects = reportObjects;
    var _paperLength = paperLength;
    var _paperWidth = paperWidth;

    this.create = function (parent) {

		var reportViewerToolbar = tb$("<table class='tbToolbar'><tr> \
														<td> <div id='toolbarEdit' class='tbToolbarButton' style='background-position: 0px 0px;'/> </td> \
														<td> <div id='toolbarNext' class='tbToolbarButton' style='background-position: 0px -25px;'/> </td> \
														<td> <div id='toolbarPrev' class='tbToolbarButton' style='background-position: 0px -50px;'/> </td> \
														<td> <div id='toolbarFirst' class='tbToolbarButton' style='background-position: 0px -75px;'/> </td> \
														<td> <div id='toolbarLast' class='tbToolbarButton' style='background-position: 0px -100px;'/> </td> \
														<td> <div id='toolbarRun' class='tbToolbarButton' style='background-position: 0px -125px;'/> </td> \
														<td> <div id='toolbarRDE' class='tbToolbarButton' style='background-position: 0px -175px;'/> </td> \
                                                        <td> <div id='toolbarPdf' class='tbToolbarButton' style='background-position: 0px -200px;'/> </td> \
														<td> <div id='toolbarXls' class='tbToolbarButton' style='background-position: 0px -224px;'/> </td> \
														<td> <div id='toolbarExit' class='tbToolbarButton' style='background-position: 0px -248px;'/> </td> \
								</tr></table>")                                                
			.appendTo(parent)
			.css({ width: 'auto' });
		
		//da lo stile bottone agli elementi della toolbar
		tb$(".tbToolbarButton").button();
		
		/*Toolbar di editor*/
		var reportEditToolbar = tb$("<table class='tbToolbar'><tr> \
																				<td> <div id='toolbarSave' class='tbToolbarEditorButton' style='background-position: 0px 0px;'/> </td> \
																				<td> <div id='toolbarSymTable' class='tbToolbarEditorButton' style='background-position: 0px -25px;'/> </td> \
																				<td> <div id='toolbarGetDbObjects' class='tbToolbarEditorButton' style='background-position: 0px -50px;'/> </td> \
																				<td> <div id='toolbarUndo' class='tbToolbarEditorButton' style='background-position: 0px -75px;'/> </td> \
																				<td> <div id='toolbarRedo' class='tbToolbarEditorButton' style='background-position: 0px -100px;'/> </td> \
									</tr></table>")
			.appendTo(parent)
			.css({ width: 'auto' });     
		
		//associa eventi ai bottoni della toolbar Viewer
		tb$("#toolbarEdit").click(function(){ reportEditToolbar.toggle();});
		tb$("#toolbarNext").click(scrollNextPage);
		tb$("#toolbarPrev").click(scrollPrevPage);
		tb$("#toolbarFirst").click(scrollFirstPage);
		tb$("#toolbarLast").click(scrollLastPage);
		tb$("#toolbarRun").click(runReport);
		tb$("#toolbarExit").click(closeWindow);

		//associa eventi ai bottoni della toolbar Editor
		tb$("#toolbarSave").click(saveServerSide);
		tb$("#toolbarSymTable").click(getReportEngine);
		tb$("#toolbarGetDbObjects").click(getDbObjects);
		tb$("#toolbarUndo").click(undo);
		tb$("#toolbarRedo").click(redo);

		//da lo stile bottone agli elementi della toolbar
		tb$(".tbToolbarEditorButton").button();

		reportEditToolbar.hide();

		

        //Crea area di disegno
        var reportPage = tb$("<div></div>")
			.addClass("tbReportPage")
			.appendTo(parent)
            .css({
                height: _paperLength,
                width: _paperWidth
            });

        function createObject(obj, f) {
            var newObject = new f();
            newObject.create(reportPage, obj);
            return newObject;
        }


        //Fine area disegno
        for (var i = 0; i < _objects.length; i++) {
            var obj = _objects[i];
            _objects[i] = createObject(obj, eval(obj.__type.substr(0, obj.__type.indexOf(':#'))));
        }
    }

    var _alignManagerInstance = AlignManager.getInstance();

    var BaseObj = Class.extend({
    	name: "BaseObj",
    	getHtmlObject: function () {
    		return this._htmlObject;
    	},

    	setBorders: function (borders, htmlObject) {
    		if (borders.Top)
    			htmlObject.addClass("tbBorderTop");

    		if (borders.Bottom)
    			htmlObject.addClass("tbBorderBottom");

    		if (borders.Left)
    			htmlObject.addClass("tbBorderLeft");

    		if (borders.Right)
    			htmlObject.addClass("tbBorderRight");
    	},

    	create: function (parent, jsonData) {
    		this._data = jsonData;
    		var _me = this;

    		if (!this._htmlObject) {
    			setObjectRoot(this._data);

    			this._htmlObject = tb$("<div></div>").appendTo(parent);
    			this._htmlObject._parent = parent;
    		}

    		function setObjectRoot(obj) {
    			for (var propName in obj) {
    				var propVal = obj[propName];
    				if (propVal == null)
    					continue;
    				var t = typeof propVal;
    				if (t != 'function') {
    					if (t == 'object') {
    						setObjectRoot(propVal);
    					}
    				}
    			}
    			obj.objectRoot = _me;
    		}

    		//impostazione attributi e stili dell'html generato

    		if (this._data.Hidden) {
    			this._htmlObject.css("visibility", "hidden");
    		}
    		else {
    			this._htmlObject
				.addClass("tbBaseObj")
				.css({ top: this._data.BaseRect.y + "px",
					left: this._data.BaseRect.x + "px",
					height: this._data.BaseRect.height + "px",
					width: this._data.BaseRect.width + "px"
				})
				.mousedown(function ()
				{ fillPropertyGrid(_me); })
				.click(
						function (evt) {
							if (evt.ctrlKey)
								SelectionManager.getInstance().addToSelection(_me);
						}
				);


    			/*gestione drag drop oggetti*/
    			SelectionManager.getInstance().addSelectableObject(_me);

    			//impostazione bordi
    			if (this._data.Borders) {
    				this.setBorders(this._data.Borders, this._htmlObject);
    			}
    		}
    	},




    	refresh: function () {
    		this.getHtmlObject().empty();
    		this.create(this.getHtmlObject()._parent, this._data);

    		//update data on server
    		this.updateDataServerSide();
    	},

    	setText: function (htmlObject, text, fontData) {
    		htmlObject.text(text);
    		if (fontData != null) {
    			htmlObject.css("font-family", fontData.Family);
    			htmlObject.css("font-size", fontData.Size);

    			if (fontData.Italic) {
    				htmlObject.css("font-style", "italic")
    			}
    			if (fontData.Bold) {
    				htmlObject.css("font-weight", "bold")
    			}
    			if (fontData.Underline) {
    				htmlObject.css("text-decoration", "underline");
    			}
    			if (fontData.Strikeout) {
    				htmlObject.css("text-decoration", "line-through");
    			}
    		}
    	},

    	updateDataServerSide: function () {
    		function replacer(key, value) {
    			if (key === 'objectRoot')
    				return undefined;  //evita ricorsione 
    			else
    				return value;
    		}

    		tb$.post('SetReportData', { __StateMachineSessionTag: window.sessionData['__StateMachineSessionTag'], JsonData: JSON.stringify(this._data, replacer) });
    	}
    });

    var FieldRect = BaseObj.extend({
        name: "FieldRect",
        create: function (parent, jsonData) {
            this._super(parent, jsonData);
            this.getHtmlObject().addClass("tbFieldRect");

            /*	var fieldLabelAndText = this._data.LocalizedText + this._data.Value.FormattedData;
            this.setText(fieldLabelAndText, this._data.Label.FontData);*/

            var htmlObject = this.getHtmlObject();
            htmlObject.addClass("tbFieldRect");

            //crate the DIV transparent for the label
            var labelText = this._data.LocalizedText
            var labelContainer = tb$("<div><span class='tbTransparentContainer'></span></div>")
            this.setText(labelContainer, labelText, this._data.Label.FontData);
            _alignManagerInstance.Align(labelContainer, this._data.BaseRect, this._data.Label.Align);
            htmlObject.append(labelContainer);

            //crate the DIV transparent for the value
            var fieldText = this._data.Value.FormattedData;
            var valueContainer = tb$("<div><span class='tbTransparentContainer'></span></div>");
            this.setText(valueContainer, fieldText, this._data.Value.FontData);
            _alignManagerInstance.Align(valueContainer, this._data.BaseRect, this._data.Value.Align);
            htmlObject.append(valueContainer);
        }
    });
    var SqrRect = BaseObj.extend({
        create: function (parent, jsonData) {
            this._super(parent, jsonData);
            this.getHtmlObject().addClass("tbSqrRect");
        }
    });

    var FileRect = BaseObj.extend({
        create: function (parent, jsonData) {
            this._super(parent, jsonData);
            this.getHtmlObject().addClass("tbFileRect");
        }
    });

    var TextRect = BaseObj.extend({
        create: function (parent, jsonData) {
            this._super(parent, jsonData);
            this.getHtmlObject().addClass("tbTextRect");
            var text = this._data.LocalizedText.length > 0 ? this._data.LocalizedText : this._data.Label.Text;
            this.setText(this.getHtmlObject(), text, this._data.Label.FontData);

            //impostazione colore
            this.getHtmlObject().css("color", this._data.TextColor);
			this.getHtmlObject().css("background-color", this._data.BackColor);
        }
    });

    var Table = BaseObj.extend({
        create: function (parent, jsonData) {
            var _me = this;
            this._super(parent, jsonData);
            this.getHtmlObject().addClass("tbTable");
            drawColumns(this._data);
            function drawTitle(col, tableData) {
                if (!col.IsHidden) {
                    var titleColHTML = tb$("<div></div>")
								.appendTo(_me.getHtmlObject())
								.css("font-size", "25")
								.addClass("tbColumnTitle")
                                .css({ height: col.ColumnTitleRect.height + 'px',
                                    width: col.ColumnTitleRect.width + 'px'
                                })
								.css("left", col.ColumnTitleRect.x - tableData.BaseRect.x)
								.css("top", col.ColumnTitleRect.y - tableData.BaseRect.y)
								.text(col.Title.Text)
								.css("font-style", col.Title.FontData.Family);
                }
            }

            function drawColumns(tableData) {
                for (var i = 0; i < tableData.Columns.length; i++) {
                    var col = tableData.Columns[i];
                    if (col.IsHidden)
                        break;
                    drawTitle(col, tableData);
                    for (var j = 0; j < col.Cells.length; j++) {
                        var cell = col.Cells[j];
                        var cellHTML = tb$("<div></div>")
								.appendTo(_me.getHtmlObject())
								.addClass("tbColumnCell")
                                .css({ height: cell.RectCell.height + 'px',
                                    width: cell.RectCell.width + 'px'
                                })
								.css("left", cell.RectCell.x - tableData.BaseRect.x)
								.css("top", cell.RectCell.y - tableData.BaseRect.y)
								.text(cell.Value.FormattedData)
								.css("font-style", cell.Value.FontData.Family)
								.css("font-size", cell.Value.FontData.Size);
                    }
                }
            }
        }
    });
}

