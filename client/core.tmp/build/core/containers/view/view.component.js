import { Component } from '@angular/core';
export class ViewComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
ViewComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-view',
                template: "<ng-content></ng-content>",
                styles: [":host(tb-view) { display: flex; flex: 1; max-width: 100%; } "]
            },] },
];
/**
 * @nocollapse
 */
ViewComponent.ctorParameters = () => [];
function ViewComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ViewComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ViewComponent.ctorParameters;
}
