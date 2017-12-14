
/*  
* 	render web page page class
*/

var tb$ = jQuery.noConflict();
var mouseDown = false;

tb$.fn.outerHTML = function () {
	tb$t = tb$(this);
	if ("outerHTML" in tb$t[0])
	{ return tb$t[0].outerHTML; }
	else {
		var content = tb$t.wrap('<div></div>').parent().html();
		tb$t.unwrap();
		return content;
	}
}

// center the element in Conteiner DIV
tb$.fn.center = function () {

	var contH   = tb$(window).height();
	var contW   = tb$(window).width();
	var boxH    = this.height();
	var boxW    = this.width();
	var contX = (contW/2) - (boxW/2);
	var contY = (contH/2) - (boxH/2);;
	
	// set position
	this.css('left', contX + 'px');
	this.css('top',  contY + 'px');

	return this;
}

// multi line split txt
function multilineSplit(txtIn) {
	var r_html = txtIn.replace("\\n", "</Br>");
	r_html = r_html.replace("\\", "");
	return r_html;
}

// send change tab command
function sendChangeTab(idTab, idTabber) {
	var postData = '{"sessionGuid":"' + window.sessionData.sessionGuid + '", ';
	postData += '"objectId":"' + idTab + '", "tabberID":"' + idTabber + '"}';
    
    var dataRet = sendPostRequestJSON('documentForm.aspx/TbLoader/ChangeTab', postData,
        function (dataRet) {
        	WebDocument.updateViewPage(dataRet.documentObjects);
    });

    var rf = WebDocument.findObjById(idTabber);
    rf.hideTabAcive();
    rf = WebDocument.findObjById(idTab);
    rf.show();
}

// Send comand to server and get result
function sendJsonCommand(command, id) {
    var postData = '{"sessionGuid":"' + window.sessionData.sessionGuid + '",';
    postData += '"objectId":"' + id + '",'
    postData += '"values": ' + WebDocument.getDirtyListJson() + '}';
    WebDocument.clearDirtyList();

    var dataRet = sendPostRequestJSON('documentForm.aspx/TbLoader/' + command, postData,
        function (dataRet) {
            WebDocument.updateViewPage(dataRet.documentObjects);
     });
}

// send command click 
function sendCommandClick(id) {
    sendJsonCommand("ButtonClick", id);
}

// obj lost focus 
function lostFocusEvent(obj) {
    if (obj.isDirty()) 
        WebDocument.addToDirtyList(obj._id ,obj.getValue());
    else
        WebDocument.removeDirtyListValues(obj._id);
}

// obj get focus 
function getFocusEvent(obj) {
    sendJsonCommand("SetFocus", obj._id);
}

function hideModalBox() {
    // tb$("div.messagebox").hide(1000);
    tb$("div.messageboxModal").hide(1500);
    tb$(window).die("resize");
}

function showModalBox() {
    tb$("div.messageboxModal").height(tb$(document).height());
    tb$("div.messageboxModal").width(tb$(document).width());  
    tb$("div.messageboxModal").show("slow");

    tb$(window).resize(function() {
        tb$("div.messageboxModal").height(tb$(document).height());
        tb$("div.messageboxModal").width(tb$(document).width());  
    });

}

