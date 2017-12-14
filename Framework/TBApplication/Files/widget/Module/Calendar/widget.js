
// WidGet canvas
function RunWidget(scope) {
    console.log("Run widget if: " + scope.widgetID);

    function setData() {
        var days_full = [
                       'Sunday',
                       'Monday',
                       'Tuesday',
                       'Wednesday',
                       'Thursday',
                       'Friday',
                       'Saturday'
        ];

        var data = new Date();
        var day = data.getDate();
        var month = data.getMonth() + 1;

        day = day < 10 ? '0' + day : day;
        month = month < 10 ? '0' + month : month;
        scope.widgetData = days_full[data.getDay()] + " " + day + "/" + month + "/" + data.getFullYear();
    }

    this.run = function () {

        setData();
        setInterval(
           function () {
               if (!scope.$root.$$phase) {
                   scope.$apply(function () {
                       setData();
                   });
               } else {
                   setData();
               }
        }, 1000 * 60 * 60); // update 1 houre

        return true;
    }
}