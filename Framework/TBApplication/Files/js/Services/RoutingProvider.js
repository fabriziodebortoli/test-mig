var routingProvider = function ($routeProvider) {
    return $routeProvider
    .when('/FullMenu', {
        controller: 'MenuController',
        templateUrl: 'templates/OldMenu/fullMenu.html'
    })
	.when('/SingleGroup', {
	    controller: 'ListViewMenuTemplateController',
	    templateUrl: 'templates/OldMenu/listViewMenuTemplate.html'
	})
	.when('/EnvironmentMenu', {
	    controller: 'EnvironmentMenuController',
	    templateUrl: 'templates/OldMenu/fullMenu.html'
	})
    .when('/MenuTemplate', {
        controller: 'NewMenuTemplateController',
        templateUrl: 'templates/newMenuTemplate.html'
    })
   
    .otherwise({ redirectTo: '/' });
};