// **************************************
// Dialog
function renderDialog(obj) {
	var _objList  = [];
	var _idDialog = obj.id;
	var _modal = obj.isModal;

	this.show = function () {
		var position = null;
		var p = null;
		var X = 0;
		var Y = 0;
		
		var html = this.getHtml();
		
		if (_modal)
		{
			tb$('div.messageboxUpModal').html(html);
			tb$('div.messageboxUpModal').show('slow', function() {
					// Animation complete.
				tb$("#" + _idDialog).show('slow', function() {
					// center messageBoxWin
					tb$('div.messageBoxWin').center();
				});
			});
			showModalBox();
		}
		else
		{
			tb$('div.messagebox').append(html);
			tb$("#" + _idDialog).show("slow");
			// event
			tb$("#" + _idDialog).mousemove(function(e) {
				if (mouseDown)
				{
					p.css('left', (position.left +  e.pageX - X) + 'px' );
					p.css('top',  (position.top  +  e.pageY - Y) + 'px' );
				}
			});

			tb$("#Top_" + _idDialog).mousedown(function(e) {
					mouseDown = !mouseDown;
					p = tb$("#" + _idDialog);
					position = p.position();
					X = e.pageX;
					Y = e.pageY;
			});

			tb$("#" + _idDialog).mouseup(function() {
				mouseDown = false;
			});

		}
    }

	this.hide = function () {
		tb$("#" + _idDialog).hide(1000);
		if (_modal)
		{
			tb$('div.messageboxUpModal').hide(1000);
			hideModalBox();	
		}
		tb$("#" + _idDialog).remove();
	}

	this.getHtml = function () {
		var html = "";
		html += '<div class="messageBoxWin" id="' + _idDialog + '">';
		if (!_modal)
		{
			html += '<div class="messageBoxWinTop" id="Top_' + _idDialog + '"></div>';
		}
        tb$.each(_objList, function (i, obj) {
            html += obj.render();
        });
        html += '</div>';
        return html;
	}

	// find object in document list
    this.findObj = function (findId) {
        var objRet = null;
        // search in obj list
        tb$.each(_objList, function (i, obj) {
            objRet = obj.findObj(findId);
            if (objRet != null)
                return false;
        });
        return objRet;
    }

	this.append = function (obj) {
        _objList.push(obj);
    }
}

// **************************************
// Frame
function renderFrame(obj) {
    var _objList = [];
    var _objToolBar = [];
    var _accelerators = {};
    var _idFrame = obj.id;
	var _frameTitle = "";
    
    this.go = function () {
        var html = "";
        html += '<div class="ToolBarForm"></div>';
        html += '<div class="ElementForm"></div>';
		tb$('div.content').html(html);
        tb$('div.ElementForm').html(this.getHtml());
    }

	// destroy document
	this.destroy = function () {
	
	}

	// set document title
	this.setTitle = function (obj) {

		var i = obj.text.search("-");
		var txt = obj.text;
		if (i > -1) 
			txt = txt.slice(0,i-1)

		_frameTitle = txt;
	}

	// get document title
	this.getTitle = function () {
		return _frameTitle;
	}

    // find object in document list
    this.findObj = function (findId) {
        var objRet = null;
        // search in obj list
        tb$.each(_objList, function (i, obj) {
            objRet = obj.findObj(findId);
            if (objRet != null)
                return false;
        });
        return objRet;
    }

    this.getHtml = function () {
        var html = "";
        html += '<div id="' + _idFrame + '">';
        tb$.each(_objList, function (i, obj) {
            html += obj.render();
        });
        html += '</div>';
        return html;
    }

    this.appendAccelerator = function (cmd, key) {
        _accelerators[key] = cmd;
    }

    this.getAccelerators = function () {
        return _accelerators;
    }

    this.enableAccelerators = function () {
        tb$.each(Object.keys(_accelerators), function (i, code) {
            /*
            tb$(document).bind('keypress.' + code, function (e) {
                var c = (e.keyCode ? e.keyCode : e.which);
                if (code == c) {
                    e.stopPropagation();
                    e.preventDefault();

                    //Todo: action Accelerators
                    if (_accelerators[c] != null)
                        console.log(_accelerators[c]);
                }
            });
            */
        });
    }

    this.enableEvent = function () {
        tb$.each(_objList, function (i, obj) {
            obj.setEvent();
        });
        this.enableAccelerators();

		tb$("div.container").mouseup(function() {
				mouseDown = false;
		});

		tb$("div.container").mousemove(function(e) {
			mouseDown = false;
		});

    }

    this.append = function (obj) {
        _objList.push(obj);
    }
}


// *************************************************************************************************************
// * Component frame object
// *************************************************************************************************************

var BaseFrameObj = Class.extend({
    name: "BaseFrameObj",

    init: function (obj) {
        this._obj = obj;
    },

    render: function () {
        console.log("Render:", this);
        return "";
    },

    setEvent: function () {
        // console.log("Event set:", this);
        // not set event
    },

    // obj is changed
    isDirty: function () {
        var str = this.getValue();
        // obj is changed
        if (str != this.getObjValue())
            return true;
        return false;
    },
    
    // get value from web page 
    getValue :function () {
        return tb$.trim(tb$("#" + this._id).val());
    },

    // get obj value
    getObjValue :function () {
        return this._txt;
    },

    findObj: function (findId) {
        if (this._id == null)
            debugger;

        if (this._id == findId) return this;
        return null;
    },

    update: function (obj) {
        console.log("Update:", this);
    },

    disable: function () {
        console.log("disable:", this);
    },

	destroy: function () {
        console.log("destroy:", this);
    }

});

