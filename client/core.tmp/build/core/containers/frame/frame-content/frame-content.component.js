import { Component } from '@angular/core';
export class FrameContentComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
FrameContentComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-frame-content',
                template: "<ng-content></ng-content>",
                styles: [":host(tb-frame-content) { flex: 1 1 auto; overflow: hidden; display: flex; flex-direction: column; } :host(tb-frame-content).scroll { overflow: scroll; } "]
            },] },
];
/**
 * @nocollapse
 */
FrameContentComponent.ctorParameters = () => [];
function FrameContentComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    FrameContentComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    FrameContentComponent.ctorParameters;
}
