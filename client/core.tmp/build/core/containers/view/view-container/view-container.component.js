import { Component } from '@angular/core';
export class ViewContainerComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
ViewContainerComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-view-container',
                template: "<ng-content></ng-content>",
                styles: [":host(tb-view-container) { display: flex; flex: 1; max-width: 100%; } "]
            },] },
];
/**
 * @nocollapse
 */
ViewContainerComponent.ctorParameters = () => [];
function ViewContainerComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ViewContainerComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ViewContainerComponent.ctorParameters;
}