// **************************************
// Toolbar

var toolbarButtonObj = BaseFrameObj.extend({
    name: "toolbarButtonObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.tooltip;
        this._en = obj.enabled;
        this._super(obj);
    },

    update: function (obj) {
        this._en = obj.enabled;
        this._txt = obj.tooltip;
        var html = this.buttonHtml();
        tb$("#DIV_" + this._id).html(html);
    },

    render: function () {
        var html = "";
        if (tb$.trim(this._txt) == "") return " ";
        html += '<div class="toolbarButton" id="DIV_' + this._id + '">';
        html += this.buttonHtml();
        html += '</div>';
        return html;
    },

    buttonHtml: function() {
        var html = "";
        if (this._en == 1) {
            html += '<a href="#" ';
            html += "' onclick=sendCommandClick('" + this._id + "'); ";
            html += " >" + this._txt;
            html += "</a>"
        }
        else
            html += this._txt;

        return html;
    }

});

var toolbarComboObj = BaseFrameObj.extend({
    name: "toolbarComboObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.tooltip;
        this._en = obj.enabled;
        this._super(obj);
    },

    update: function (obj) {
        this._en = obj.enabled;
        this._txt = obj.tooltip;

        var html = this.render();
        tb$("#DIV_" + this._id).html(html);
    },

    render: function () {
        var html = "";
        html += '<div id="DIV_' + this._id + '">';
        html += '<select name="" id="' + this._id + '"';
        if (this._en == 0) html += ' disabled="disabled" style="background-color:' + color_disable + '"';
        html += '> </select>';
        html += '</div>';
        return html;
    }

});

var toolbarObj = BaseFrameObj.extend({
    name: "toolbarObj",

    init: function (obj) {
        this._id = obj.id;
        this._buttonList = [];
        this._super(obj);
    },

    findObj: function (id) {
        var objRet = null;
        var t = this;
        if (this._id == id) return this;
        tb$.each(this._buttonList, function (i, obj) {
            if (obj._id == id) {
                objRet = obj;
                return false;
            }
        });

        return objRet;
    },

    render: function () {
        var html = "";
        html += '<div id="' + this._id + '">';
        tb$.each(this._buttonList, function (i, obj) {
            html += obj.render();
        });
        html += '</div><br>';

        tb$('div.ToolBarForm').append(html);
        return "";
    },

    toolbarButtonAppend: function (obj) {
        this._buttonList.push(new toolbarButtonObj(obj));
    },

    toolbarComboAppend: function (obj) {
        this._buttonList.push(new toolbarComboObj(obj));
    }

});

// **************************************
// View
var viewObj = BaseFrameObj.extend({
    name: "viewObj",

    init: function (obj) {
        this._id = obj.id;
        this._super(obj);
    },

    render: function () {
        return "";
    }
});


// **************************************
// Body Edit
var bodyEditObj = BaseFrameObj.extend({
    name: "bodyEditObj",

    init: function (obj) {
        this._id = obj.id;
        this._objList = [];
        this._super(obj);
    },

    render: function () {
        var r_html = "";
        r_html += '<div id="' + this._id + '">';
        tb$.each(this._objList, function (i, obj) {
            r_html += obj.render();
        });
        r_html += '</div>';
        return r_html;
    },

    findObj: function (id) {
        var objRet = null;
        tb$.each(this._objList, function (i, obj) {
            objRet = obj.findObj(id);
            if (objRet != null) return false;
        });
        return objRet;
    },

    append: function (obj) {
        this._objList.push(obj);
    },

	setEvent: function () {
        tb$.each(this._objList, function (i, obj) {
            obj.setEvent();
        });
    },
});

