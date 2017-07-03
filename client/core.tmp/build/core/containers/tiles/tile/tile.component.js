import { Component, Input, ViewEncapsulation } from '@angular/core';
export class TileComponent {
    constructor() {
        this._isCollapsed = false;
        this._isCollapsible = true;
        this._hasTitle = true;
    }
    /**
     * @return {?}
     */
    ngOnInit() { }
    /**
     * @return {?}
     */
    getArrowIcon() {
        return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
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
    set hasTitle(value) {
        this._hasTitle = value;
    }
    /**
     * @return {?}
     */
    get hasTitle() {
        return this._hasTitle;
    }
}
TileComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-tile',
                template: "<!--<md-card [ngClass]=\"{'sameHeight': !isCollapsed}\"> <md-card-title (click)=\"toggleCollapse($event)\" *ngIf=\"hasTitle\" [ngClass]=\"{'c-pointer': isCollapsible }\"> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </md-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </md-card>--> <div [ngClass]=\"{'sameHeight': !isCollapsed}\" class=\"tbcard\"> <tb-card-title [ngClass]=\"{'c-pointer': isCollapsible }\" [isCollapsible]=\"isCollapsible\" [isCollapsed]=\"isCollapsed\" (click)=\"toggleCollapse($event)\" *ngIf=\"hasTitle\" [title]=\"title\"></tb-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </div>",
                styles: [".h200 md-card-content { min-height: 200px; max-height: 200px; overflow: auto; } tb-tile { flex: 1; } tb-tile.tile-micro { flex: 0 0 12.5%; } tb-tile.tile-mini { flex: 0 0 25%; } tb-tile.tile-standard { flex: 0 0 50%; } tb-tile.tile-wide { flex: 0 0 100%; } tb-tile.tile-autofill { flex: 1; } tb-tile.tile-standard md-card.sameHeight { height: 100%; } tb-tile.tile-standard div.sameHeight { height: 100%; } tb-tile.tile-wide md-card-content { display: flex; flex-direction: column; } tb-tile.tile-wide .col { flex: 0 0 100%; } tb-tile.tile-wide .col2 { margin-left: 16px; } .anchored { display: flex; flex-direction: column; } tb-tile > div { background: white; margin: 5px 5px 0px 5px; padding: 5px; } tb-tile > div tb-card-title { position: relative; font-size: 1.2rem; font-weight: 500; color: #000; font-size: 1rem; text-transform: uppercase; } tb-tile > div tb-card-title div { display: flex; flex-wrap: nowrap; flex-basis: row; } tb-tile > div tb-card-title div span { flex: 2 0 0%; align-self: flex-start; } tb-tile > div tb-card-title div md-icon { align-self: flex-end; } tb-tile > div md-card-content { border-top: 0px; display: flex; flex-wrap: nowrap; flex-basis: row; } @media screen and (min-width: 48em) { tb-tile.tile-wide md-card-content { flex-direction: row; } tb-tile.tile-wide .col { flex: 0 0 100%; } } @media screen and (min-width: 75em) { tb-tile.tile-wide md-card-content { flex-direction: row; } tb-tile.tile-wide .col { flex: 0 0 50%; } .anchored { flex-direction: row; } } "],
                encapsulation: ViewEncapsulation.None
            },] },
];
/**
 * @nocollapse
 */
TileComponent.ctorParameters = () => [];
TileComponent.propDecorators = {
    'title': [{ type: Input, args: ['title',] },],
    'isCollapsed': [{ type: Input },],
    'isCollapsible': [{ type: Input },],
    'hasTitle': [{ type: Input },],
};
function TileComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TileComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileComponent.ctorParameters;
    /** @type {?} */
    TileComponent.propDecorators;
    /** @type {?} */
    TileComponent.prototype.title;
    /** @type {?} */
    TileComponent.prototype._isCollapsed;
    /** @type {?} */
    TileComponent.prototype._isCollapsible;
    /** @type {?} */
    TileComponent.prototype._hasTitle;
}
