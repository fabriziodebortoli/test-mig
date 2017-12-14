var loggingService = function ($http, $log, $uibModal, localizationService) {
	var thiz = this;
   
    //---------------------------------------------------------------------------------------------
    this.LogError = function (error) {
    }

	//---------------------------------------------------------------------------------------------
    this.handleError = function (sRequest, status) {
    	if (status === 401)
    		return;
     	thiz.showDiagnostic("Error during request " + sRequest + "; status code: " + status);
    }

	///messages : un oggetto del tipo { text: "...", type: numero_dell'enumerativo_del_diagnostic_manager}, oppure un array di questi oggetti
	///buttons: un oggetto che contiene le funzioni da associare ai bottoni, può contenere tutti i seguenti: { onOk: f1, onYes: f2, onCancel: f3, onNo: f4 }
	//le funzioni passate per i bottoni determinano anche la visibilità degli stessi
	//---------------------------------------------------------------------------------------------
    this.showDiagnostic = function (messages, buttons) {
    	if (!buttons)
    		buttons = {};
    	if (typeof (messages) === "string") {
    		messages = [{ text: messages }];
    	}
    	else if (messages.constructor !== Array) {
    		messages = [messages];
    	}
    	var defaultAction = buttons.onCancel ? buttons.onCancel : buttons.onNo;

    	var modalInstance = $uibModal.open({
    		templateUrl: '/tb/menu/templates/diagnostic.html',
    		backdrop: 'static',
    		controller: function ($scope) {
    			$scope.yes = function () {
    				var fn = buttons.onOk ? buttons.onOk : buttons.onYes;
    				modalInstance.close(fn);
    			};
    			$scope.no = function () {
    				modalInstance.close(buttons.onNo);
    			};
    			$scope.cancel = function () {
    				modalInstance.close(buttons.onCancel);
    			};
    			$scope.yesText = localizationService.getLocalizedElement('OK');
    			$scope.trim = function (text) { return text.replace(/\n/g, "<br>") };
    			if (buttons.onYes)
    				$scope.yesText = localizationService.getLocalizedElement('Yes');
    			if (buttons.onOk)
    				$scope.yesText = localizationService.getLocalizedElement('OK');
    			if (buttons.onNo)
    				$scope.noText = localizationService.getLocalizedElement('No');
    			if (buttons.onCancel)
    				$scope.cancelText = localizationService.getLocalizedElement('Cancel');
    			$scope.title = localizationService.getLocalizedElement('Messages');
    			$scope.messages = messages;
    		},
    		resolve: {
    		}
    	});
    	modalInstance.result.then(function (fn) {
    		if (fn)
    			setTimeout(fn, 1);//così la modale si chiude
    	},
		function () {
			if (defaultAction)
				setTimeout(setTimeout(defaultAction, 1));//così la modale si chiude, 1);//così la modale si chiude
		}
		);

    };

    this.openDialog = function (templateUrl, controller) {
    	return $uibModal.open({
    		templateUrl: templateUrl,
    		backdrop: 'static',
    		controller: controller
    	});

    };
};
