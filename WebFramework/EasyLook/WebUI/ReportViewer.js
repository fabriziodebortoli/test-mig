/// <reference path="Widget.js" />

/// <reference path="Persistence.js" />

/// <reference path="jquery.min.js" />

/// <reference path="jquery-ui.min.js" />

/// <reference path="PropertyGrid.js" />
/// <reference path="Report.js" />


tb$ = jQuery.noConflict();

var grid = new PropertyGrid();
_lastObject = null;

function fillPropertyGrid(object, reload) {
	//ottimizzazione: property grid gia valorizzata per questo oggetto
	if (_lastObject == object && reload !== true)
		return;
	_lastObject = object

	var propertiesWidget;
	for (var i = 0; i < widgets.length; i++) {
		var w = widgets[i];
		if (w.getName() == "Properties") {
			propertiesWidget = w;
			break;
		}
	}
	grid.fillAndRender(object._data);
	propertiesWidget.setContent(grid);
}

var persistenceManager = new PersistenceManager();
var widgets = new Array();

tb$(function () {
	runReport();
	createWidgets();
	/*persistenceManager.getWidgets(function (results) {
	if (results.length == 0)
	createWidgets();
	else
	restoreWidgets(results);
		
	getReportData(createReport);
	});*/

})
function createReport(reportObjects, paperLength, paperWidth) {

	var reportWidget;
	for (var i = 0; i < widgets.length; i++) {
		var w = widgets[i];
		if (w.getName() == "Report") {
			reportWidget = w;
			break;
		}
	}
	//svuoto la pagina da eventuale ask dialog
	tb$(".askDialogContainer").remove();


	reportWidget.setContent(new Report(reportObjects, paperLength, paperWidth));

}

function runReport() {
	var jqxhr = tb$.getJSON('WoormHandler.axd/RunReport', window.sessionData, function (data, textStatus, jqXHR) {
		window.sessionData = data;
		if (data.ask)
			getAskDialogData(createAskDialog);
		else
			getReportData(createReport, data);
	})
	.error(function () { alert("Error calling the server to get report json data!"); });
}


function getReportData(createReport, sessionData) {
	var jqxhr = tb$.getJSON('WoormHandler.axd/GetReportData', sessionData, function (data, textStatus, jqXHR) {
		if (!data.ready)
			setTimeout(function () { getReportData(createReport, sessionData) }, 1000);
		else {
			if (data.error)
				alert('An error occurred on the server: ' + data.message);
			else
				createReport(data.reportObjects, data.paperLength, data.paperWidth);
		}
	})
	.error(function () { alert("Error calling the server to get report json data!"); }) ;
}

function restoreWidgets(results) {
	for (var i = 0; i < results.length; i++) {
		var wi = results[i];
		var state = wi.STATE;
		var w = new Widget(wi.NAME, "");
		w.create(document.body);
		w.setState(state);
		w.setPersistenceManager(persistenceManager);
		widgets.push(w);
	}
}
function createWidgets() {
	var w = new Widget("Report", "Report");
	w.create(document.body);
	//w.setWidth("200px");
	//w.setHeight("300px");
	w.setPersistenceManager(persistenceManager);
	widgets.push(w);
	w = new Widget("Properties", "Properties");
	w.create(document.body);
	w.setPersistenceManager(persistenceManager);
	widgets.push(w);

	/*w = new Widget("Model", "Model");
	w.create(document.body);
	w.setPersistenceManager(persistenceManager);
	widgets.push(w);
	*/
	w = new Widget("Database", "Database");
	w.create(document.body);
	w.setPersistenceManager(persistenceManager);
	widgets.push(w);
}