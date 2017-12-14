function TBWebSocket(clientId, baseURL)
{
	var me = this;
	var connection;
	var events = {};
	var clientId = clientId;
	var path = "getWebSocketsPort/";

	if (typeof baseURL !== "undefined" && baseURL != "")
	    path = baseURL + "/" + path;
	else {
	 
	    //possono arrivarmi chiamate per l'handler /tb/menu/", /tb/document/" o /tb/ ma le dirotto tutte allo stesso di /tb/

	    var idx = window.location.href.indexOf("/tb/menu/");
        if (idx >= 0)
            path = window.location.href.substring(0, idx + "/tb/menu/".length) + path;
	    else
        {
            idx = window.location.href.indexOf("/tb/document/");
            if (idx >= 0)
               path = window.location.href.substring(0, idx + "/tb/document/".length) + path;
        }
	}

	$.get(path, null, function (data) {
	    var url = "ws://" + window.location.hostname + ":" + data + "/TBWebSocketsController";
		connection = new WebSocket(url);
		connection.onmessage = function (e) {
			if (typeof (e.data) === "string") {
				var idx = e.data.indexOf('-');
				var eventName = e.data.substr(0, idx);
				me.fire(eventName, e.data.substr(idx + 1));
			}
		}

		connection.onerror = function (e) {
			me.fire("error", e);
		}

		connection.onopen = function (e) {
			connection.send("SetWebSocketName:" + clientId);
			me.fire("open", e);
		};
	});

	this.close = function ()
	{
		if (connection)
			connection.close();
	}
	this.on = function (evtName, fn)
	{
		var evt = events[evtName];
		if (!evt) {
			evt = [];
			events[evtName] = evt;
		}
		evt.push(fn);
	}
	this.fire = function (evtName, data)
	{
		var evt = events[evtName];
		if (evt) {
			for (var i = 0; i < evt.length; i++) {
				evt[i](data);
			}
		}
	}
}