// table
var tableObj = BaseFrameObj.extend({
	name: "tableObj",

	init: function (obj) {
		this._id = obj.id;
		this._txt = obj.text;
		this._en = obj.enabled;
		this._objList = [];
		this._super(obj);
	},

	findObj: function (id) {
		var objRet = null;
		tb$.each(this._objList, function (i, obj) {
			objRet = obj.findObj(id);
			if (objRet != null) return false;
		});
		return objRet;
	},

	render: function () {
		var r_html = "";
		var colN = 0;
		var n = 0;
		var notCol = false;

		r_html += '<div class="tableObj" id="' + this._id + '">';

		r_html += '<table id="table-design">';
		r_html += '<thead>'
		tb$.each(this._objList, function (i, obj) {
			if (obj.isCol()) {
				colN++;
				r_html += '<th>';
				r_html += obj.render();
				r_html += '</th>';
			}
		});
		r_html += '</thead>';
		r_html += '<tbody>';

		tb$.each(this._objList, function (i, obj) {
			if (n == 0) r_html += '<tr>';
			n++;
			if (!obj.isCol()) {
				r_html += '<td>';
				r_html += obj.render();
				r_html += '</td>';
			}
			if (n >= colN) {
				r_html += '</tr>';
				n = 0;
			}

		});

		r_html += '</tbody>';
		r_html += '</table>';

		r_html += '</div>';
		return r_html;
	},

	append: function (obj) {
		this._objList.push(obj);
	},

	setEvent: function () {
		tb$.each(this._objList, function (i, obj) {
			obj.setEvent();	
		});
	}

});

var colTitleObj = BaseFrameObj.extend({
    name: "colTitleObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._super(obj);
    },

    isCol: function () {
        return true;
    },

    render: function () {
    	return '<label id="' + this._id + '">' + multilineSplit(this._txt) + '</label>';
    }

});

var cellObj = BaseFrameObj.extend({
    name: "CellObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._super(obj);
    },

    isCol: function () {
        return false;
    },

    render: function () {
        return '<label id="' + this._id + '">' + this._txt + '</label>';
    },

	setEvent: function () {
		var t = this;
		
		tb$("#" + this._id).dblclick(function() {
		  t.cellEdit();
		});
	},

	cellEdit: function()
	{
		tb$("#" + this._id).html('<input type="text" value="' + this._txt + '" id="input_' + this._id +  '">');
		
		var t = this;
		tb$("#input_" + this._id).focus();
		tb$("#input_" + this._id).focusout(function () {
			t._txt = tb$("#input_" + t._id).val();
			tb$("#" + t._id).html(t._txt);
		});

	}

});

// BeButtonRight
var beButtonRightObj = BaseFrameObj.extend({
    name: "beButtonRightObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._super(obj);
    },

    update: function (obj) {
        this._txt = obj.text;
        this._en = obj.enabled;

        if (this._en == 0)
            tb$("#" + this._id).attr("disabled", "disabled");
        else
            tb$("#" + this._id).removeAttr("disabled", "disabled");

        tb$("#" + this._id).val(obj.text);
    },

    render: function () {
        var r_html = '<input type="button" id="' + this._id + '" value="' + this._txt + '"';
        r_html += "' onclick=sendCommandClick('" + this._id + "'); ";
        if (this._en == 0) r_html += ' disabled="disabled" ';
        r_html += '>';
        return r_html;
    }

});

// BeButton
var beButtonObj = BaseFrameObj.extend({
    name: "beButtonObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._super(obj);
    },

    update: function (obj) {
        this._txt = obj.text;
        this._en = obj.enabled;

        if (this._en == 0)
            tb$("#" + this._id).attr("disabled", "disabled");
        else
            tb$("#" + this._id).removeAttr("disabled", "disabled");

        tb$("#" + this._id).val(obj.text);
    },

    render: function () {
        var r_html = '<input type="button" id="' + this._id + '" value="' + this._txt + '"';
        r_html += "' onclick=sendCommandClick('" + this._id + "'); ";
        if (this._en == 0) r_html += ' disabled="disabled" ';
        r_html += '>';
        return r_html;
    }

});

