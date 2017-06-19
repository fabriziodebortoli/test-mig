import { Component, Input } from '@angular/core';
export class TilePanelComponent {
    constructor() {
        this._showAsTile = true;
        this._isCollapsed = false;
        this._isCollapsible = true;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
    /**
     * @param {?} value
     * @return {?}
     */
    set showAsTile(value) {
        this._showAsTile = value;
    }
    /**
     * @return {?}
     */
    get showAsTile() {
        return this._showAsTile;
    }
    /**
     * @param {?} event
     * @return {?}
     */
    toggleCollapse(event) {
        if (!this._isCollapsible)
            return;
        // event.preventDefault();
        this._isCollapsed = !this._isCollapsed;
    }
    /**
     * @return {?}
     */
    getArrowIcon() {
        return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
    }
}
TilePanelComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-tilepanel',
                template: "<div class=\"tile-panel\"> <md-card-title (click)=\"toggleCollapse($event)\" *ngIf=\"showAsTile\" [ngClass]=\"{'c-pointer': isCollapsible }\"> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </md-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TilePanelComponent.ctorParameters = () => [];
TilePanelComponent.propDecorators = {
    'showAsTile': [{ type: Input },],
};
function TilePanelComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TilePanelComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TilePanelComponent.ctorParameters;
    /** @type {?} */
    TilePanelComponent.propDecorators;
    /** @type {?} */
    TilePanelComponent.prototype._showAsTile;
    /** @type {?} */
    TilePanelComponent.prototype._isCollapsed;
    /** @type {?} */
    TilePanelComponent.prototype._isCollapsible;
    /** @type {?} */
    TilePanelComponent.prototype.tilePanel;
}
