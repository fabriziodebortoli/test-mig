
// WidGet canvas
function RunWidget(scope) {
    var widgetId = scope.widgetID;

    scope.CalculatorOp = function (val)
    {
        scope.SetWidgetIdvalue("DisplayCalculator", val);
    }
    
    scope.CalculatorAdd = function (val)
    {
        var valOld = scope.GetWidgetIdvalue("DisplayCalculator");
        scope.SetWidgetIdvalue("DisplayCalculator", valOld + val);
    }

    scope.OnCalculator = function () {
        try {
            scope.CalculatorOp(eval(scope.GetWidgetIdvalue("DisplayCalculator")));
        }
        catch (e) {
            scope.CalculatorOp("Error");
        }
        
    }

    this.run = function () {
        return true;
    }
}