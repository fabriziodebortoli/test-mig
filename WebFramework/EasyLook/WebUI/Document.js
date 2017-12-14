
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


/*  
 * 	Document from json to web interface
 */

var WebDocument = null;
var processingRequest = false;

function sendPostRequestJSON(url, dataJsonData, callback) {
	//semaforizzo per evitare chiamate multiple prima che arrivi la risposta
		tb$.post(
			url,
			dataJsonData,
			function (data) {
				if (!data.ready)
					setTimeout(function () {
						sendPostRequestJSON(url, dataJsonData, function (dataRet) { callback(dataRet); })
					}, 1000);
				else {
					if (data.error)
						alert('An error occurred on the server: ' + data.message);
					else {
						callback(data);
					}
				}
			},
        "json"
		);
}

function getDocumentData() {
    // first request, first creating web page

    var postData = '{"sessionGuid":"' + window.sessionData.sessionGuid + '",';
    postData += '"objectNamespace":"' + window.sessionData.objectNamespace + '"}';
    
    sendPostRequestJSON('documentForm.aspx/TbLoader/GetDocumentData', postData,
        function (dataRet) {
            // read json structure
            WebDocument = new Thread();
            WebDocument.create(dataRet.documentObjects, null);
            WebDocument.goDocStart();
        });   
}

/*********************************************************************************************************************
 read json end structure to dictionary
 **********************************************************************************************************************/

function Dictionary() {
	var _dictionary = {};

	this.insert = function (key, value) {
		_dictionary[key] = value;
	}

	this.existsKey = function (key){
		return key in _dictionary;
	}

	this.deleteEntry = function (key){
		delete _dictionary[key]; 
	}

	this.clear = function (key) {
	    _dictionary = {};
	}

	this.getValues = function () {
		return _dictionary;
    }

}



