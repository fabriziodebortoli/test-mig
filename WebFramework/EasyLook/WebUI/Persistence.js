/// <reference path="Widget.js" />
/// <reference path="jquery.min.js" />
/// <reference path="jquery-ui.min.js" />

function PersistenceManager() {

	var enabled = "undefined" !== typeof (openDatabase);

	function nullDataHandler() {
		console.log("SQL Query Succeeded");
	}


	function errorHandler(transaction, error) {
		if (error.code == 1) {
			// DB Table already exists
		} else {
			// Error is a human-readable string.
			console.log('Oops.  Error was ' + error.message + ' (Code ' + error.code + ')');
		}
		return false;
	}


	this.getWidgets = function (callback) {
		var ret = [];
		if (!enabled) {
			callback(ret);
			return;
		}
		var sql = 'SELECT * FROM WIDGETS';
		db.transaction(
				function (transaction) {
					transaction.executeSql(sql, [], dataSelectHandler, errorHandler);
				});
		function dataSelectHandler(transaction, results) {
			for (var i = 0; i < results.rows.length; i++)
				ret.push(results.rows.item(i));
			callback(ret);

		}
		function errorHandler(transaction, error) {
			callback(ret);
			return false;
		}
	}
	this.persistWidget = function (widget) {
		if (!enabled)
			return;
		
		function insertWidget() {
			var sql = 'INSERT INTO WIDGETS (NAME, STATE) VALUES(?, ?)';
			db.transaction(
				function (transaction) {
					transaction.executeSql(sql, [widget.getName(), widget.getState()], nullDataHandler, errorHandler);
				});
		}
		function updateWidget() {
			var sql = 'UPDATE WIDGETS SET STATE = ? WHERE NAME = ?';
			db.transaction(
				function (transaction) {
					transaction.executeSql(sql, [widget.getState(), widget.getName()], nullDataHandler, errorHandler);
				});
		}

		function dataSelectHandler(transaction, results) {
			// Handle the results  
			if (results.rows.length > 0)
				updateWidget();
			else
				insertWidget();
		}

		var sql = 'SELECT * FROM WIDGETS WHERE NAME = ?';
		db.transaction(
				function (transaction) {
					transaction.executeSql(sql, [widget.getName()], dataSelectHandler, errorHandler);
				});

	}

	if (!enabled)
		return;

	var db = openDatabase('EasyLook', '1.0', 'Test DB', 2 * 1024 * 1024);
	db.transaction(
        function (transaction) {
        	transaction.executeSql(
                "CREATE TABLE IF NOT EXISTS WIDGETS (NAME TEXT NOT NULL PRIMARY KEY, STATE TEXT NOT NULL)", [], nullDataHandler, errorHandler
            );
        }

    );

}