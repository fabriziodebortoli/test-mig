import { Component, ViewEncapsulation } from '@angular/core';
export class TopbarMenuComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
TopbarMenuComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-topbar-menu',
                template: "<div class=\"tbcontainer\"> <tb-topbar-menu-test class=\"topbar-menu-layout\"></tb-topbar-menu-test> <tb-topbar-menu-user class=\"topbar-menu-layout\"></tb-topbar-menu-user> <tb-topbar-menu-app class=\"topbar-menu-layout\"></tb-topbar-menu-app> </div>",
                styles: [".tbcontainer { display: -webkit-box; display: -moz-box; display: -ms-flexbox; display: -webkit-flex; display: flex; flex-direction: row; flex-wrap: nowrap; align-items: baseline; justify-content: flex-end; } .topbar-menu-layout { padding: 10px; } .tbcontainer .topbar-menu-layout .k-button { color: white; background-color: #00578c; background-image: none; border-color: transparent; border: 0; border-radius: 0px; } .tbcontainer .topbar-menu-layout .topbar-menu-button { background-color: transparent; border-width: 0px; border-style: none; border-color: transparent; border-image: none; padding-top: 5px; margin: 0px; } .tbcontainer .topbar-menu-layout .k-button:hover { color: #0277BD; background-color: white; background-image: none; border-color: #00578c; border: 0; border-left: 2px; border-right: 2px; border-radius: 0px; } .tbcontainer .topbar-menu-layout .topbar-menu-button:focus, .tbcontainer .topbar-menu-layout .topbar-menu-button:active { background-color: #00578c; background-image: none; border-color: #00578c; border: 0; border-radius: 0px; } .tbcontainer .topbar-menu-layout.k-list .tbcontainer .topbar-menu-layout.k-item:focus, .tbcontainer .topbar-menu-layout.k-list .tbcontainer .topbar-menu-layout.k-item.k-state-focused { box-shadow: inset 0 0 0 0 transparent; } .tbcontainer .topbar-menu-layout .material-icons { color: white; background-color: transparent; border-color: transparent; cursor: pointer; } .tbcontainer .topbar-menu-layout .material-icons:focus { color: #bcd3e1; background-color: transparent; border-color: transparent; } .tbcontainer .topbar-menu-layout .content { display: flex; flex-direction: column; flex-wrap: wrap; width: 200px; height: 24px; } "],
                encapsulation: ViewEncapsulation.None
            },] },
];
/**
 * @nocollapse
 */
TopbarMenuComponent.ctorParameters = () => [];
function TopbarMenuComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TopbarMenuComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TopbarMenuComponent.ctorParameters;
}