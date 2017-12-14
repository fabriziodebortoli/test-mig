function CustomizationContextController($scope, $log, $http, $rootScope, $uibModal, localizationService, generalFunctionsService, loggingService, easyStudioService, settingsService) {

	$scope.menu = undefined;
	$scope.favorites = undefined;
	$scope.generalFunctionsService = generalFunctionsService;
	$scope.localizationService = localizationService;
	$scope.loggingService = loggingService;
	$scope.easyStudioService = easyStudioService;
	$scope.settingsService = settingsService;

	$scope.customization = "";

	//---------------------------------------------------------------------------------------------
	angular.element(document).ready(function () {
		easyStudioService.getCustomizationContextAppAndModule();
		$scope.listenXSocket();
	});

	//---------------------------------------------------------------------------------------------
	$scope.listenXSocket = function () {
		controllerConnection = new TBWebSocket(generalFunctionsService.getCookieByName('authtoken'));
		controllerConnection.on("CustomizationContextUpdated", function (response) {
			try {
				//var oData = d.data;
				$scope.$apply(function () {
					$scope.easyStudioService.extractApplicationAndModuleFromResponse(response);
				});
			}
			catch (ex) {
				$scope.loggingService.showDiagnostic(ex);
			}
		});
	}

	//---------------------------------------------------------------------------------------------
	$scope.closeCustomizationContext = function () {
		$scope.easyStudioService.closeCustomizationContext();
	}

	//---------------------------------------------------------------------------------------------
	$scope.openEasyStudioAndContextIfNeeded = function (object, customization) {
		if (!(easyStudioService.currentApplication !== undefined && easyStudioService.currentApplication !== null && easyStudioService.currentModule !== undefined && easyStudioService.currentModule !== undefined)) {
			$scope.changeCustomizationContext(function () {
				easyStudioService.openEasyStudio(object, customization);
			});
		}
		else 
			easyStudioService.openEasyStudio(object, customization);
	}

    //---------------------------------------------------------------------------------------------
	$scope.cloneAsEasyStudioDocumentIfNeeded = function (object) {
	    if (!(easyStudioService.currentApplication !== undefined && easyStudioService.currentApplication !== null && easyStudioService.currentModule !== undefined && easyStudioService.currentModule !== undefined)) {
	        $scope.changeCustomizationContext(function () {
	            easyStudioService.cloneAsEasyStudioDocument(object);
	        });
	    }
	    else
	        easyStudioService.cloneAsEasyStudioDocument(object);
	}
	//---------------------------------------------------------------------------------------------
	$scope.changeCustomizationContext = function (callback) {
		//$scope.easyStudioService.changeCustomizationContext(); // vecchio context Menu

		easyStudioService.readDefaultContext(false);
		easyStudioService.getAllAppsAndModules(function (data) { return data; });

		var modalInstance = $uibModal.open({
			templateUrl: 'templates/Common/customizationContextDialog.html',
			controller: ModalEasyStudioCustomizationCtrl,
			easyStudioService,
			localizationService,
			settingsService
		});

		modalInstance.result.then(function () {
			if (callback != undefined)
				callback();
		});

	}

	//---------------------------------------------------------------------------------------------
	$scope.openDefaultContext = function () {
		easyStudioService.readDefaultContext(true);
	}

	//---------------------------------------------------------------------------------------------
	$scope.getCustomizationContextTooltip = function () {
		return localizationService.getLocalizedElement('CurrentApplication') + ": " + easyStudioService.currentApplication + "<br/>" + localizationService.getLocalizedElement('CurrentModule') + ": " + easyStudioService.currentModule;
	}

	//---------------------------------------------------------------------------------------------
	$scope.isCustomizationContextVisible = function () {
		return settingsService.isEasyStudioActivated && easyStudioService.currentApplication != undefined && easyStudioService.currentModule != undefined;
	}

}


