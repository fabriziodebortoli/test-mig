
/// <reference path="const.js" />

function DocumentEvents(renderEngine) {
	var currentCellChanged = false,
	engine = renderEngine,
	focusedElement = null,
	row,
	col,
	editor = null,
	notifySuspended = false,
	me = this;
	this.onTabActivate = function (sender, eOpts) {
		if (notifySuspended)
			return;
		sendRequest("activateTab/", { "controlId": sender.id, "session": window.session }, function (data) {
			if (!data.success) {
				safeAlert(data.error);
			}
		});
	}

	this.onClick = function (sender, eOpts) {
		safeAlert("onClick handler");
		if (notifySuspended)
			return;
		var focusedControlInfo = getPreviousFocusedInfo(sender);
		sendRequest("buttonClick/", { "controlId": sender.id, "session": window.session, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val }, function (data) {
			safeAlert("onClick callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return false;
	}

	this.onHKLUpClick = function (sender, eOpts) {
		safeAlert("onHKLUpClick handler");
		var focusedControlInfo = getPreviousFocusedInfo(sender);
		sendRequest("hklUpClick/", { "controlId": sender.ownerId, "session": window.session, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val }, function (data) {
			safeAlert("onClick callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return false;
	}

	this.onHKLLowClick = function (sender, eOpts) {
		safeAlert("onHKLLowClick handler");
		var focusedControlInfo = getPreviousFocusedInfo(sender);
		sendRequest("hklLowClick/", { "controlId": sender.ownerId, "session": window.session, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val }, function (data) {
			safeAlert("onClick callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return false;
	}
	this.onHyperLink = function (controlId) {
		sendRequest("doHyperLink/", { "controlId": controlId, "session": window.session }, function (data) {
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return true;
	}
	this.onMenuItemClick = function (sender, eOpts) {
		safeAlert("onMenuItemClick handler");

		return false;
	}



	this.onMenuClick = function (sender, item, e, eOpts) {
		safeAlert("onMenuClick handler");
		var sCommand = item.command;
		var sMenuID = sender.owner;
		safeAlert("onMenuItemClick handler");
		var focusedControlInfo = getPreviousFocusedInfo(sender);
		sendRequest("menuItemClick/", { "controlId": sMenuID, "session": window.session, "command": sCommand }, function (data) {
			safeAlert("onMenuClick callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return false;
	}

	this.onMenuClose = function (panel, eOpts) {
		safeAlert("onMenuClose handler");
	}

	this.onCollapse = function (p, eOpts) {
		safeAlert("onCollapse handler");
	}

	this.onMenuHide = function (menu, eOpts) {
		safeAlert("onMenuHide handler");
		sendRequest("menuHide/", { "controlId": menu.owner, "session": window.session }, function (data) {
			safeAlert("onMenuHide callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
		return false;
	}

	this.onRadioChange = function (sender, newValue, oldValue, eOpts) {
		// this check prevents not currently selected radio
		// buttons to fire the event. These events are not 
		// needed and can cause wrong behaviours.
		if (newValue == true) {
			safeAlert("onRadioChange handler");
			me.onClick(sender, eOpts);
		}
		return false;

	}
	this.onEditForm = function () {
		if (!me.editor) {
			me.editor = new FormEditor({ renderEngine: engine });
		}

		//// TODO: add a "onFocus" event listener to the created object.
		//// may be useful to change main panel properties.
		//if (oMainPanel) {
		//    oMainPanel.on('focus', editor.onComponentFocus);
		//    oMainPanel.focus();
		//    oMainPanel.addCls('x-focus');
		//}

	}
	this.suspendNotify = function () { notifySuspended = true; }
	this.resumeNotify = function () { notifySuspended = false; }

	//focus may change because of a client event (usually an user action) or because of a server event
	//in the case of a server event, the server has not to be notified of the change (it already knows it!)
	this.onFocus = function (sender) {
		safeAlert("onFocus handler");
		if (focusedElement == sender && !currentCellChanged) return;
		currentCellChanged = false;
		var focusedControlInfo = updateFocusedElement(sender);
		// sender is the currently focused control.
		if (!me.notifyFocus(focusedControlInfo.id, sender.id))
			return true;
		if (focusedControlInfo.id != sender.id) {
			// notify the server only if current ctrl has actually changed
			var params = { "session": window.session, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val, "toControl": sender.id, "row": row, "col": col };

			sendRequest("moveTo/", params, function (data) {
				safeAlert("onFocus callback");
				if (!data.success) {
					safeAlert(data.error);
				}
			});
		}
		return true;
	}


	this.onBlur = function (sender) {
		// notify the server only if current ctrl has actually changed
		var params = { "session": window.session, "value": sender.value, "control": sender.id };

		sendRequest("blur/", params, function (data) {
			if (!data.success) {
				safeAlert(data.error);

				return true;
			}
		});
	}

	this.onSelectedItem = function (sender, records) {
		safeAlert("onSelectedItem handler");
		var focusedControlInfo = getPreviousFocusedInfo(sender);
		var indexes = [];
		// CB store
		var oStore = sender.getStore();
		for (var iCount = 0; iCount < records.length; iCount++) {
			indexes.push(records[iCount].index);
		}
		var params = { "session": window.session, "controlId": sender.id, "selectedIndexes": indexes, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val };
		// inviare richiesta + parametri: TODO
		sendRequest("selectedItem/", params, function (data) {
			safeAlert("onSelectedItem callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
	}

	this.onRadarDblClickItem = function (sender, record, item, index, e, eOpts) {
		safeAlert("onRadarDblClickItem handler");
		var sID = sender.panel ? sender.panel.id : sender.id;
		var params = { "session": window.session, "controlId": sID, "index": index };
		sendRequest("radarDblClick/", params, function (data) {
			safeAlert("radarDblClick callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
	}

	this.afterRadarRender = function (sender, eOpts) {
		// set selected item
		// sender.getSelectionModel().select(sender.jsonItem.activeRow, false);
		safeAlert("afterRadarRender callback");

		if (sender.view.getEl()) {

			sender.lastScrollTop = sender.view.getEl().getScroll().top;
			sender.view.getEl().on('scroll', function (e, t, eOpts) {
				var sID = sender.panel ? sender.panel.id : sender.id;
				var params = { "session": window.session, "controlId": sID };

				//if (t.scrollTop + t.clientHeight >= t.scrollHeight) {
				if (t.scrollTop > sender.lastScrollTop) {
					params.direction = -1;
					console.log('down');
				}
					//else if (t.scrollTop == 0) {
				else if (t.scrollTop < sender.lastScrollTop) {
					params.direction = +1;
					console.log('up');
				}
				sender.lastScrollTop = t.scrollTop;
				if (params.direction) {
					sendRequest("radarScroll/", params, function (data) {
						safeAlert("radarScroll callback");
						if (!data.success) {
							safeAlert(data.error);
						}
					});
				}
			});
		}
	}

	this.radarSelect = function (sender, record, index, eOpts) {

		var sID = sender.store.parentGrid.id;
		var params = { "session": window.session, "controlId": sID, "selected": index };

		sendRequest("radarSelect/", params, function (data) {
			safeAlert("radarSelect callback");
			if (!data.success) {
				safeAlert(data.error);
			}
		});
	}

	this.arrowButtonClick = function (sender, menu, eOpts) {

		var params = { "session": window.session, "controlId": sender.id };

		sendRequest("buttonArrowClick/", params, function (data) {
			safeAlert("arrowButtonClick callback");
			//if (!data.success) {
			//    safeAlert(data.error);
			//}
		});
	}

	////////////////////////////////////////////////////////////////////////
	/// Returns true if the change focus has to be notified to the server.
	/// oPrevFocused: The control previously had the focus on.
	/// oCurrFocused: The control now has the focus on.
	////////////////////////////////////////////////////////////////////////
	this.notifyFocus = function (oPrevFocused, oCurrFocused) {
		return !notifySuspended;
	}


	this.onBeforeEditCell = function (editor, e, eOpts) {
		col = e.colIdx;
		row = e.rowIdx;
		currentCellChanged = true;
		var bIsReadOnly = e.grid.jsonItem.readOnly == undefined ? true : e.grid.jsonItem.readOnly;
		var bEditable = !bIsReadOnly && e.record.data[e.field + readOnlySuffix] != 1;
		if (bEditable) {
			e.grid.editingRow = row;
		} else {
			e.grid.editingRow = -1;
		}
		return bEditable;
	}

	this.onCloseDialog = function (sender, eOpts) {
		safeAlert("dialog close event.");

		var params = { "session": window.session, "controlId": sender.id };

		sendRequest("closeDialog/", params, function (data) {
			safeAlert("closeDialog callback");
		});
	}

	this.onGridLoad = function (me, records, successful, eOpts) {

		//no data extracted for a page greater than 1: go to first page!
		if (successful && records.length == 0 && me.currentPage > 1) {
			me.loadPage(1);
		}
		var oGrid = me.parentGrid;
		if (oGrid) {
			// update grid selected item
			if (oGrid.jsonItem.selected && oGrid.jsonItem.selected.length > 0) {
				// TODO: handle multi selection.
				var iSelected = oGrid.jsonItem.selected[0].index;
				if (iSelected >= oGrid.store.pageSize) {
					iSelected %= oGrid.store.pageSize;
				}
				oGrid.getSelectionModel().select(iSelected);
			}
		}
	}

	this.onGridSelectionChanged = function (sender, selected, eOpts) {
		if (notifySuspended)
			return;

		if (!sender.store.parentGrid.readOnly) {
			// if the grid is in editing no selection info has to be notified to the server.
			return;
		}
		safeAlert("grid selection event.");

		var oGrid = sender.getStore().parentGrid;
		var oStore = oGrid.getStore();
		var sSelected = "";
		for (var iCount = 0; iCount < selected.length; iCount++) {
			// TODO: take into account the paging.
			sSelected += (oStore.pageSize * (oStore.currentPage - 1) + oStore.indexOf(selected[iCount])) + "_";
		}
		sSelected = sSelected.slice(0, sSelected.length);
		var iIndex = oStore.indexOf(selected[0]);

		var params = {
			"session": window.session, "controlId": oGrid.id, "selectedId": sSelected
		};

		sendRequest("gridSelectionChanged/", params, function (data) {
			safeAlert("GridSelectionChanged callback");
			//if (!data.success) {
			//    safeAlert(data.error);
			//}
		});
	}

	/////////////////////////////////
	/// Handler for a CheckList item checked state changed.
	/////////////////////////////////
	this.onListItemCheckChanged = function (sender, rowIndex, isChecked, eOpts) {
		safeAlert("checklist checked state changed for row " + rowIndex + ", checked: " + isChecked);
		var params = { "session": window.session, "controlId": sender.parentId, "rowIndex": rowIndex, "checked": isChecked };

		sendRequest("checkListChanged/", params, function (data) {
			safeAlert("checkListChanged callback");
			//if (!data.success) {
			//    safeAlert(data.error);
			//}
		});
	}

	////////////////////////////////////////////////////////////////////////
	/// Updates the current focused control. Returns information about the previously focused one.
	/// control: The control now has the focus on.
	////////////////////////////////////////////////////////////////////////
	function updateFocusedElement(control) {
		var obj = {};
		if (focusedElement) {
			obj.id = focusedElement.id;
			var sValue = "";
			sValue = getElementValue(focusedElement);
			obj.val = normalizeText(sValue);
		}

		if ((obj.val == null) || (obj.val == undefined))
			obj.val = '';
		focusedElement = control;
		return obj;
	}

	function normalizeText(text) {
		if (!Ext.isDefined(text) || text == null)
			return undefined;

		if (text.constructor === String) {
			return text.replace(/\r/gm, "").replace(/\n/gm, "\r\n");
		}
		// default case
		return text;
	}

	function getElementValue(oFocusedElement) {
		var oResult = null;
		if (oFocusedElement) {
			if (focusedElement.value) {
				// extract the value of the focused element. It will be
				// transferred to the server, so some kind/format conversion 
				// may be done in there.
				if (focusedElement.value.constructor == Date) {
					// ExtJs fieldDate field case.
					oResult = focusedElement.getRawValue();
				} else if ((typeof focusedElement.value) == "boolean") {
					// convert boolean value to an integer as expected by the backend.
					oResult = focusedElement.value ? 1 : 0;
				} else {
					// default case
					oResult = focusedElement.value;
				}
			}
			if (Ext.isDefined(focusedElement.checked)) {
				oResult = focusedElement.checked;
			}
		}
		return oResult;
	}

	function getPreviousFocusedInfo(sender) {
		return (focusedElement == sender)
			? { "id": 0, "val": '' }
			: updateFocusedElement(sender);

	}
}

