
// WidGet canvas
function RunWidget(scope) {
    console.log("Run widget if: " + scope.widgetID);

    function setTime() {
        var data = new Date();
        var seconds = data.getSeconds();
        var hours = data.getHours();
        var mins = data.getMinutes();
        // second
        var degree = seconds * 6;
        scope.ClockSec = { '-webkit-transform': 'rotate(' + degree + 'deg)' };
        
        // hour
        degree = hours * 30 + (mins / 2);
        scope.ClockHour = { '-webkit-transform': 'rotate(' + degree + 'deg)' };

        // mins
        degree = mins * 6;
        scope.ClockMin = { '-webkit-transform': 'rotate(' + degree + 'deg)' };
    }

    this.run = function () {   
        //setTime();

        setInterval(
        function ()
        {
            if (!scope.$root.$$phase) {
                scope.$apply(function () {
                    setTime();
                });
            } else {
                setTime();
            }    
        }, 1000);

        return true;
    }
}