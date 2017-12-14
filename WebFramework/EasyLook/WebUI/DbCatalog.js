

function Catalog(catalog) {
	var tb$ = jQuery.noConflict();
	_me = this;
	_catalog = catalog;

	this.fillDatabaseWidget = function () {
		var databaseWidget;
		for (var i = 0; i < widgets.length; i++) {
			var w = widgets[i];
			if (w.getName() == "Database") {
				databaseWidget = w;
				break;
			}
		}
		databaseWidget.setContent(this);
	}

	this.updateTableColumns = function (tableName, columnsData) {
		var tbl = null;
		//search the table to update
		for (var i = 0; i < _catalog.tables.length; i++) {
			tbl = _catalog.tables[i];
			if (tbl.name == tableName)
				break;
		}
		tbl.columns = columnsData;
	}

	this.tableIsAlreadyFilled = function (tableName) {
		var tbl = null;
		//search the table
		for (var i = 0; i < _catalog.tables.length; i++) {
			tbl = _catalog.tables[i];
			if (tbl.name == tableName)
				break;
		}
		//se le columns sono gia valorizzate vuol dire che le ho gia scricate in precedenza dal server
		if (tbl.columns != undefined)
			return true;
		
		return false;
	}

	this.expandTable = function (tableName) {
		if (_me.tableIsAlreadyFilled(tableName))
			return;

		var jqxhr = tb$.getJSON('WoormHandler.axd/GetColumns', { __StateMachineSessionTag: window.sessionData.__StateMachineSessionTag, sessionID: window.sessionData.sessionID, table: tableName }, function (data, textStatus, jqXHR) {
			if (!data.ready)
				setTimeout(function () { getReportEngine() }, 1000);
			else {
				if (data.error)
					alert('An error occurred on the server');
				else {

					_me.updateTableColumns(tableName, data.columns);
					_me.show();
				}
			}
		})
	.error(function () { alert("Error calling the server to get report engine json data!"); });
	}


	this.create = function (parent) {

		var table = tb$('<table class="pgGridContainer"><tbody/></table>');


		for (var i = 0; i < _catalog.tables.length; i++) {
			var tbl = _catalog.tables[i];
			var columns = tbl.columns != undefined ? tbl.columns : null;
			createTable(tbl.name, columns);
		}

		function createColumns(columns) {
			var colsHtmlTable = tb$('<table class="pgGridContainer"><tbody/></table>')
			for (var i = 0; i < columns.length; i++) {
				var col = columns[i];
				createColumn(col);
			}

			function createColumn(column) {
				try {

					var row = tb$('<tr class="pgGroupItem"><td class="pgGroupItemName">' + column.name + '</td><td>' + column.type + '</td></tr>');
					tb$('tbody:first', colsHtmlTable).append(row);
				}
				catch (e) {

				}
			}
			return colsHtmlTable;
		}

		function createTable(tableName, columns) {
			try {
				var row = tb$('<tr class="pgGroupItem"><td class="pgGroupItemName">' + tableName + '</td><td class="pgGroupItemValue" /></tr>');
				row.click(function () { _me.expandTable(tableName); })
				tb$('tbody:first', table).append(row);

				if (columns != null) {
					var itemValue = tb$('.pgGroupItemValue', row);
					itemValue.append(createColumns(columns));
				}
			}
			catch (e) {

			}
		}
		table.appendTo(parent);
		return table;
	}

	this.show = function () {
		this.fillDatabaseWidget();
	}
}

