/// <reference path="Widget.js" />

/// <reference path="Persistence.js" />

/// <reference path="jquery.min.js" />

/// <reference path="jquery-ui.min.js" />

/// <reference path="PropertyGrid.js" />
/// <reference path="Report.js" />


tb$ = jQuery.noConflict();


/*
tb$(function () {
	getAskDialogData(createAskDialog);
});*/

function checkradioDataChanged(event) {
	var _askDialogManagerInstance = AskDialogManager.getInstance();
	_askDialogManagerInstance.setFieldData(this.id, this.checked ? 1 : 0);

	if (event.data.isReferenced) {
		updateAskDataOnServer(true);
	}
}

function textBoxDataChanged(event) {
	var _askDialogManagerInstance = AskDialogManager.getInstance();
	_askDialogManagerInstance.setFieldData(this.id, event.data.control.val());
	
	if (event.data.isReferenced){
		updateAskDataOnServer(true);
	}
}

function comboBoxDataChanged(event) {
	var _askDialogManagerInstance = AskDialogManager.getInstance();
	_askDialogManagerInstance.setFieldData(this.id, event.data.control.val());

	if (event.data.isReferenced) {
		updateAskDataOnServer(true);
	}
}

function submitAsk() {
	updateAskDataOnServer(false);
}

function okAsk() {
	var jqxhr = tb$.getJSON('WoormHandler.axd/OkAsk', window.sessionData, function (data, textStatus, jqXHR) {
		if (data.ask)
			getAskDialogData(createAskDialog);
		else
			getReportData(createReport, data);
	})
	.error(function () { alert("Error calling the server to get report json data!"); });
}

function updateAskDataOnServer(renderAsk) {
	/*inizializzazione   AskDialogManager*/
	var _askDialogManagerInstance = AskDialogManager.getInstance();


	var jqxhr = tb$.getJSON('WoormHandler.axd/UpdateAskDialogData', { __StateMachineSessionTag: window.sessionData.__StateMachineSessionTag, sessionID: window.sessionData.sessionID, fields: JSON.stringify(_askDialogManagerInstance.getFields()) }, function (data, textStatus, jqXHR) {
		if (data.error)
			alert('An error occurred on the server: ' + data.message);
		else {
			if (renderAsk) 
				createAskDialog(data);
			else
				okAsk();
		}
	})
	.error(function () { alert("Error calling the server to get askdialog json data!"); });
}



