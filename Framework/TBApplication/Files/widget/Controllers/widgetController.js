
// register module controller for widget
//---------------------------------------------------------------------------------------------
function LoadWidGetController($app) {
    console.log("Load WidGet Controller");
    //$app.controller('Controller', Controller);    // controller
}

//---------------------------------------------------------------------------------------------
function WidgetController($scope, $http, $compile) {

    $scope.ShowWidgetAdd = false;
    $scope.WidgetIcoManager = "widget/ico/add.png";
    $scope.showWidgeIco = "widget/ico/Open.png";
    $scope.widgetListAvailable = new Array();
    $scope.widgetID = 0;

    var MouseEvent = 0; // Mouse id widget event

    // Load widget configuration file
    $http.get('widget/widget.json')
        .success(function (data) {
            // Load Widget list
            angular.forEach(data.widget, function (obj, i) {
                this.path = "widget/module/" + obj.path;

                // Icon, Widget Title, path, resizable, wobj
                $scope.widgetListAvailable.push([path, obj]);
            });
        })
    .error(function (data, status, headers, config) {
        //  Do some error handling here
    });

    // Show or hiden widget Manager
    $scope.showWidgeManager = function () {
        $scope.ShowWidgetAdd = !$scope.ShowWidgetAdd;
        if ($scope.ShowWidgetAdd) {
            $scope.WidgetIcoManager = "widget/ico/ok.png";
        }
        else {
            $scope.WidgetIcoManager = "widget/ico/add.png";
        }
    }
    
    $scope.GetWidgetIco = function (wIndex)
    {
        // return icons by index
        return $scope.widgetListAvailable[wIndex][0] + '/'+ $scope.widgetListAvailable[wIndex][1].ico;
    }

    // Mouse on widget over
    $scope.MouseOverWidget = function (wIndex)
    {
        MouseEvent = wIndex;
    }

    // Mouse on widget leave
    $scope.MouseLeaveWidget = function (wIndex)
    {
        MouseEvent = 0;
    }

    // Show or hiden widget command control
    $scope.ShowWidgetCommand = function (wIndex)
    {
        return MouseEvent == wIndex;
    }

    // Remove widget
    $scope.WidgetRemove = function (wIndex)
    {
        console.log("WidgetRemove: " + wIndex);
        $("#Widget-Conteiner-" + wIndex).remove();
    }

    $scope.onWidgetDrop = function (target, source) {
        // Get all HTML from <DIV Widget-Conteiner-XX>
        var nPosSorce  = $scope.GetWidgetPos(source);
        var nPosTarget = $scope.GetWidgetPos(target);
        if (nPosSorce == nPosTarget) return;

        var widgetSorce = $("#Widget-Conteiner-" + source)[0].outerHTML;
        $("#Widget-Conteiner-" + source).remove();

        if (nPosSorce > nPosTarget)
            $("#Widget-Conteiner-" + target).before(widgetSorce);
        else
            $("#Widget-Conteiner-" + target).after(widgetSorce);

        $compile($("#Widget-Conteiner-" + source))($scope);
    };

    $scope.GetWidgetPos = function (idName) {
        var objFind = $('#widgetAddZone');
        var child = objFind.children();
        var nLement = child.length;
        var n = 0;
        do {
            if (child[n].id.localeCompare("Widget-Conteiner-" + idName) == 0) {
                return n;
            }
            n++;
        }
        while (n < nLement);
        return -1;
    };

    $scope.GetWidgetIdvalue = function (idName) {
        var obj = $scope.FindIdPaent(idName);
        if (obj != null) {
            return obj.get(0).value;
        }
        return null;
    };

    $scope.SetWidgetIdvalue = function (idName, val) {
        var obj = $scope.FindIdPaent(idName);
        if (obj != null)
        {
            obj.get(0).value = val;
        }
    };

    $scope.FindIdPaent = function (idName) {
        var t = $scope.GetWidgetObj();
        if (t == null) return null;
        var objFind = t.find('#' + idName)
        return objFind;
    };

    $scope.GetWidgetObj = function () {
        var target = $(event.target);
        do {
            if (target.get(0) != null)
            {
                if (typeof target.get(0).id === "string") {
                    idName = target.get(0).id;
                    if (idName.length >= 6)
                    {
                        if (idName.substring(0, 7).localeCompare("Widget-") == 0)
                        {
                            console.log("ID: " + idName);
                            return target;
                            break;
                        }
                    }
                }
            }
            target = target.parent();
            // BODY end of document page HTML
        }
        while (target.get(0).tagName != "BODY");
        return null;
    };

    $scope.AddWidget = function (wIndex) {
        $scope.widgetID = $scope.widgetID + 1;
        var path = $scope.widgetListAvailable[wIndex][0];
       
        // Load widget CSS
        $scope._style = document.createElement('link');
        $scope._style.type = 'text/css';
        $scope._style.href = path + '/widget.css';
        $scope._style.rel = 'stylesheet';
        $scope._style = document.head.appendChild($scope._style);

        // Make and compiling HTML
        var varImgDelete = '<img class="widgetPng" ng-src="widget/ico/remove.png" ng-click="WidgetRemove(' + $scope.widgetID +')">';
        var htmlCommand = '<div class="widgetButton" ng-show="ShowWidgetCommand(' +$scope.widgetID + ')">' + varImgDelete + '</div>';

        htmlFile = path + "/widget.html";
        var htmlEventInclude = 'ng-mouseover="MouseOverWidget(' + $scope.widgetID + ')" ';
        htmlEventInclude += 'ng-mouseleave="MouseLeaveWidget(' + $scope.widgetID + ')"';
        var html = '<div id="Widget-Conteiner-' + $scope.widgetID + '" ' + htmlEventInclude + '>';
        html += htmlCommand;

        html += '<div id="Widget-' + $scope.widgetID + '" ng-include="' + "'" + htmlFile + "'" +
                '" ui-draggable="true" drag="' + $scope.widgetID + '" ui-on-drop="onWidgetDrop(' +$scope.widgetID + ', $data)"> </div>';

        html += '</div>';

        // Compiling
        $("#widgetAddZone").append(html)
        $compile($("#Widget-Conteiner-" + $scope.widgetID)) ($scope);

        // Load widget Js and Run
        $.getScript(path + "/widget.js", function (data, textStatus, jqxhr) {
            funcCall = "new RunWidget($scope)";
            rObj = eval(funcCall);
            ret = rObj.run();
            if (!ret) alert("Error in task" + path);
        });
    }
}
