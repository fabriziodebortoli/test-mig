var localizationService = function ($http, $log, $rootScope) {

    this.localizedElements = undefined;
    var thiz = this;

    //---------------------------------------------------------------------------------------------
    this.loadLocalizedElements = function (needLoginThread) {
        //se ho gia' gli elementi in canna, li mantengo solo se corrispondono allo stesso valore di needLoginThread;
        //se sono relativi al thread di login, devono prevalere rispetto a quelli del thread di applicazione
        if (thiz.localizedElements != undefined && (thiz.localizedElements.needLoginThread === needLoginThread || thiz.localizedElements.needLoginThread)) {
            return thiz.localizedElements;
        }
        
        $http.post('getLocalizedElements/?needLoginThread=' + needLoginThread)
		.success(function (data, status, headers, config) {
		    thiz.localizedElements = data.LocalizedElements;
		    thiz.localizedElements.needLoginThread = needLoginThread;
		    thiz.loaded = true;
		})
		.error(function (data, status, headers, config) {
		    error('error getLocalizedElements' + status);
		});
    }

    //---------------------------------------------------------------------------------------------
    this.getLocalizedElement = function (key) {

        if (this.localizedElements == undefined)
            return undefined;

        var allElements = this.localizedElements.LocalizedElement;
        for (var i = 0; i < allElements.length; i++) {
            if (allElements[i].key == key)
                return allElements[i].value;
        };

        return key;
    }

};
