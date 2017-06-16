import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
export class LayoutService {
    constructor() {
        this.viewHeight = new BehaviorSubject(0);
    }
    /**
     * @param {?} viewHeight
     * @return {?}
     */
    setViewHeight(viewHeight) {
        this.viewHeight.next(viewHeight);
    }
    /**
     * @return {?}
     */
    getViewHeight() {
        return this.viewHeight;
    }
}
LayoutService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
LayoutService.ctorParameters = () => [];
function LayoutService_tsickle_Closure_declarations() {
    /** @type {?} */
    LayoutService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LayoutService.ctorParameters;
    /** @type {?} */
    LayoutService.prototype.viewHeight;
}