// **************************************
// Tabber
var tabberObj = BaseFrameObj.extend({
    name: "tabberObj",
    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._objList = [];
        this._super(obj);
    },

    findObj: function (id) {
        var objRet = null;
        if (this._id == id) return this;
        tb$.each(this._objList, function (i, obj) {
            objRet = obj.findObj(id);
            if (objRet != null) return false;
        });
        return objRet;
    },

    hideTabAcive: function () {
        tb$.each(this._objList, function (i, obj) {
            if (obj._isShow == true) {
                obj.hide();
            }
        });
    },

    render: function () {
        var html = "";
        var id = this._id;

        html += '<div class="tabber" id="' + id + '">';
        var first = true
        tb$.each(this._objList, function (i, obj) {
            html += "<Br>" + obj.render(first, id);
            first = false;
        });
        html += '</div>';
        return html;
    },

    setEvent: function () {
        tb$.each(this._objList, function (i, obj) {
            obj.setEvent();
        });
    },

    append: function (obj) {
        this._objList.push(obj);
    }
});

// **************************************
// Tab
var tabObj = BaseFrameObj.extend({
    name: "tabObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._objList = [];
        this._isShow = false;
        this._super(obj);
    },

    findObj: function (id) {
        var objRet = null;
        if (this._id == id) return this;
        tb$.each(this._objList, function (i, obj) {
            objRet = obj.findObj(id);
            if (objRet != null) return false;
        });
        return objRet;
    },

    update: function (obj) {
        var html = '';
        tb$.each(this._objList, function (i, obj) {
            html += "<Br>" + obj.render();
        });
        tb$("#DIV_" + this._id).html(html);
    },

    hide: function () {
        tb$("#DIV_" + this._id).hide(1000);
        this._isShow = false;
    },

    show: function () {
        tb$("#DIV_" + this._id).show("slow");
        this._isShow = true;
    },

    render: function (first, idParent) {
        var html = '';
        this._isShow = first;
        html += '<a href="#" ';
        html += "' onclick=sendChangeTab('" + this._id + "','" + idParent + "'); >";
        html += this._txt.replace('&','') + '</a>';
        html += '<div class="tab" id="DIV_' + this._id + '" ';
        if (first == false)
            html += 'style="display:none;" ';
        html += '>';
        tb$.each(this._objList, function (i, obj) {
            html += "<Br>" + obj.render();
        });
        html += '</div>';
        return html;
    },

    setEvent: function () {
        tb$.each(this._objList, function (i, obj) {
            obj.setEvent();
        });
    },

    append: function (obj) {
        this._objList.push(obj);
    }
});

// **************************************
// Label
var labelObj = BaseFrameObj.extend({
    name: "labelObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
    },

    render: function () {
    	return '<div class="labelObj"> <label id="' + this._id + '">' + this._txt + '</label> </div>';
    }

});

// **************************************
// Text box input
var textBoxObj = BaseFrameObj.extend({
	name: "textBoxObj",
	init: function (obj) {
		this._id = obj.id;
		this._txt = obj.text;
		this._en = obj.enabled && !(obj.isStatic);
		this._label = obj.label;
		this._super(obj);
	},

	update: function (obj) {
		this._en = obj.enabled && !(obj.isStatic);
		this._txt = obj.text;
		this._label = obj.label;

		if (this._en == 0) {
			tb$("#" + this._id).attr("style", "background-color:" + color_disable);
			tb$("#" + this._id).attr("disabled", "disabled");
		}
		else {
			tb$("#" + this._id).removeAttr("disabled", "disabled");
			tb$("#" + this._id).attr("style", "background-color:" + color_enable);
		}

		tb$("#" + this._id).val(obj.text);
	},

	render: function () {
		var r_html = '<div class="textBoxObj">';
		r_html += '<label>' + this._label + '&nbsp;</label> <input type="text" value="' + this._txt + '" name="" id="' + this._id + '"';
		if (this._en == 0) r_html += ' disabled="disabled" style="background-color' + color_disable + '"';
		r_html += '/>';
		r_html += '</div>';
		return r_html;
	},

	setEvent: function () {
		var tObj = this;

		tb$("#" + this._id).focusin(function () {
			getFocusEvent(tObj);
		});


		tb$("#" + this._id).focusout(function () {
			lostFocusEvent(tObj);
		});
	}
});

