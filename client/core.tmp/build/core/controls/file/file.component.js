import { Component } from '@angular/core';
import { ControlComponent } from './../control.component';
export class FileComponent extends ControlComponent {
    constructor() { super(); }
}
FileComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-file',
                template: "<div class=\"tb-control tb-file\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"file\"> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
FileComponent.ctorParameters = () => [];
function FileComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    FileComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    FileComponent.ctorParameters;
}
