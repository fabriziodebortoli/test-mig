import { Component, ContentChildren, ViewEncapsulation } from '@angular/core';
import { LayoutService } from './../../../services/layout.service';
import { TileGroupComponent } from './../tile-group/tile-group.component';
export class TileManagerComponent {
    /**
     * @param {?} layoutService
     */
    constructor(layoutService) {
        this.layoutService = layoutService;
    }
    /**
     * @return {?}
     */
    getTiles() {
        return this.tiles.toArray();
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
    /**
     * @return {?}
     */
    ngAfterContentInit() {
        // get all active tiles
        let /** @type {?} */ activeTiles = this.tiles.filter((tile) => tile.active);
        //if there is no active tab set, activate the first
        if (activeTiles.length === 0 && this.tiles.toArray().length > 0) {
            this.selectTile(this.tiles.first);
        }
    }
    /**
     * @param {?} tile
     * @return {?}
     */
    selectTile(tile) {
        if (tile.active)
            return;
        // deactivate all tabs
        this.tiles.toArray().forEach(tile => tile.active = false);
        // activate the tab the user has clicked on.
        tile.active = true;
    }
    /**
     * @param {?} event
     * @return {?}
     */
    changeTabByIndex(event) {
        let /** @type {?} */ index = event.index;
        let /** @type {?} */ currentTile = this.tiles.toArray()[index];
        this.selectTile(currentTile);
    }
}
TileManagerComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-tilemanager',
                template: "<div class=\"tile-manager\"> <kendo-tabstrip #tabber (tabSelect)=\"changeTabByIndex($event)\"> <kendo-tabstrip-tab *ngFor=\"let tile of getTiles();let i = index\" [selected]=\"i == 0\"> <ng-template kendoTabTitle> <tb-icon [iconType]=\"tile?.iconType\" [icon]=\"tile?.icon\"></tb-icon> <span>{{tile?.title}}</span> </ng-template> <template kendoTabContent> <div class='kendoTabContent' [style.height.px]=\"viewHeight\"> <ng-content></ng-content> </div> </template> </kendo-tabstrip-tab> </kendo-tabstrip> </div>",
                styles: ["tb-tilemanager { flex: 1; max-width: 100%; } .tile-manager { flex: 1; display: flex; min-height: 100%; } .tile-manager .k-tabstrip { flex-direction: row; display: flex; flex: 1; max-width: 100%; } .tile-manager .k-tabstrip-top > .k-content { flex: 1; background-color: transparent; } .tile-manager .k-tabstrip > .k-tabstrip-items { flex-direction: column; flex: 0 0 60px; background: #fff; } @media screen and (min-width: 48em) { .tile-manager .k-tabstrip > .k-tabstrip-items { flex: 0 0 160px; } } @media screen and (min-width: 64em) { .tile-manager .k-tabstrip > .k-tabstrip-items { flex: 0 0 200px; } } @media screen and (min-width: 75em) { .tile-manager .k-tabstrip > .k-tabstrip-items { flex: 0 0 260px; } } .tile-manager .k-item .k-link { display: flex; flex-direction: row; flex: 1; align-items: center; } .tile-manager .k-item .k-link img { margin-right: 5px; } .tile-manager .k-item .k-link span { font-size: 12px; color: #000; } .tile-manager .k-item.k-state-active { border: none; background: #f1f4f7; } "],
                encapsulation: ViewEncapsulation.None
            },] },
];
/**
 * @nocollapse
 */
TileManagerComponent.ctorParameters = () => [
    { type: LayoutService, },
];
TileManagerComponent.propDecorators = {
    'tiles': [{ type: ContentChildren, args: [TileGroupComponent,] },],
};
function TileManagerComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TileManagerComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TileManagerComponent.ctorParameters;
    /** @type {?} */
    TileManagerComponent.propDecorators;
    /** @type {?} */
    TileManagerComponent.prototype.tiles;
    /** @type {?} */
    TileManagerComponent.prototype.viewHeightSubscription;
    /** @type {?} */
    TileManagerComponent.prototype.viewHeight;
    /** @type {?} */
    TileManagerComponent.prototype.layoutService;
}
