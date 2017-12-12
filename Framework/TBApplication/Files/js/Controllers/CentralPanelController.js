function CentralPanelController($scope) {

    $(window).resize(function () {
        $scope.setCentralPanelHeight();
    });
    
    $scope.setCentralPanelHeight = function () {
        var height = $(document).outerHeight() - 70 - 47 - 27;
        $('#modal1-content').css("height", height);
    }
};