function createAskDialog(askDialogData) {
	/*inizializzazione   AskDialogManager*/
	var _askDialogManagerInstance = AskDialogManager.getInstance();

	_askDialogManagerInstance.clearFields();

	/*renderizzazione html askDialog */

	//svuoto il body per ricreare l'askdialog da zero nel caso di postBack dovuti a campi referenziati
	var askDialogDiv = tb$(".askDialogContainer");
	
	var reportContent = tb$("#Report .tbWidgetContent");
	reportContent.empty();
	askDialogDiv = tb$("<div> </div>").addClass("askDialogContainer").draggable({ containment: "parent" });
	reportContent.append(askDialogDiv);
	
	//aggiunta titolo
	tb$("<div>" + askDialogData.askDialog.LocalizedFormTitle + "</div>").addClass("askDialogTitle").appendTo(askDialogDiv);
	
	//aggiunta field
	for (var i = 0; i < askDialogData.askDialog.Groups.length; i++) {
		var group = askDialogData.askDialog.Groups[i];
		if (group.Hidden)
			continue;

		var groupDiv = tb$("<div></div>").addClass("askDialogGroup");
		askDialogDiv.append(groupDiv);
		
		for (var j = 0; j < group.Entries.length; j++) {
			var askEntry = group.Entries[j];
			var fieldName = askEntry.Field.name;
			var askEntryDiv = tb$("<div></div>").addClass("askDialogEntry")
			if (askEntry.Field.dataType === "Boolean") {
				var checked = askEntry.Field.data;

				if (askEntry.ControlStyle == _askDialogManagerInstance.ASKSTYLESCONST.get('CHECK_BOX_BOOL_STYLE')) {

					askEntryDiv.append(tb$("<label><input id=" + fieldName + " type=checkbox />" + askEntry.Caption + "</label>"));
					groupDiv.append(askEntryDiv);
					var checkBox = tb$('#' + fieldName);
					checkBox.attr('checked', checked);
					_askDialogManagerInstance.addField(fieldName, checked ? 1 : 0);
					checkBox.bind("click", { isReferenced: askEntry.isReferenced }, checkradioDataChanged);
				}
				else if (askEntry.ControlStyle == _askDialogManagerInstance.ASKSTYLESCONST.get('RADIO_BUTTON_BOOL_STYLE'))
				{
					var idRadioBtn = "radio" + i.toString() + j.toString();
					askEntryDiv.append(tb$("<label><input id=" + fieldName + " type=radio " + (checked ? "checked=true" : "") + " name= radioGroup" + i.toString() + " />" + askEntry.Caption + "</label>"));
					groupDiv.append(askEntryDiv);
					var radioButton = tb$('#' + fieldName);
					_askDialogManagerInstance.addField(fieldName, checked ? 1 : 0);
					radioButton.bind("change", { isReferenced: askEntry.isReferenced }, checkradioDataChanged);
				}
				else
				{
					//TODO SILVANO
						// allora si tratta di un EDIT control per il tipo boolean. Devo modificare 
						// la lunghezza perchè il comportamento di default chiede "True/False" e non 
						// "Si/No" e la lunghezza invece è 2
						//	askEntry.Len = 5;
						//	TextBox(askEntry, groupTable, groupRow, groupCell);
				}
			}
			else if (askEntry.Field.dataType === "DataEnum")
			{
				var combo = tb$("<select id=" + fieldName + "/>");
				var selectedItemText = "";
				tb$.each(askEntry.enumsList, function (index, value) {
					combo.append('<option value="' + value + '">' + value + '</option>');
					selectedItemText = value;
					if (index == askEntry.selectedEnumIndex)
						_askDialogManagerInstance.addField(fieldName, selectedItemText);
				}
				)
				combo[askEntry.selectedEnumIndex].selected = true;
				combo.bind("change", { isReferenced: askEntry.isReferenced, control: combo }, comboBoxDataChanged);
				var caption = askEntry.Caption == null ? "" : askEntry.Caption;
				askEntryDiv.append(tb$("<label>" + caption + "</label>").append(combo));
				groupDiv.append(askEntryDiv);
			}
			else
			{
				// tutti gli altri tipi li edito con un TextBox
				var textBox = tb$("<input type=text id=" + fieldName + " value= '"  + askEntry.Field.data.toString() + "' />");
				if (askEntry.enabled)
					textBox.removeAttr("disabled");
				else
					textBox.attr("disabled",true);

				textBox.addClass("askDialogTextBox");
				_askDialogManagerInstance.addField(fieldName, askEntry.Field.data);
				textBox.bind("change", { isReferenced: askEntry.isReferenced, control: textBox}, textBoxDataChanged);
				askEntryDiv.append(tb$("<label>" + askEntry.Caption + "</label>").append(textBox));
				groupDiv.append(askEntryDiv);
			}
					
		}
	}

	//aggiunta bottoni
	var btnContainer = tb$("<div></div>").addClass("askDialogMandatoryBtnContainer").appendTo(askDialogDiv);
	var okBtn = tb$("<input type='submit' value='OK'/>").appendTo(btnContainer).button();
	tb$("<input type='submit' value='Cancel'/>").appendTo(btnContainer).button();

	okBtn.click(submitAsk);
}

function getAskDialogData(createAskDialog) {
	var jqxhr = tb$.getJSON('WoormHandler.axd/GetAskDialogData', window.sessionData, function (data, textStatus, jqXHR) {
			if (data.error)
				alert('An error occurred on the server: ' + data.message);
			else
				createAskDialog(data);
	})
	.error(function () { alert("Error calling the server to get askdialog json data!"); });
}



function AskDialogManager() {
	//Singleton pattern
	if (arguments.callee.instance)
		return arguments.callee.instance;
	arguments.callee.instance = this;

	_fieldArray = [];

	//esempio di utilizzo <istanza AskDialogManager>.ASKSTYLESCONST.get('MY_CONST')); 
	this.ASKSTYLESCONST = (function () {
		var private = {
			'CHECK_BOX_BOOL_STYLE': 0x00000000,
			'RADIO_BUTTON_BOOL_STYLE': 0x00000001,
			'EDIT_BOOL_STYLE': 0x00000002,
			'BOOL_BTN_LEFT_ALIGNED': 0x00000004,
			'COMBO_STYLE': 0x00000008
		};

		return {
			get: function (name) { return private[name]; }
		};

	})();

	this.clearFields = function () {
		_fieldArray = [];
	};

	this.addField = function (name, data) {
		_fieldArray.push({ name: name,  data: data });
	};

	this.setFieldData = function (name, data) {
		for (var i = 0; i < _fieldArray.length; i++) {
			if (_fieldArray[i].name == name) {
				_fieldArray[i].data = data;
				break;
			}
		}
	};

	this.getFields = function () {
		return _fieldArray;
	};

}

AskDialogManager.getInstance = function () {
	var _askDialogManagerInstance = new AskDialogManager();
	return _askDialogManagerInstance;
}

