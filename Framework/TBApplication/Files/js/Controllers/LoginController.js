//---------------------------------------------------------------------------------------------
function LoginController($scope, $location, $uibModal, $http, generalFunctionsService, localizationService, settingsService, loggingService, generalFunctionsService) {

	$scope.localizationService = localizationService;
	$scope.settingsService = settingsService;
	$scope.loggingService = loggingService;
	$scope.generalFunctionsService = generalFunctionsService;
	$scope.existMoreCompanies = false;
	$scope.loginEnabled = false;
	$scope.rememberMe = false;  //todo 
	$scope.autoLoginVisible = false;
	$scope.winAuth = false;     //todo  
	$scope.winAuthVisible = false;     //todo 
	$scope.ccdVisible = true;
	$scope.ccd = false;     //todo  
	$scope.lastUsername = "";   //todo 
	$scope.lastPassword = "";   //todo 
	$scope.loading = false;
	$scope.options = [];
	$scope.lastCompany;
	$scope.allvisible = false;
	$scope.changeWinNTLoaded = false;
	$scope.NTlastCompany = "";
	$scope.NTlastUsername = "";
	$scope.NTlastPassword = "";
	$scope.passwordPlaceholder = "";
	$scope.isrelogin = false;
	$scope.overwrite = false;
	$scope.viewCompanies = false;
	$scope.settingsOpen = false;
	$scope.settingsmouseover = false;
	$scope.companywasvisible = false;
	$scope.viewuserdropdown = false;
	$scope.cryptedtoken = "";
	$scope.tblink = "";


    //---------------------------------------------------------------------------------------------
	$(document).ready(function () {
	 
	    var prmstr = window.location.search.substr(1);
	    var tmparr = prmstr.split("=");
	    $scope.cryptedtoken = tmparr[1];

	});

    //siccome lo spazio è un carattere che la dropdown bootstrap gestisce in maniera speciale  io eseguo uno stop propagation per fare in modo che si possano inserire spazi nel cambio password e nel cambio login
    //---------------------------------------------------------------------------------------------
	$scope.keydown = function ($event) {
	    if ($event.which == 32) $event.stopPropagation();
	}

    //---------------------------------------------------------------------------------------------
	$(document).ready(function () {

	    var q = window.location.search;

	    var r = "?tk=";
        //se non arriva una string che contiene tk vuol dire che è roba che non mi interessa e tralascio
	    if (q.substring(0, r.length) === r)
	    {
	        var prmstr = window.location.search.substr(4); //substring eliminando ?tk=

	        var tmparr = prmstr.split("&tblink=");

	        $scope.cryptedtoken = tmparr[0];
	        $scope.tblink = tmparr[1];
	    }
	});

	//---------------------------------------------------------------------------------------------
	$scope.clearError = function () {
	   
		$scope.setError("");
		$scope.error = false;
	};

	//---------------------------------------------------------------------------------------------
	$scope.setCompany = function (company) {
		$scope.lastCompany = company;
		$scope.viewCompanies = false;
	};


	//---------------------------------------------------------------------------------------------
	$scope.openCompanies = function () {
		if ($scope.loading)
			return;
		$scope.viewCompanies = !$scope.viewCompanies;
		$scope.settingsOpen = false;

	};

	//---------------------------------------------------------------------------------------------
	$scope.openSettingshover = function () {

		if ($scope.loading)
			return;

		if ($scope.settingsOpen) return;
		$scope.settingsmouseover = true;

		$scope.settingsOpen = !$scope.settingsOpen;

		if ($scope.viewCompanies) {
			$scope.viewCompanies = false;
			$scope.companywasvisible = true;
		}
	};

	//---------------------------------------------------------------------------------------------
	$scope.closeSettingshover = function () {

		if ($scope.loading)
			return;

		if ($scope.settingsmouseover && $scope.settingsOpen)
			$scope.settingsOpen = !$scope.settingsOpen;
		if ($scope.companywasvisible) {
			$scope.viewCompanies = true;
			$scope.companywasvisible = false;
		}
	};

	//---------------------------------------------------------------------------------------------
	$scope.openSettings = function () {

		if ($scope.loading)
			return;

		if ($scope.settingsmouseover && $scope.settingsOpen) {
			$scope.settingsmouseover = false;
			$scope.companywasvisible = false
			$scope.viewCompanies = false;
			return;
		}
		$scope.settingsOpen = !$scope.settingsOpen;
		$scope.viewCompanies = false;
	};

	//---------------------------------------------------------------------------------------------
	$scope.settingsSetted = function () {

		$scope.settingsOpen = false;

	};

	//---------------------------------------------------------------------------------------------
	$scope.changeClass = function () {

		$scope.allvisible = true;
		var myEl = angular.element(document.querySelector('#loginContainer'));
		myEl.removeClass('elementInvisible');

		var loader = angular.element(document.querySelector('#loader'));
		loader.removeClass('loading');
	};

	//---------------------------------------------------------------------------------------------
	$scope.getLoginInitInformation = function (callback) {
		$http.post('getLoginInitInformation/')
		.success(function (data, status, headers, config) {
			callback(data);

		})
		.error(function (data, status, headers, config) {
			$scope.setError('getLoginInitInformation' + status)
		});
	};

	//---------------------------------------------------------------------------------------------
	$scope.valorize = function (result, winnt) {

		if (result == null) return;

		$scope.lastUsername = result.userName;
		$scope.lastCompany = result.company;
		$scope.lastPassword = result.passWord;
		
		$scope.autoLoginVisible = generalFunctionsService.parseBool(result.autoLoginVisible);
		if ($scope.autoLoginVisible)
		    $scope.rememberMe = generalFunctionsService.parseBool(result.rememberMe);
		else $scope.rememberMe = false;
		$scope.winAuthVisible = generalFunctionsService.parseBool(result.windowsAuthentication);
		$scope.ccdVisible = generalFunctionsService.parseBool(result.clearCachedDataVisible);

		$scope.NTlastCompany = result.NTcompany;
		$scope.NTlastUsername = result.NTLoginName;
		$scope.NTlastPassword = result.NTpassword;

		if (winnt) $scope.winAuth = true;
		else $scope.winAuth = generalFunctionsService.parseBool(result.windowsAuthenticationSelected);


		if ($scope.winAuth) {
			$scope.changeWinNTLoaded = true;
			$scope.lastCompany = $scope.NTlastCompany;
			$scope.lastUsername = $scope.NTlastUsername;
			$scope.lastPassword = $scope.NTlastPassword;

		}
		if (result.messages)
			$scope.loggingService.showDiagnostic(
				result.messages,
				{ onOk: function () { $scope.loadCompanies(); } }
				);
		else
			$scope.loadCompanies();
	};

	//---------------------------------------------------------------------------------------------
	$scope.useronBlur = function () {
		$scope.loadCompanies();

	}
	$scope.keypressed = false;
	//---------------------------------------------------------------------------------------------
	$scope.keypress = function () {
		if ($scope.existMoreCompanies)
			$scope.lastCompany = "";

	}

	$scope.error = false;
	//---------------------------------------------------------------------------------------------
	$scope.setError = function (error) {
		$scope.error = true;
		$("#error").text(error);
	}

    //---------------------------------------------------------------------------------------------
	$scope.loadCompanies = function (callback) {

	    $scope.clearError();
	    if ($scope.loading)
	        return;
	    $scope.keypressed = false;
	    if (!$scope.lastUsername || $scope.lastUsername.length === 0) {
	        $scope.loginEnabled = false;
	        $scope.settingsOpen = false;
	        $scope.existMoreCompanies = false;
	        $scope.viewCompanies = false;
	        return;
	    }
	    var copyCompany = $scope.lastCompany;

	    $scope.getloginCompanies($scope.lastUsername, function (result) {
	        var companies = generalFunctionsService.ToArray(result.Company).sort(comparestring);

	        if (result != null) {
	            $scope.companiesloaded = true;
	            if (companies.length == 0) {//todo message box verifica che siano visibili gli alert anche se siamo nella pagina login
	                $scope.setError(localizationService.getLocalizedElement('NoCompany'));
	                $scope.lastCompany = "";
	                $scope.loginEnabled = false;
	                $scope.settingsOpen = false;
	                $scope.existMoreCompanies = false;
	                $scope.viewCompanies = false;
	                return;
	            }

	            $scope.loginEnabled = result.Company.length > 0;
	            $scope.existMoreCompanies = companies.length > 1;

	            var found = false;
	            var newOptions = [];
	            for (var i = 0; i < companies.length; i++) {
	                found = found || (copyCompany == companies[i].name);
	                newOptions.push(companies[i].name);
	            }
	            $scope.options = newOptions;

	            if (found) { $scope.lastCompany = copyCompany; }
	            else {
	                $scope.lastCompany = $scope.options[0];
	            }
	        }
	        else {

	            $scope.setError(localizationService.getLocalizedElement('NoCompany'));

	        }

	        if (callback != undefined) {
	            callback();
	        }

	    });
	};

    //---------------------------------------------------------------------------------------------
	function comparestring(a, b) {
	    var nameA = a.name.toLowerCase(), nameB = b.name.toLowerCase();
	    if (nameA < nameB) //sort string ascending
	        return -1;
	    if (nameA > nameB)
	        return 1;
	    return 0; //default return value (no sorting)
	}

    //---------------------------------------------------------------------------------------------
	$scope.changeRemMe = function () {
	    $scope.rememberMe = !$scope.rememberMe;//con lo stop propagation non aggionrna il ng-model 
	};

    //---------------------------------------------------------------------------------------------
	$scope.isAutoLoginVisible = function () {

	    return $scope.autoLoginVisible;
	};

    //---------------------------------------------------------------------------------------------
	$scope.changeccd = function () {
	    $scope.ccd = !$scope.ccd;//con lo stop propagation non aggionrna il ng-model 
	};

    //---------------------------------------------------------------------------------------------
	$scope.changeWinNT = function (force) {
	    //ho cercato un modo per evitare che cliccando furiosamente sulla checkbox della winAuth non si incasinasse perdendo la sincro col bool ma  poi ho rimandato a tempi migliori, dettaglio


	    if (force)
	        $scope.winAuth = force;
	    else
	        $scope.winAuth = (document.getElementById('win').checked);//con lo stop propagation non aggionrna il ng-model

	    if (!$scope.winAuth) {
	        $scope.lastUsername = "";
	        $scope.lastPassword = "";
	        $scope.loadCompanies();

	        //vorrei impostare il fuoco sulla txt dell'utente, ma  verificare che non lo fa
	        var txt = angular.element(document.querySelector('#UsernameInput'));

	        txt.focus;
	        return;
	    }

	    if ($scope.changeWinNTLoaded) {

	        $scope.lastCompany = $scope.NTlastCompany;
	        $scope.lastUsername = $scope.NTlastUsername;
	        $scope.lastPassword = $scope.NTlastPassword;
	        $scope.loadCompanies();
	    }
	    else {


	        $scope.getLoginInitInformation(function (result) {

	            $scope.valorize(result, true);
	        });
	    }
	};

    //---------------------------------------------------------------------------------------------
	$scope.getloginCompanies = function (user, callback) {

	    var urlToRun = 'getloginCompanies/?user=' + encodeURIComponent(user);

	    $http.post(urlToRun)
		.success(function (data, status, headers, config) {
		    callback(data.Companies);
		})
		.error(function (data, status, headers, config) {
		    $scope.setError('getloginCompanies' + status);
		});
	};

    //ready
    //---------------------------------------------------------------------------------------------
	angular.element(document).ready(function () {

	    //questo per  far cambiare il booleano della visibilità del change login che sull onblur non veniva recepito.
	    $("body").click(function (e) {

	        if ($(e.target).attr('id') !== "cldiv") {

	            settingsService.changeLoginChangeVisibility1(false)
	        }
	    });
	    //


	    $scope.getLoginInitInformation(function (result) {

	        $scope.valorize(result, false);

	        $scope.changeClass();
	        $scope.isrelogin = settingsService.getRelogin();

	        if ($scope.isrelogin) {

	            $scope.rememberMe = false;
	            $scope.lastPassword = '';
	            return;
	        }

	        //siamo in condizioni di nessun utente come lastuser ma c'è  possibiltà di utente in sicurezzaintegrata, allora propongo quella di default
	        if (!$scope.winAuth && $scope.winAuthVisible) {
	            if ($scope.NTlastCompany != undefined && $scope.NTlastCompany.length > 0 &&
					$scope.lastUsername != undefined && $scope.lastUsername.length > 0 &&
					$scope.lastPassword != undefined && $scope.lastPassword.length > 0 &&
					$scope.lastUsername == undefined || $scope.lastUsername.length == 0) {
	                $scope.changeWinNT(true);
	            }
	        }

	        $scope.loginEnabled = ($scope.lastCompany != undefined || $scope.lastCompany.length > 0);//la getlogininformation controlla gia la validità della lastcompany

	        //autologin
	        if ($scope.rememberMe && !$scope.isrelogin) {
	            $scope.companiesloaded = true;//se no si ferma in attesa di selezionare la company
	            //todo verifica presenza di tutti i dati se no lo tolgo
	            $scope.doLogin();
	        }
	    });
	});

    //---------------------------------------------------------------------------------------------
	$scope.GoToLogin = function () {
	    $scope.clearError()
	    $scope.loading = true;
	    $scope.allvisible = false;
	    settingsService.setRememberMe($scope.rememberMe);

	    var urlToRun = 'doLogin/?user=' + encodeURIComponent($scope.lastUsername) + '&company=' + encodeURIComponent($scope.lastCompany) + '&password=' +
            encodeURIComponent($scope.lastPassword) + '&overwrite=' + $scope.overwrite + '&rememberme=' + $scope.rememberMe + '&winNT=' + $scope.winAuth + '&ccd=' + $scope.ccd + '&relogin=' + $scope.isrelogin + '&changeAutologinInfo=' + $scope.changeAutologinInfo + '&saveAutologinInfo=' + $scope.saveAutologinInfo;
	    $scope.saveAutologinInfo = "undefined";
	    $http.post(urlToRun)
        .success(function (data, status, headers, config) {

            if (data && data.success) {
                if (data.messages) {
                    $scope.loggingService.showDiagnostic(data.messages, {
                        onOk: function () {
                            if (data.menuPage)
                                window.location.href = data.menuPage;
                        }
                    });
                }
                else if (data.menuPage)
                    window.location.href = data.menuPage;
            }
            else {


                $scope.loading = false;
                if (data.alreadyLogged) {
                    var wasEnabled = $scope.loginEnabled;
                    $scope.loginEnabled = false;
                    $scope.loggingService.showDiagnostic(
						localizationService.getLocalizedElement('ErrUserAlreadyLogged'),
						{
						    onYes: function () {
						        $scope.overwrite = true;
						        $scope.loading = false;
						        $scope.loginEnabled = wasEnabled;
						        $scope.doLogin();
						    },
						    onNo: function () {
						        $scope.loading = false;
						        $scope.loginEnabled = wasEnabled;
						    }
						});
                }



                if (data.changeAutologinInfo) {
                    var wasEnabled = $scope.loginEnabled;
                    $scope.loginEnabled = false;
                    $scope.loggingService.showDiagnostic(
						localizationService.getLocalizedElement('changeAutologinInfo'),
						{
						    onYes: function () {
						        $scope.loading = false;
						        $scope.saveAutologinInfo = "1";
						        $scope.loginEnabled = wasEnabled;
						        $scope.doLogin();
						    },
						    onNo: function () {
						        $scope.loading = false;
						        $scope.saveAutologinInfo = "-1";
						        $scope.loginEnabled = wasEnabled;
						        $scope.doLogin();
						    }
						});
                }



                else if (data.changePassword) {
                    $scope.loggingService.openDialog("templates/changePasswordModal.html", ChangePasswordController);
                }
                else if (data.messages) {
                    $scope.loggingService.showDiagnostic(data.messages);
                }

            }
        })
        .error(function (data, status, headers, config) {
            $scope.setError('Connection error; data returned: ' + data + " status: " + status + " headers: " + headers + " config: " + config);
        });
	}
    //---------------------------------------------------------------------------------------------
	$scope.doLogin = function () {

	    if (!$scope.loginEnabled || $scope.loading)
	        return;



	    if ($scope.cryptedtoken != null && $scope.cryptedtoken.length > 1) { $scope.doSSOLogin(); return; }//ssologin se ho il token

	    if (!$scope.companiesloaded) {
	        $scope.loading = false;
	        $scope.loadCompanies(
                function () {
                    if ($scope.existMoreCompanies) {
                        alert(localizationService.getLocalizedElement('selectCompany'));
                        return;
                    }

                    $scope.loading = true;
                    $scope.GoToLogin();

                })
	        return;
	    }


	    //se non ho caricvato le company devo farlo adesso , se è una proseguo se è più di una farò scegliere ed interrompo la login.}

	    $scope.GoToLogin();
	};

    //---------------------------------------------------------------------------------------------
	$scope.doSSOLogin = function () {

	    if (!$scope.loginEnabled || $scope.loading) {

	        return;
	    }


	    if ($scope.cryptedtoken.length < 1)
	        return;

	    $scope.clearError()
	    $scope.loading = true;
	    $scope.allvisible = false;
	    settingsService.setRememberMe($scope.rememberMe);

	    var urlToRun = 'ssologin?tk=' + $scope.cryptedtoken + '&user=' + encodeURIComponent($scope.lastUsername) + '&company=' + encodeURIComponent($scope.lastCompany) + '&password=' + encodeURIComponent($scope.lastPassword) + '&winNT=' + $scope.winAuth + '&tblink=' + encodeURIComponent($scope.tblink);

	    $scope.saveAutologinInfo = "undefined";
	    $http.post(urlToRun)


        .success(function (data, status, headers, config) {

            if (data && !data.success) {
                $scope.loading = false;
                if (data.alreadyAssociated) {
                    $scope.loggingService.showDiagnostic("Questo utente Mago è già associato ad un altro utente Infinity. Impossibile proseguire.");
                }
                else if (data.passworderror) {
                    $scope.loggingService.showDiagnostic(
                        localizationService.getLocalizedElement('passwordError'));
                }

                else if (data.alreadyLogged) {
                    var wasEnabled = $scope.loginEnabled;
                    $scope.loginEnabled = false;
                    $scope.loggingService.showDiagnostic(
            			localizationService.getLocalizedElement('ErrUserAlreadyLogged'),
            			{
            			    onYes: function () {
            			        $scope.overwrite = true;
            			        $scope.loading = false;
            			        $scope.loginEnabled = wasEnabled;
            			        $scope.doLogin();
            			    },
            			    onNo: function () {
            			        $scope.loading = false;
            			        $scope.loginEnabled = wasEnabled;
            			    }
            			});
                }
                else if (data.changeAutologinInfo) {
                    var wasEnabled = $scope.loginEnabled;
                    $scope.loginEnabled = false;
                    $scope.loggingService.showDiagnostic(
            			localizationService.getLocalizedElement('changeAutologinInfo'),
            			{
            			    onYes: function () {

            			        $scope.loading = false;
            			        $scope.saveAutologinInfo = "1";
            			        $scope.loginEnabled = wasEnabled;
            			        $scope.doLogin();
            			    },
            			    onNo: function () {

            			        $scope.loading = false;
            			        $scope.saveAutologinInfo = "-1";
            			        $scope.loginEnabled = wasEnabled;
            			        $scope.doLogin();
            			    }
            			});
                }

                else if (data.changePassword) {
                    $scope.loggingService.openDialog("templates/changePasswordModal.html", ChangePasswordController);
                }
                else if (data.messages) {
                    $scope.loggingService.showDiagnostic(data.messages);
                }

            };
        });
	};




    //fine controller
};
