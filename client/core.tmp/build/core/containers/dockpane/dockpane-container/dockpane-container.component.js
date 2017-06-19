import { Component } from '@angular/core';
export class DockpaneContainerComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
DockpaneContainerComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-dockpane-container',
                template: "<ng-content></ng-content>",
                styles: [":host(tb-dockpane-container) { display: none; } "]
            },] },
];
/**
 * @nocollapse
 */
DockpaneContainerComponent.ctorParameters = () => [];
function DockpaneContainerComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    DockpaneContainerComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    DockpaneContainerComponent.ctorParameters;
}