// **************************************
// Combo box
var comboBoxObj = BaseFrameObj.extend({
	name: "comboBoxObj",

	init: function (obj) {
		this._id = obj.id;
		this._txt = obj.text;
		this._en = obj.enabled;
		this._label = obj.label;
		this._items = obj.items;
		this._super(obj);
	},

	update: function (obj) {
		this._txt = obj.text;
		this._en = obj.enabled;
		this._label = obj.label;
		this._items = obj.items;
		if (this._en == 0) {
			tb$("#" + this._id).attr("disabled", "disabled");
			tb$("#" + this._id).attr("style", "background-color:" + color_disable);
		}
		else {
			tb$("#" + this._id).removeAttr("disabled", "disabled");
			tb$("#" + this._id).attr("style", "background-color:" + color_enable);
		}

		tb$("#" + this._id).html = this.renderList();
		tb$("#" + this._id).val(this._txt);
	},

	renderList: function () {
		var r_html = "";
		var tSelect = this._txt;
		if (tSelect == "") r_html += '<option value="" selected></option>';
		tb$.each(this._items, function (i, txt) {
			if (tSelect == txt)
				r_html += '<option value="' + txt + '" selected>' + txt + '</option>';
			else
				r_html += '<option value="' + txt + '">' + txt + '</option>';
		});
		return r_html;
	},

	render: function () {
		var r_html = '<div class="comboBoxObj">';
		r_html += '<label>' + this._label + '&nbsp;</label> <select name="" id="' + this._id + '"';
		if (this._en == 0) r_html += ' disabled="disabled" style="background-color:' + color_disable + '"';
		r_html += '>';
		r_html += this.renderList();
		r_html += '</select>';
		r_html += '</div>';
		return r_html;
	},

	setEvent: function () {
		var tObj = this;
		tb$("#" + this._id).focusout(function () {
			lostFocusEvent(tObj);
		});
	}

});


// **************************************
// Check box
var checkBoxObj = BaseFrameObj.extend({
    name: "checkBoxObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._ck = obj.checked;
    },

    update: function (obj) {
        this._txt = obj.text;
        this._en = obj.enabled;

        if (this._en == 0) {
            tb$("#" + this._id).attr("disabled", "disabled");
        }
        else {
            tb$("#" + this._id).removeAttr("disabled", "disabled");
        }
        tb$("#" + this._id).val(obj.text);
    },

       render: function () {
       	var r_html = '<div class="checkBoxObj">';
       	r_html += '<label>' + this._txt + '&nbsp;</label><input type="checkbox" name="" id="' + this._id + '"';
        if (this._ck == 1) r_html += ' checked="checked" ';
        if (this._en == 0) r_html += ' disabled="disabled" ';
        r_html += '/>';
        r_html += '</div>';
        return r_html;
    },

    setEvent: function () {
        var tObj = this;
        tb$("#" + this._id).focusout(function () {
            lostFocusEvent(tObj);
        });
    },

    // get value from web page 
    getValue :function () {
        if (tb$("#" + this._id).is(':checked'))
            return "1";
        return "0";
    },

    // get obj value
    getObjValue :function () {
        return this._ck;
    }

});

// **************************************
// Button
var buttonObj = BaseFrameObj.extend({
	name: "buttonObj",

	init: function (obj) {
		this._id = obj.id;
		this._txt = obj.text;
		this._en = obj.enabled;
		this._super(obj);
	},

	update: function (obj) {
		this._txt = obj.text;
		this._en = obj.enabled;

		if (this._en == 0)
			tb$("#" + this._id).attr("disabled", "disabled");
		else
			tb$("#" + this._id).removeAttr("disabled", "disabled");

		tb$("#" + this._id).val(obj.text);
	},

	render: function () {
		var r_html = '<div class="buttonObj">';
		r_html += '<input type="button" id="' + this._id + '" value="' + this._txt + '"';
		r_html += "' onclick=sendCommandClick('" + this._id + "'); ";
		if (this._en == 0) r_html += ' disabled="disabled" ';
		r_html += '>';
		r_html += '</div>';
		return r_html;
	}

});