function Thread() {    
    var _threadId = 0;
    var _documents = {};
    var _openDialogList = {};

	//lista delle coppie {id,value} dei controlli modificati dall'utente
    var _dirtyList = new Dictionary();
    var tb$ = jQuery.noConflict();

    // find obj in all document open
    this.findObjById = function (id) {
        var obj = null;
        tb$.each(Object.keys(_documents), function (i, IdDoc) {
            obj = _documents[IdDoc].findObj(id);
            if (obj != null)
                return false;
        });

        tb$.each(Object.keys(_openDialogList), function (i, IdDialog) {
        	obj = _openDialogList[IdDialog].findObj(id);
           	if (obj != null)
           		return false;
        });

        return obj;
    }

    // **********************************************************************
    this.addToDirtyList = function (id, value) {
        _dirtyList.insert(id, value);
	}

	this.getDirtyListValues = function () {
		return _dirtyList.getValues();
	}

	this.clearDirtyList = function () {
		return _dirtyList.clear();
    }

    this.removeDirtyListValues = function (id) {
        if (_dirtyList.existsKey(id))
            _dirtyList.deleteEntry(id);
    }

    this.getDirtyListJson = function () {
        var jTxt = '[';
        var sep = false;
        var dicList = _dirtyList.getValues();
        tb$.each(Object.keys(dicList), function (i, key) {
            if (sep) jTxt += ',';
            jTxt += '{';
            jTxt += '"objectId": "' + key + '", ';
            jTxt += '"txt": "' + dicList[key] + '"';
            jTxt += '}';
            sep = true;
        });
        jTxt += ']';

        return jTxt;
    }

	
    // update web page
    this.updateViewPage = function (newDocObj) {

    	var parentList = {};
    	var t = this;
    	// update, remove, add object list
    	tb$.each(newDocObj.children, function (i, obj) {
    		switch (obj.descState) {
    			case descState_REMOVED:
    				switch (obj.type) {
    					case WndObjType_Dialog:
						case WndObjType_PropertyDialog:
    						t.destroyDialog(obj.id);
    						break;

    					case WndObjType_Frame:
    						t.destroyDocument(obj.id);
    						break;

    					default:
    						var findObj = t.findObjById(obj.id);
    						if (findObj != null) {
    							findObj.destroy();
    						}
    						break;
    				}

    				break;

    			case descState_UPDATED:
    				var findObj = t.findObjById(obj.id);
    				if (findObj != null) {
    					findObj.update(obj);
    				}
    				break;

    			case descState_ADDED:
    				switch (obj.type) {
    					case WndObjType_Frame:
    						t.create(obj, _threadId);
    						t.goDocument(obj.id);
    						break;

    					case WndObjType_Dialog:
						case WndObjType_PropertyDialog:
    						var rd = t.createDialog(obj);
    						rd.show();
    						_openDialogList[obj.id] = rd;
    						break;

    					default:
    						if (parentList[obj.parentId] == null) {
    							var findObj = t.findObjById(obj.parentId)
    							if (findObj != null)
    								parentList[obj.parentId] = findObj;
    						}

    						if (parentList[obj.parentId] != null) {
    							apendObj(obj, parentList[obj.parentId]);
    						}
    				}

    				break;
    		}

    	});


    	tb$.each(Object.keys(parentList), function (i, idParent) {
    		parentList[idParent].update();
    		parentList[idParent].setEvent();
    	});
    }

    // show first document page
    this.goDocStart = function () {
        this.goDocument(Object.keys(_documents)[0]);
    }

    // go to document
    this.goDocument = function (id) {
        var rf = _documents[id];
        if (rf == null)
            debugger;

        // web page render
        rf.go();
        // enable event in web form
        rf.enableEvent();
    }

    // render Tabber
    function rCompInTabber(objIn, rf) {
        var TabberObj = new tabberObj(objIn);
        if (objIn.children != null) {
            tb$.each(objIn.children, function (i, obj) {
                tbObj = new tabObj(obj);
                if (obj.children != null) {
                    renderComponent(obj.children, tbObj);
                }
                TabberObj.append(tbObj);
            });
        }
        rf.append(TabberObj);
       }

    // render Group
	function rCompInGroup(objIn, rf) {
		var GroupObj = new groupObj(objIn);
		if (objIn.children != null) {
			renderComponent(objIn.children, GroupObj);
       	}
       	rf.append(GroupObj);
    }

    // render report
    function rReport(objIn, rf) {
        // renderComponent(obj.children, rf)
        var rObj = new reportObj(objIn);
        if (objIn.children != null) {
            tb$.each(objIn.children, function (i, obj) {
                switch (obj.type) {
                    // Field Report               
                    case WndObjType_FieldReport:
                        rObj.append(new reportFielObj(obj));
                        break;

                    case WndObjType_TableReport:
                        rObj.append(new reportTableObj(obj));
                        break;

                    default:
                        console.log(obj);
                }

            });
        }
        rf.append(rObj);
    }

    // Body Edit
    function rBodyEdit(objIn, rf) {
        var rObj = new bodyEditObj(objIn);
        if (objIn.children != null) {
            tb$.each(objIn.children, function (i, obj) {
                apendObj(obj, rObj);
            });
        }
        rf.append(rObj);
    }

    // Table
    function rTable(objIn, rf) {
        var rObj = new tableObj(objIn);
        if (objIn.children != null) {
            tb$.each(objIn.children, function (i, obj) {
                apendObj(obj, rObj);
            });
        }
        rf.append(rObj);
    }

    // append single obj
    function apendObj(obj, rf) {
        if (obj.id == null)
            debugger;

        switch (obj.type) {
            case WndObjType_Undefined:
                debugger;
            case WndObjType_View:
                debugger;
                break;
            case WndObjType_Label:
                // Append label
                rf.append(new labelObj(obj));
                break;
            case WndObjType_Button:
                rf.append(new buttonObj(obj));
                break;
            case WndObjType_PdfButton:
                break;
            case WndObjType_BeButton:
                rf.append(new beButtonObj(obj));
                break;
            case WndObjType_BeButtonRight:
                rf.append(new beButtonRightObj(obj));
                break;
            case WndObjType_SaveFileButton:
                break;
            case WndObjType_Image:
                break;
            case WndObjType_Group:
            	rCompInGroup(obj, rf);
				break;
            case WndObjType_Radio:
                rf.append(new radioObj(obj));
                break;
            case WndObjType_Check:
                rf.append(new checkBoxObj(obj));
                break;
            case WndObjType_Combo:
                // Append combo
                rf.append(new comboBoxObj(obj));
                break;

            case WndObjType_MailAddressEdit:
                rf.append(new textBoxObj(obj));
                break;
            case WndObjType_WebLinkEdit:
                rf.append(new textBoxObj(obj));
                break;
            case WndObjType_AddressEdit:
                rf.append(new textBoxObj(obj));
                break;
            case WndObjType_FieldReport:
                break
            case WndObjType_TableReport:
                break;
            case WndObjType_Edit:
                // Append text box
                rf.append(new textBoxObj(obj));
                break;
            case WndObjType_BodyEdit:
                // Append Body edit
                rBodyEdit(obj, rf);
                break;
            case WndObjType_Table:
                rTable(obj, rf);
                break;
            // Tabber                                      
            case WndObjType_Tabber:
                rCompInTabber(obj, rf);
                break;
            // Report     
            case WndObjType_Report:
                rReport(obj, rf);
                break;

            case WndObjType_ColTitle:
                rf.append(new colTitleObj(obj));
                break;

            case WndObjType_Cell:
                rf.append(new cellObj(obj));
                break;

            case WndObjType_HotLink:
                break;

            default:
                console.log('Non definito: ' + obj.type);
                break;
        }
        
    }

    // render component label, textbox, toolTip ecc...
    function renderComponent(childrenObj, rf) {
        tb$.each(childrenObj, function (i, obj) {
            apendObj(obj, rf)
        });
    }

    // create dialog
    this.createDialog = function (obj) {
    	var rd = new renderDialog(obj);
    	tb$.each(obj.children, function (i, obj) {
    		apendObj(obj, rd)
    	});
    	return rd;
    }

    // destroy dialog
    this.destroyDialog = function (id) {
    	if (_openDialogList[id] != null) {
    		_openDialogList[id].hide();
    		delete _openDialogList[id];
    	}
    	else
    		debugger;
    }

	// destroy document
    this.destroyDocument = function (id) {
    	if (_documents[obj.id] != null) {
    		_documents[obj.id].destroy();
    		tb$("#doc_" + obj.id).remove();
    		delete _documents[obj.id];
    	}
    	else
    		debugger;
    }

    // create the new object in document
    this.create = function (documentObjects, threadId) {

    	// get button bar button
    	function iterateChildrenToolbarButton(obj, rf) {
    		var htmlButton = "";
    		var tb = new toolbarObj(obj);
    		tb$.each(obj.children, function (i, obj) {

    			switch (obj.type) {
    				case WndObjType_ToolbarButton:
    					tb.toolbarButtonAppend(obj);
    					break;

    				case WndObjType_Combo:
    					// tb.toolbarComboAppend(obj.id, obj.enabled, obj.tooltip);
    					break;

    				default:
    					console.log(obj);

    			}

    		});

    		// Add tool bar
    		rf.append(tb);
    	}

    	// get children frame object
    	function iterateChildrenFrame(childrenObj, rf) {
    		tb$.each(childrenObj, function (i, obj) {
    			if (obj.id != null) {
    				switch (obj.type) {
    					// Undefined                                                                 
    					case WndObjType_Undefined:
    						debugger;
    						break;
    					// Title                                             
    					case WndObjType_Title:
    						rf.setTitle(obj);
    						break;
    					// MainMenu                                               
    					case WndObjType_MainMenu:
    						break;
    					// add tool bar                                                             
    					case WndObjType_Toolbar:
    						// iterate children buton bar
    						if (obj.children != null)
    							iterateChildrenToolbarButton(obj, rf);
    						break;
    					// View                                             
    					case WndObjType_View:
    						if (obj.children != null) {
    							rf.append(new viewObj(obj));
    							renderComponent(obj.children, rf);
    						}
    						else {
    							debugger;
    						}
    						break;
    					// Status bar                                            
    					case WndObjType_StatusBar:
    						break;

    					case WndObjType_Label:
    						break;

    					default:
    						console.log(obj);
    						break;

    				}
    			}
    		});
    	}

    	// get frame
    	function iterateFrame(obj) {
    		var rf;
    		if (obj.id != null) {
    			rf = new renderFrame(obj);

    			// accelerators append
    			tb$.each(obj.accelerators, function (i, accObj) {
    				rf.appendAccelerator(accObj.cmd, accObj.key);
    			});

    			switch (obj.type) {
    				// Undefined                                                                
    				case WndObjType_Undefined:
    					debugger;
    					break;
    				// add new frame                                                                  
    				case WndObjType_Frame:
    					if (obj.children != null)
    						iterateChildrenFrame(obj.children, rf);
    					break;
    			}

    			// apend new frame (documenti page)
    			if (_documents[obj.id] == null) {
    				var name = rf.getTitle(); 
					//  "doc: " + obj.id;
    				var htmlButton = "<a href='#'";
    				htmlButton += " onclick=WebDocument.goDocument('" + obj.id + "');";
    				htmlButton += ">" + name + "</a>";
    				tb$('div.document').append('<div class="documentTitle" id="doc_' + obj.id + '">' + htmlButton + '</div>');
    				_documents[obj.id] = rf;
    			}
    			else {
    				_documents[obj.id] = rf;
    			}
    		}
    	}

    	/*
    	* start iterate main mamber
    	*/
    	function startThread(JSonObj) {
    		// Thread of windows in render
    		tb$.each(JSonObj, function (i, obj) {
    			var thread = obj.thread;
    			_threadId = thread.id;
    			// all freame
    			tb$.each(thread.children, function (i, obj) {
    				iterateFrame(obj);
    			});
    		});
    	}

    	if (threadId != null)
    		iterateFrame(documentObjects);
    	else
    		startThread(documentObjects.children);

    }

}