//=================================================================================================
function ModalEasyStudioCustomizationCtrl($scope, $uibModalInstance, easyStudioService, localizationService, settingsService) {

	$scope.localizationService = localizationService;

	$scope.title = localizationService.getLocalizedElement('Customization Context');
	$scope.defaultNewApp = localizationService.getLocalizedElement('NewApplication');
	$scope.defaultNewMod = localizationService.getLocalizedElement('NewModule');

	$scope.memory = easyStudioService.memory;
	$scope.application = easyStudioService.currentApplication;
	$scope.applications = easyStudioService.applications;
	$scope.isDevelopEd = easyStudioService.developEd;
	$scope.module = easyStudioService.currentModule;

	$scope.showModules = $scope.application !== undefined;
	$scope.modules = [];
	$scope.addPairVisible = false;
	$scope.addModuleVisible = false;
	$scope.isThisPairDefault = false;
	$scope.newAppName = undefined;
	$scope.newModName = undefined;


	//---------------------------------------------------------------------------------------------
	$scope.close = function () {
		$uibModalInstance.close();
	};

	//---------------------------------------------------------------------------------------------
	$scope.ok = function () {
		easyStudioService.SetContext($scope.application, $scope.module, $scope.isThisPairDefault);
		$scope.isThisPairDefault = false;
		$uibModalInstance.close();
	};

	//---------------------------------------------------------------------------------------------
	$scope.disabledIf = function () {
		return $scope.application === undefined || $scope.module === undefined || $scope.addPairVisible;
	};

	//---------------------------------------------------------------------------------------------
	$scope.cancel = function () {
		$scope.application = undefined;
		$scope.module = undefined;
		$uibModalInstance.close();
	};


	//---------------------------------------------------------------------------------------------
	$scope.setApplic = function (app) {
		$scope.application = app;
		$scope.formData.isFavorite = false;
		$scope.modules = $scope.getModulesOf(app);

		$scope.hightlightApp(app);
		$scope.module = undefined;

		$scope.setInvisiblePair();
		$scope.setInvisibleMod();
	};


	$scope.lastAppSelected = undefined;
	$scope.lastModSelected = undefined;
	//---------------------------------------------------------------------------------------------
	$scope.hightlightApp = function (elem) {
		if (!elem)
			return;
		if ($scope.lastAppSelected) {
			$scope.lastAppSelected.className = "";
		}
		itemSelected = document.getElementById(elem);
		if (itemSelected) {
			$scope.lastAppSelected = itemSelected;
			itemSelected.className = "selected";
		}

		//se c'è un solo modulo per l'app, lo preseleziono in automatico
		if ($scope.modules.length == 1) {
			$scope.module = mod = $scope.modules[0];
			$scope.modules[0].className = "selected";
			$scope.hightlightMod(mod);
		}
		//se invece ho già indicazione di un modulo, controllo che esista e lo evidenzio
		else if ($scope.module && $scope.ExistsModule(elem, $scope.module)) {
			$scope.hightlightMod($scope.module);
		}
	};

	//---------------------------------------------------------------------------------------------
	$scope.hightlightMod = function (elem) {
		if ($scope.application == easyStudioService.defaultApplication && $scope.module == easyStudioService.defaultModule) {
			$scope.formData.isFavorite = true;
		}

		if ($scope.lastModSelected) {
			$scope.lastModSelected.className = "";
		}
		itemSelected = document.getElementById(elem);
		if (!itemSelected) return;
		$scope.lastModSelected = itemSelected;
		itemSelected.className = "selected";
	};


	//---------------------------------------------------------------------------------------------
	$scope.setModule = function (mod) {
		$scope.module = mod;
		$scope.hightlightMod(mod);
		$scope.setInvisiblePair();
		$scope.setInvisibleMod();
	};

	//---------------------------------------------------------------------------------------------
	$scope.getModulesOf = function (app) {
		var modulesFound = [];
		for (var i = 0; i < $scope.memory.length; i++) {
			var doubleItem = $scope.memory[i];
			if (doubleItem[0] == app) {
				modulesFound.push(doubleItem[1]);
			}
		}
		return modulesFound;
	};

	//---------------------------------------------------------------------------------------------
	$scope.addNewPair = function (newAppName, newModName) {
		if (newAppName === undefined || newModName === undefined)
			return;
		$scope.newAppName = newAppName;
		$scope.newModName = newModName;
		if ($scope.memory.indexOf([$scope.newAppName, $scope.newModName]) === -1) { //nessuna occorrenza
			var e = document.getElementById("ApplicationType");
			var type = "Customization";
			if (e !== null)
				type = e.options[e.selectedIndex].value;
			easyStudioService.CreateContext($scope.newAppName, $scope.newModName, type);
			$scope.memory.push([$scope.newAppName, $scope.newModName]);

			$scope.addPairVisible = false;
			$scope.addModuleVisible = false;

			$scope.application = $scope.newAppName;
			$scope.module = $scope.newModName;
			if ($scope.applications.indexOf($scope.newAppName) === -1) { //nessuna occorrenza
				$scope.applications.push($scope.newAppName);
			}
			$scope.modules = $scope.getModulesOf($scope.newAppName);
		}
		easyStudioService.getAllAppsAndModules(function (data) {
			return data;
		});
		return true;
	}

	$scope.formData = {};
	//---------------------------------------------------------------------------------------------
	$scope.addNewMod = function () {
		$scope.newAppName = $scope.application;
		$scope.newModName = $scope.formData.newModName;
		$scope.addPairVisible = false;
		$scope.addModuleVisible = false;
		$scope.addNewPair($scope.application, $scope.newModName);
	}

	//---------------------------------------------------------------------------------------------
	$scope.setInvisibleMod = function () {
		$scope.addModuleVisible = false;
	}

	//---------------------------------------------------------------------------------------------
	$scope.setInvisiblePair = function () {
		$scope.addPairVisible = false;
	}

	//---------------------------------------------------------------------------------------------
	$scope.setVisibleMod = function () {
		$scope.addModuleVisible = true;
		if ($scope.application !== undefined)
			$scope.GenerateNewModuleName($scope.application);
		$scope.setInvisiblePair();
	}

	//---------------------------------------------------------------------------------------------
	$scope.setVisiblePair = function () {
		$scope.addPairVisible = true;
		var newApp = $scope.GenerateNewApplicationName();
		$scope.GenerateNewModuleName(newApp);
		$scope.setInvisibleMod();
	}

	//---------------------------------------------------------------------------------------------
	$scope.GenerateNewApplicationName = function () {
		var i = 0;
		var newName = undefined;
		do {
			i++;
			newName = $scope.defaultNewApp + i.toString();

		} while ($scope.ExistsApplication(newName));
		$scope.addPairVisible = true;
		$scope.formData.newAppName = newName;
		return newName;
	}

	//---------------------------------------------------------------------------------------------
	$scope.GenerateNewModuleName = function (appName) {
		var i = 0;
		var newName = undefined;
		do {
			i++;
			newName = $scope.defaultNewMod + i.toString();

		} while ($scope.ExistsModule(appName, newName));
		$scope.formData.newModName = newName;
	}

	//---------------------------------------------------------------------------------------------
	$scope.ExistsApplication = function (newName) {
		var list = [];
		list = $scope.applications;
		return list.indexOf(newName) !== -1;
	}

	//---------------------------------------------------------------------------------------------
	$scope.ExistsModule = function (appName, newModName) {
		if (appName === undefined || newModName === undefined)
			return;
		var list = [];
		list = $scope.getModulesOf(appName);
		if (list.indexOf(newModName) !== -1)
			return true;
		return false;//nessuna occorrenza
	}

	//---------------------------------------------------------------------------------------------
	angular.element(document).ready(function () {
		if ($scope.application && $scope.module) {
			var mod = $scope.module;
			$scope.setApplic($scope.application);
			$scope.setModule(mod);
		}
		else
			easyStudioService.getCustomizationContextAppAndModule();
	});


	$scope.ifHasToBe = function (mod) {
		return (($scope.modules.length == 1) ||
		($scope.module && $scope.module === mod));
	}

	$scope.ApplicIsEmpty = function () {
		return true;
		//TODOROBY
	}

	$scope.SetDefaultContext = function () {
		$scope.isThisPairDefault = $scope.application != "" && $scope.module != "" && $scope.application != undefined && $scope.module != undefined;
	}
};
