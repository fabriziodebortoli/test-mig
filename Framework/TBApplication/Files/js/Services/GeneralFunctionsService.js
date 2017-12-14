var generalFunctionsService = function ($http, $log, $rootScope, $uibModal, localizationService, loggingService) {

	//---------------------------------------------------------------------------------------------
	this.ToArray = function (items) {
		var filtered = [];

		if (items == undefined)
			return filtered;

		if (Object.prototype.toString.call(items) === '[object Array]')
			return items;
		else {
			filtered.push(items);
			return filtered;
		}
	};

	//---------------------------------------------------------------------------------------------
	this.parseBool = function (str) {

		if (typeof str === 'boolean')
			return str;

		if (typeof str === 'string' && str.toLowerCase() == 'true')
			return true;

		return (parseInt(str) > 0);
	}

	//---------------------------------------------------------------------------------------------
	this.getCookieByName = function (cookie) {

		var start = document.cookie.indexOf(cookie + "=");
		if (start < 0) {
			return;
		}
		start = document.cookie.indexOf("=", start) + 1;
		var end = document.cookie.indexOf(";", start);
		if (end == -1) {
			end = document.cookie.length;
		}
		var res = document.cookie.substring(start, end);
		return res;
	}

	//---------------------------------------------------------------------------------------------
	this.getApplicationFromQueryString = function () {
		var application = '';
		var pageUrl = window.location.search;
		var index = pageUrl.indexOf("?app=");
		if (index < 0)
			return application;
		application = pageUrl.substring(index + "?app=".length);
		index = application.indexOf("&");
		if (index < 0)
			return application;

		return application.substring(0, index);
	}

	//---------------------------------------------------------------------------------------------
	this.getGroupFromQueryString = function () {
		var group = '';
		var pageUrl = window.location.search;
		var index = pageUrl.indexOf("?group=");
		if (index < 0)
			return group;
		group = pageUrl.substring(index + "?group=".length);

		index = group.indexOf("&");
		if (index < 0)
			return group;

		return group.substring(0, index);
	}


	//---------------------------------------------------------------------------------------------
	this.getCurrentDate = function () {
		var d = new Date();
		var p = parseInt(
            d.getFullYear() +
            ("00" + (d.getMonth() + 1)).slice(-2) +
            ("00" + d.getDate()).slice(-2) +
            ("00" + d.getHours()).slice(-2) +
            ("00" + d.getMinutes()).slice(-2) +
            ("00" + d.getSeconds()).slice(-2));

		return p;
	}

	//---------------------------------------------------------------------------------------------
	this.hasWhiteSpace = function (s) {
		return /\s/g.test(s);
	};
	

	this.post = function (url) {
		// the $http API is based on the deferred/promise APIs exposed by the $q service
		// so it returns a promise for us by default
		var promise = $http.post(url);
		promise.then(function (response) {
			return response.data;
		}, function (response) {
			if (response.status === 401)
				loggingService.showDiagnostic(localizationService.getLocalizedElement("LoginExpired"), { onOk: function () { window.location.href = "loginhost.html"; } });
		});
		return promise;
	}
};