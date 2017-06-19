import { Component, Input } from '@angular/core';
export class TbCardTitleComponent {
    constructor() {
        this._isCollapsible = true;
        this._isCollapsed = false;
    }
    /**
     * @param {?} value
     * @return {?}
     */
    set isCollapsible(value) {
        this._isCollapsible = value;
    }
    /**
     * @return {?}
     */
    get isCollapsible() {
        return this._isCollapsible;
    }
    /**
     * @param {?} value
     * @return {?}
     */
    set isCollapsed(value) {
        this._isCollapsed = value;
    }
    /**
     * @return {?}
     */
    get isCollapsed() {
        return this._isCollapsed;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
    /**
     * @return {?}
     */
    getArrowIcon() {
        return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
    }
}
TbCardTitleComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-card-title',
                template: "<div> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TbCardTitleComponent.ctorParameters = () => [];
TbCardTitleComponent.propDecorators = {
    'title': [{ type: Input, args: ['title',] },],
    'isCollapsible': [{ type: Input },],
    'isCollapsed': [{ type: Input },],
};
function TbCardTitleComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TbCardTitleComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TbCardTitleComponent.ctorParameters;
    /** @type {?} */
    TbCardTitleComponent.propDecorators;
    /** @type {?} */
    TbCardTitleComponent.prototype._isCollapsible;
    /** @type {?} */
    TbCardTitleComponent.prototype._isCollapsed;
    /** @type {?} */
    TbCardTitleComponent.prototype.title;
}
