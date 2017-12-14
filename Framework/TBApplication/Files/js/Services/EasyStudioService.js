var easyStudioService = function ($http, $log, $sce, $uibModal, $rootScope, $q, localizationService, settingsService, generalFunctionsService, loggingService) {

    var thiz = this;

	this.localizationService = localizationService;
	this.settingsService = settingsService;
	this.generalFunctionsService = generalFunctionsService;
	this.loggingService = loggingService;

	var deferredGetCustomizations = undefined;

	this.currentApplication = undefined;
	this.currentModule = undefined;
	this.defaultApplication = undefined;
	this.defaultModule = undefined;
	this.tbBaseRoute = "/tb/document";

    //---------------------------------------------------------------------------------------------
	this.initEasyStudioData = function (object) {

	    object.isEasyStudioDocument = undefined;
	    this.getEasyStudioCustomizations(object);
    }

	//---------------------------------------------------------------------------------------------
	this.getEasyStudioCustomizations = function (object) {

	    var promise = this.getEasyStudioCustomizationsInternal(object);
		promise.then(function (data) {
			deferredGetCustomizations = undefined;

			//inizializza a null l'array delle customizzazioni di object, o lo svuota se è già pieno
			if (object.Customizations == undefined || object.Customizations == '') {
				object.Customizations = [];
			}
			else
				object.Customizations.splice(0, object.Customizations.length);

			object.isDesignable = data.Customizations != undefined;
			if (data != undefined && data.Customizations != undefined && data.Customizations.length > 0) {

				for (var i = 0; i < data.Customizations.length; i++) {
					object.Customizations.push(data.Customizations[i]);
				}
			}
		});
	}


	//---------------------------------------------------------------------------------------------
	this.getEasyStudioCustomizationsInternal = function (object) {
		if (deferredGetCustomizations != undefined)
			return deferredGetCustomizations.promise;

		deferredGetCustomizations = $q.defer();

		var ns = object.target;
		var objType = object.objectType.toLowerCase();

		//gestiamo solo batch e document, e per il c+ sono sempre document.app.mod...
		if (object.objectType.toLowerCase() != 'document' && object.objectType.toLowerCase() != 'batch')
			return;

		ns = 'document' + "." + ns;
		var urlToRun = this.tbBaseRoute + '/getCustomizationsForDocument/?ns=' + encodeURIComponent(ns);
		generalFunctionsService.post(urlToRun)
	    .success(function (data, status, headers, config) {
	    	deferredGetCustomizations.resolve(data);
	    })
	    .error(function (data, status, headers, config) {
	    	loggingService.handleError(urlToRun, status);
	    	deferredGetCustomizations.reject('error GetCustomizations' + status);
	    });

		return deferredGetCustomizations.promise;
	}

	//---------------------------------------------------------------------------------------------
	this.readDefaultContext = function (alsoSet) {

		var urlToRun = this.tbBaseRoute + '/getDefaultContext/';
		generalFunctionsService.post(urlToRun)
			       .success(function (data) {
			       	var keys = data.split(';');
			       	if (keys.length != 2)
			       		return false;
			       	if (keys[0] == "" || keys[1] == "")
			       		return false;

			       	thiz.defaultApplication = keys[0];
			       	thiz.defaultModule = keys[1];
			       	if (alsoSet)
			       		thiz.SetContext(thiz.defaultApplication, thiz.defaultModule, false);
			       	else {
			       		thiz.currentApplication = undefined;
			       		thiz.currentModule = undefined;
					   }

			       })
	}

	//---------------------------------------------------------------------------------------------
	this.getCustomizationContextAppAndModule = function () {

		var urlToRun = 'getCustomizationContextAppAndModule/';
		generalFunctionsService.post(urlToRun)
	    .success(function (data, status, headers, config) {
	    	thiz.extractApplicationAndModuleFromResponse(data);
	    })
	    .error(function (data, status, headers, config) {
	    	loggingService.handleError(urlToRun, status);
	    });
	}

	//---------------------------------------------------------------------------------------------
	this.extractApplicationAndModuleFromResponse = function (response) {
		if (response != undefined) {
			var keys = response.split(';');
			if (keys.length != 2)
				return false;
			if (keys[0] == "" || keys[1] == "")
				return false;

			this.currentApplication = keys[0];
			this.currentModule = keys[1];
			return true;
		}
		else {
			this.currentApplication = undefined;
			this.currentModule = undefined;
		}
		return false;
	}

	//---------------------------------------------------------------------------------------------
	this.changeCustomizationContext = function (callback) {

		var urlToRun = 'changeCustomizationContext/';
		generalFunctionsService.post(urlToRun)
	    .success(function (data, status, headers, config) {
	    	if (data.success) {
	    		if (callback != undefined)
	    			callback(thiz.extractApplicationAndModuleFromResponse(data));
	    	}
	    	else if (data.message) {
	    		loggingService.showDiagnostic(data.message);
	    	}
	    })
	    .error(function (data, status, headers, config) {
	    	loggingService.handleError(urlToRun, status);
	    });
	}

	//---------------------------------------------------------------------------------------------
	this.getAllAppsAndModules = function (callback) {
		var urlToRun = this.tbBaseRoute + '/getAllAppsAndModules/';
		generalFunctionsService.post(urlToRun)
       .success(function (data) {
       	if (callback != undefined)
       		return callback(thiz.extractApps(data));
       })
	}

	this.applications = [];
	this.memory = [];
	this.developEd = false;
	//---------------------------------------------------------------------------------------------
	this.extractApps = function (response) {
		if (response === undefined)
			return false;

		this.applications = [];
		this.modules = [];
		this.memory = [];
		var applicArray = response["allApplications"];
		for (var a = 0; a < applicArray.length; a++) {
			var app = applicArray[a]["application"];
			var mod = applicArray[a]["module"]
			if (this.applications.indexOf(app) === -1)
				this.applications.push(app);
			this.memory.push([app, mod]);
		}
		this.developEd = response["DeveloperEd"];
		return true;
	}

	//---------------------------------------------------------------------------------------------
	this.SetContext = function (application, module, isThisPairDefault) {
		this.currentApplication = application;
		this.currentModule = module;
		var urlToRun = this.tbBaseRoute + '/setAppAndModule/?app=' + application + '&mod=' + module + '&def=' + isThisPairDefault;
		generalFunctionsService.post(urlToRun)
			.success(function () {
			})
        .error(function () {
        });
	}

	//---------------------------------------------------------------------------------------------
	this.CreateContext = function (application, module, type) {
		this.currentApplication = application;
		this.currentModule = module;

		var urlToRun = this.tbBaseRoute + '/createNewContext/?app=' + application + '&mod=' + module + '&type=' + type;
		$http.post(urlToRun)
			 .success(function (data, status, headers, config) {
			 	if (data.success) {
			 		alert("SUCCESS");       	   
			 	}
			 }).error(function (data, status, headers, config) {
			 	alert("NOT SUCCESS");
			 	loggingService.handleError(urlToRun, status);
			 });
		/*generalFunctionsService.post(urlToRun)
			.success(function () {
			})
        .error(function () {
        });*/
	}

	//---------------------------------------------------------------------------------------------
	this.canShowEasyStudioButton = function (object) {
		return settingsService.isEasyStudioActivated && (object.objectType.toLowerCase() == 'document' || object.objectType.toLowerCase() == 'batch') && !object.noeasystudio;
	}

	//---------------------------------------------------------------------------------------------
	this.closeCustomizationContext = function () {

		var urlToRun = this.tbBaseRoute + '/closeCustomizationContext/';
		generalFunctionsService.post(urlToRun)
       .success(function (data, status, headers, config) {
       	if (data.success) {
       		thiz.currentApplication = undefined;
       		thiz.currentModule = undefined;
       	}
       	else if (data.message) {
       		loggingService.showDiagnostic(data.message);
       	}
       })
	}

	//---------------------------------------------------------------------------------------------
	this.getApplicationFromCustomizationFileName = function (customization) {
		return customization.applicationOwner;
	}

	//---------------------------------------------------------------------------------------------
	this.getModuleFromCustomizationFileName = function (customization) {
		return customization.moduleOwner;
	}

	//---------------------------------------------------------------------------------------------
	this.getCustomizationTooltip = function (customization) {
		if (this.isCustomizationEnabled(customization))
			return "";

		customization.toolTip = $sce.trustAsHtml(localizationService.getLocalizedElement("CustomizationNotActive") + "<br/>" + localizationService.getLocalizedElement("ApplicationLabel") + ": " + customization.applicationOwner + "<br/>" + localizationService.getLocalizedElement("ModuleLabel") + ": " + customization.moduleOwner + "<br/>");
	}

	//---------------------------------------------------------------------------------------------
	this.isCustomizationEnabled = function (customization) {

	    if (this.currentApplication == undefined || this.currentModule == undefined)
			return false;

		var appAndModulePartialPath = "\\" + this.currentApplication + "\\" + this.currentModule + "\\";
		var ind = customization.fileName.toLowerCase().indexOf(appAndModulePartialPath.toLowerCase());

		return ind > 0;
	}

    //---------------------------------------------------------------------------------------------
	this.isEasyStudioDocument = function (object) {
	    alert("a");
	    if (object.isEasyStudioDocument != undefined)
	        return object.isEasyStudioDocument;

	    var urlToRun = this.tbBaseRoute + '/isEasyStudioDocument/?ns=' + encodeURIComponent(object.target);
	
	    generalFunctionsService.post(urlToRun)

        .success(function (data, status, headers, config) {
            if (data && data.message && data.message.text) {
                object.isEasyStudioDocument = data.message.text == "true";
                return object.isEasyStudioDocument;
            }
          })

        .error(function (data, status, headers, config) {
            loggingService.handleError(urlToRun, status);
            return false;
        });
	    return true;
    }

    //---------------------------------------------------------------------------------------------
	this.cloneAsEasyStudioDocument = function (object) {

	    if (object.isLoading == true)
	        return;

	    if ((this.currentApplication == undefined || this.currentModule == undefined)) {
	        this.changeCustomizationContext(
                function (response) {
                    if (response)
                        thiz.cloneAsEasyStudioDocument(object);
                });
	        return;
	    }

	    $rootScope.objectCurrentlyLoading = object;
	    object.isLoading = true;

	    var modalInstance = $uibModal.open({
	        templateUrl: 'templates/Common/cloneDocumentDialog.html',
	        controller: "ESCloneDocumentController",
	        easyStudioService,
	        localizationService,
	        settingsService
	    });
	    
	}

    //---------------------------------------------------------------------------------------------
	this.executeCloneDocument = function (object, docName, docTitle) {
	    
	    if (docName == undefined || this.currentApplication == undefined || this.currentModule == undefined)
	        return;

	    if (docTitle == undefined)
	        docTitle = docName;

	    var ns = object.target;
	    var newNs = this.currentApplication + "." + this.currentModule + ".DynamicDocuments." + docName;
	    var urlToRun = this.tbBaseRoute + '/cloneEasyStudioDocument/?ns=' + encodeURIComponent(ns);
	    urlToRun += "&newNamespace=" + encodeURIComponent(newNs);
	    urlToRun += "&newTitle=" + encodeURIComponent(docTitle);

	    if ($rootScope.objectCurrentlyLoading != undefined)
	        $rootScope.objectCurrentlyLoading.isLoading = false;

	    document.body.style.cursor = 'wait';

	    generalFunctionsService.post(urlToRun)

        .success(function (data, status, headers, config) {
            object.isLoading = false;
            document.body.style.cursor = 'default';
            if (data && data.message && data.message.text)
                loggingService.showDiagnostic(data.message.text);
            else
                loggingService.showDiagnostic(localizationService.getLocalizedElement("New Document Created with Success"));
            $rootScope.$emit('runFunctionCompleted', object);
        })

        .error(function (data, status, headers, config) {
            object.isLoading = false;
            document.body.style.cursor = 'default';
            loggingService.handleError(urlToRun, status);
        });
	}

    //---------------------------------------------------------------------------------------------
	this.cloneDocumentClear = function (object) {

	    object.isLoading = false;
	    document.body.style.cursor = 'default';
    }


	//---------------------------------------------------------------------------------------------
	this.openEasyStudio = function (object, customization) {

		if (object.isLoading == true)
			return;

		if (customization != undefined && !thiz.isCustomizationEnabled(customization)) {
			return;
		}

		customizationName = undefined;
		if (customization != undefined)
			customizationName = customization.customizationName;

		var ns = object.target;
		var objType = object.objectType.toLowerCase();

		var urlToRun = this.tbBaseRoute + '/runEasyStudio/?ns=' + encodeURIComponent(ns);

		if (customizationName != undefined)
			urlToRun += "&customization=" + encodeURIComponent(customizationName);

		if ($rootScope.objectCurrentlyLoading != undefined)
			$rootScope.objectCurrentlyLoading.isLoading = false;

		$rootScope.objectCurrentlyLoading = object;
		object.isLoading = true;

		document.body.style.cursor = 'wait';

		generalFunctionsService.post(urlToRun)
        .success(function (data, status, headers, config) {
        	if (data && data.windowUrl)
        		window.open(data.windowUrl);
        	$rootScope.$emit('runFunctionCompleted', object);

        	var objType = object.objectType.toLowerCase();
        	if (objType == 'function' || objType == 'officeitem') {
        		object.isLoading = false;
        		document.body.style.cursor = 'default';

        	}
        })
        .error(function (data, status, headers, config) {
        	loggingService.handleError(urlToRun, status);
        	});
       	}
  }