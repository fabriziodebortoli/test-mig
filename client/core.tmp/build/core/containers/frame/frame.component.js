import { Component, HostBinding } from '@angular/core';
import { LayoutService } from './../../services/layout.service';
export class FrameComponent {
    /**
     * @param {?} layoutService
     */
    constructor(layoutService) {
        this.layoutService = layoutService;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.viewHeightSubscription.unsubscribe();
    }
}
FrameComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-frame',
                template: "<div [style.height.px]=\"viewHeight\" class=\"tb-frame\"> <ng-content></ng-content> </div>",
                styles: [".tb-frame { display: flex; flex-direction: column; } "]
            },] },
];
/**
 * @nocollapse
 */
FrameComponent.ctorParameters = () => [
    { type: LayoutService, },
];
FrameComponent.propDecorators = {
    'viewHeight': [{ type: HostBinding, args: ['style.height',] },],
};
function FrameComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    FrameComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    FrameComponent.ctorParameters;
    /** @type {?} */
    FrameComponent.propDecorators;
    /** @type {?} */
    FrameComponent.prototype.viewHeightSubscription;
    /** @type {?} */
    FrameComponent.prototype.viewHeight;
    /** @type {?} */
    FrameComponent.prototype.layoutService;
}
