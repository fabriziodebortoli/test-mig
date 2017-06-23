import { Component, Input } from '@angular/core';
export class TileGroupComponent {
    constructor() {
        this.iconType = 'M4';
        this.icon = 'erp-purchaseorder';
    }
    /**
     * @return {?}
     */
    ngOnInit() { }
}
TileGroupComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-tilegroup',
                template: "<div class=\"tile-group\" *ngIf=\"active\"> <ng-content></ng-content> </div>",
                styles: [".tile-group { flex-direction: row; flex-wrap: wrap; justify-content: flex-start; padding-bottom: 100px; } "]
            },] },
];
/**
 * @nocollapse
 */
TileGroupComponent.ctorParameters = () => [];
TileGroupComponent.propDecorators = {
    'title': [{ type: Input },],
    'iconType': [{ type: Input },],
    'icon': [{ type: Input },],
};
function TileGroupComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TileGroupComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileGroupComponent.ctorParameters;
    /** @type {?} */
    TileGroupComponent.propDecorators;
    /** @type {?} */
    TileGroupComponent.prototype.active;
    /** @type {?} */
    TileGroupComponent.prototype.title;
    /** @type {?} */
    TileGroupComponent.prototype.iconType;
    /** @type {?} */
    TileGroupComponent.prototype.icon;
}
