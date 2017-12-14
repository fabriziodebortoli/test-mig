function NotificationController($scope,  localizationService, loggingService) {

    $scope.loggingService = loggingService;
    $scope.localizationService = localizationService;
    $scope.messages = ["Hello World", "There is a message for you"];


    $(".closeAlert").click(function () {
        $("#alertMessage").html("");
        $(".alertContainer").css("display", "none");
    });

    $(".closeBalloon").click(function () {
        $("#balloonMessage").html("");
        $(".balloonContainer").css("display", "none");
    });

    //---------------------------------------------------------------------------------------------
    $scope.getMessages = function () {
        $scope.getNewMessages(function (data) {
            $scope.messages = data.Notifications;
        })
    };

    //---------------------------------------------------------------------------------------------
    $scope.initBalloon = function () {
        window.setInterval(function () {
          
            var display = $(".balloonContainer").css("display");    
            if (display == 'block')
                return;

            //$scope.getMessages();
            if ($scope.messages.length == 0)
                return;
           
            for (var i = 0; i < $scope.messages.length; i++)
                $("#balloonMessage").append("<p>" + $scope.messages[i] + "<p>");

            $(".balloonContainer").css("display", "block");


        }, 600000);
    }


    //---------------------------------------------------------------------------------------------
    $scope.getNewMessages = function (callback) {

        var urlToRun =  'getNewMessages/';
        $http.post(urlToRun)
			.success(function (data, status, headers, config) {
			    callback(data);
			})
			.error(function (data, status, headers, config) {
			    alert('error getNewMessages ' + status)
			});
    }
  
}