// **************************************
// Group
groupObj = BaseFrameObj.extend({
    name: "groupObj",

    init: function (obj) {
        this._id = obj.id;
		this._txt = obj.text;
        this._en = obj.enabled;
        this._objList = [];
        this._super(obj);
    },

	findObj: function (id) {
        var objRet = null;
        if (this._id == id) return this;
        tb$.each(this._objList, function (i, obj) {
            objRet = obj.findObj(id);
            if (objRet != null) return false;
        });
        return objRet;
    },

    render: function () {
        var html = "";
        var id = this._id;
        html += '<div class="group" id="' + id + '">';
		html += '<H1>' + this._txt + '</H1>';
        tb$.each(this._objList, function (i, obj) {
            html += obj.render();
			html += '<br>';
        });
        html += '</div>';
        return html;
    },

	setEvent: function () {
        tb$.each(this._objList, function (i, obj) {
            obj.setEvent();
        });
    },

    append: function (obj) {
        this._objList.push(obj);
    }

});

// **************************************
// Radio
radioObj = BaseFrameObj.extend({
    name: "radioObj",

	init: function (obj) {
		this._id = obj.id;
		this._txt = obj.text;
		this._en = obj.enabled;
		this._groupName = obj.groupName;
		this._checked = obj.checked;
		this._super(obj);
	},

	update: function (obj) {
		this._txt = obj.text;
		this._en = obj.enabled;
		this._groupName = obj.groupName;
		this._checked = obj.checked;

		if (this._en == 0)
			tb$("#" + this._id).attr("disabled", "disabled");
		else
			tb$("#" + this._id).removeAttr("disabled", "disabled");

		tb$("#" + this._id).val(obj.text);
	},

	render: function () {
		var r_html = '<div class="radioObj">';
		r_html += '<input type="radio" id="' + this._id + '" name="' + this._groupName + '" value="' + this._txt + '"'
		if (this._en == 0)
			r_html += ' disabled="disabled" ';
		if (this._checked)
			r_html += ' checked = "true" ';
		r_html += '/>' + this._txt;
		r_html += '</div>'
		return r_html;
	},

	setEvent: function () {
		var tObj = this;
		tb$("#" + this._id).focusout(function () {
			lostFocusEvent(tObj);
		});
	}

});

//Report objects (TODO: move in separate file)
// **************************************
// Report
var reportObj = BaseFrameObj.extend({
    name: "reportObj",

    init: function (obj) {
        this._id = obj.id;
        this._txt = obj.text;
        this._en = obj.enabled;
        this._objList = [];
        this._super(obj);
    },

    findObj: function (id) {
        var objRet = null;
        tb$.each(this._objList, function (i, obj) {
            if (obj._id == id) {
                objRet = obj;
                return false;
            }
        });
        return objRet;
    },

    render: function () {
        var html = "";
        html += '<div class="report" id="' + this._id + '">'; 

        tb$.each(this._objList, function (i, obj) {
            html += obj.render();
        });
        html += '</div>';
        return html;
    },

    append: function (obj) {
        this._objList.push(obj);
    }

});

// **************************************
// Report field
var reportFielObj = BaseFrameObj.extend({
    name: "reportFieldObj",

    init: function (obj) {
        this._obj = obj;
        this._super(obj);
    },

    update: function (obj) {
        console.log("report Field");
    },

    render: function () {
        var htmlObj = tb$('<div>' + this._obj.text + '</div>');
        htmlObj.width(this._obj.rect.width).height(this._obj.rect.height);
        htmlObj.css("left", this._obj.rect.x); // 
        htmlObj.css("top", this._obj.rect.y); // 
        htmlObj.css("border", "solid 1px black");
        htmlObj.css("background-color", "lightgray");
        htmlObj.css("position", "absolute");
        return htmlObj.outerHTML();
    }
});

// Report table
var reportTableObj = BaseFrameObj.extend({
    name: "reportTableObj",

    init: function (obj) {
        this._obj = obj;
        this._super(obj);
    },

    update: function (obj) {
        console.log("report Table");
    },

    render: function () {
        var htmlObj = tb$('<div>' + this._obj.text + '</div>');
        htmlObj.width(this._obj.rect.width).height(this._obj.rect.height);
        htmlObj.css("left", this._obj.rect.x); // 
        htmlObj.css("top", this._obj.rect.y); // 
        htmlObj.css("border", "solid 1px black");
        htmlObj.css("background-color", "lightgray");
        htmlObj.css("position", "absolute");
        return htmlObj.outerHTML();
    }

});

