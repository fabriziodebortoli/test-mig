//---------------------------------------------------------------------------------------------
function CalendarController($scope, $http, $log, imageService, settingsService, menuService, localizationService, loggingService, generalFunctionsService) {

    $scope.settingsService = settingsService;
    $scope.localizationService = localizationService;
    $scope.menuService = menuService;
    $scope.imageService = imageService;
    $scope.loggingService = loggingService;
    $scope.generalFunctionsService = generalFunctionsService;
    $scope.applicationDate = undefined;

    //---------------------------------------------------------------------------------------------
    angular.element(document).ready(function () {

        var promise = menuService.getMenuElements();
        promise.then(function (menu) {
            $scope.getApplicationDate(function (data) {
                var d = data.split("T");
                var parts = d[0].split("-");
                $scope.applicationDate = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
            });

            
            $scope.getCulture(function (culture) {
                $scope.culture = culture;
            });

            $scope.getApplicationDateFormat(function (data) {
                $scope.applicationDateFormat = data;

                $('.datepicker').datepicker({
                    format: $scope.applicationDateFormat,
                    autoclose: 1,
                    language: $scope.culture,
                    todayHighlight: 1            
                });

      
                $('.datepicker').datepicker('update', $scope.applicationDate);
            });
        });
    });


    //---------------------------------------------------------------------------------------------
    $scope.changeApplicationDate = function () {
        if ($(".dateDropdownContainer").css('display') == 'none')
        {
            $scope.getApplicationDate(function (data) {
                var d = data.split("T");
                var parts = d[0].split("-");
                $scope.applicationDate = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
                $('.datepicker').datepicker('update', $scope.applicationDate);
               // $('.datepicker').val($scope.applicationDate);
                $(".dateDropdownContainer").css('display', 'block');
                $("#datePickerInput").focus();
                $("#datePickerInput").select();
                $("input:text").focus(function () { $(this).select(); });

            });
           
        }
            
        else
            $(".dateDropdownContainer").css('display', 'none');
        //$scope.changeApplicationDateInternal(function () {
        //    $scope.getApplicationDate(function (data) {
        //        $scope.applicationDate = data;
        //    });
        //});
    }

    //---------------------------------------------------------------------------------------------
    $scope.changeApplicationDateInternal = function () {
       
      
        var date = $(".datepicker").datepicker('getUTCDate');
        var day = date.getDate();
        var month = date.getMonth() + 1;
        var year = date.getFullYear();
        
        var yearStr = year.toString();

        if (yearStr.length == 2) {

            if (year >= 30)
                year = 1900 + year;
            else
                year = 2000 + year;
        }
        else if (yearStr.length == 3 || yearStr.length == 1) {
            $scope.loggingService.showDiagnostic("Invalid date");
            $scope.setDateToDefault();
            $(".dateDropdownContainer").css('display', 'block');
            return;
        }


        if (day < 10) {
            day = '0' + day
        }
        if (month < 10) {
            month = '0' + month
        }
        
        $http.post( 'changeApplicationDate/?day=' + day + '&month=' + month + '&year=' + year)
       .success(function (data, status, headers, config) {
           if (data.success) {
               $scope.applicationDate = date;
               $('.datepicker').datepicker('update', $scope.applicationDate);
       	   
       	}
       	else if (data.message) {
       	    $scope.loggingService.showDiagnostic(data.message);
       	}
       })
       .error(function (data, status, headers, config) {
           $log.warn(status);
       });

       
    }

    //---------------------------------------------------------------------------------------------
    $scope.getCulture = function (callback) {
        $http.post( 'getCulture/')
		.success(function (data, status, headers, config) {
		    callback(data);
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.getApplicationDateFormat = function (callback) {
        $http.post( 'getApplicationDateFormat/')
		.success(function (data, status, headers, config) {
		    callback(data);
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.getApplicationDate = function (callback) {
        $http.post( 'getApplicationDate/')
		.success(function (data, status, headers, config) {
		    callback(data);
		})
		.error(function (data, status, headers, config) {
		    $log.warn(status);
		});
    }

    //---------------------------------------------------------------------------------------------
    $scope.setTodayDate = function () {
        $('.datepicker').datepicker('update', new Date());

    }

    //---------------------------------------------------------------------------------------------
    $("body").click(function (event) {
        if (event.target.className != 'dateDropdownContainer' && event.target.className != 'calendar link' && $('.dateDropdownContainer').css('display') == 'block') {

            $('.dateDropdownContainer').css('display', 'none');
        }
            
    });

    //---------------------------------------------------------------------------------------------
    $scope.setDateToDefault = function () {
        $('.datepicker').datepicker('update', $scope.applicationDate);
    }

    //---------------------------------------------------------------------------------------------
    $scope.cancelPressed = function () {
        $scope.setDateToDefault();
        $('.dateDropdownContainer').css('display', 'none');
    }

    //---------------------------------------------------------------------------------------------
    $(document).keydown(function (e) {
        if (((e.which >= 48 && e.which <= 57) || (e.which >= 96 && e.which <= 105)) && $('.dateDropdownContainer').css('display') != 'none') {
            var k = $('.datepicker').val();
            if (k.length == 2 || k.length == 5)
                $('.datepicker').val(k + '/');
            
            return;
        }

        if (e.which == 13 && $('.dateDropdownContainer').css('display') != 'none') {
            if (document.activeElement.id == 'datepickerCancel')
                $scope.cancelPressed();
          
            else if (document.activeElement.id == 'btnToday')
                $scope.setTodayDate();
            else
            {
                $scope.changeApplicationDateInternal();
                $('.dateDropdownContainer').css('display', 'none');
              
            }
                                   
        }   
    });

};



