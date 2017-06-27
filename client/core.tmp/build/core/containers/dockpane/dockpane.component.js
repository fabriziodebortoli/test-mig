import { Component } from '@angular/core';
export class DockpaneComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
DockpaneComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-dockpane',
                template: "<div class=\"tb-dockpane\"> <ng-content></ng-content> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
DockpaneComponent.ctorParameters = () => [];
function DockpaneComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    DockpaneComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DockpaneComponent.ctorParameters;
}
