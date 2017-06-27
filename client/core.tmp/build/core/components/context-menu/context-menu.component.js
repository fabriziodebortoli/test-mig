import { Component, Input, ViewChild } from '@angular/core';
import { ContextMenuItem } from '../../../shared/models/context-menu-item.model';
import { EventDataService } from '../../../core/services/eventdata.service';
import { WebSocketService } from '../../../core/services/websocket.service';
export class ContextMenuComponent {
    /**
     * @param {?} webSocketService
     * @param {?} eventDataService
     */
    constructor(webSocketService, eventDataService) {
        // SCENARIO 1: RIEMPIRE DA SERVER
        // this.webSocketService.contextMenu.subscribe((result) => {
        //   this.contextMenu = result.contextMenu;
        // });
        this.webSocketService = webSocketService;
        this.eventDataService = eventDataService;
        this.anchorAlign = { horizontal: 'left', vertical: 'bottom' };
        this.popupAlign = { horizontal: 'right', vertical: 'top' };
        this.collision = { horizontal: 'flip', vertical: 'fit' };
        this.anchorAlign2 = { horizontal: 'left', vertical: 'top' };
        this.popupAlign2 = { horizontal: 'right', vertical: 'top' };
        this.show = false;
        this.isMouseDown = false;
        this.fontIcon = 'more_vert';
        this.popupClass = 'content popup';
        // SCENARIO 2: RIEMPITO DA HTML
        this.contextMenu = new Array();
        const subItems_bis = new Array();
        const item4 = new ContextMenuItem('solo questo disable unchecked', 'Id4', false, false);
        subItems_bis.push(item4);
        const subItems = new Array();
        const item1 = new ContextMenuItem('disabled unchecked', 'Id1', false, false);
        const item5 = new ContextMenuItem('enabled checked', 'Id5', true, true);
        const item2 = new ContextMenuItem('has one sub item', 'Id2', true, false, subItems_bis);
        subItems.push(item1, item5);
        const item3 = new ContextMenuItem('has 2 sub items', 'Id3', true, false, subItems);
        this.contextMenu.push(item1, item2, item5, item3);
    }
    /**
     * @return {?}
     */
    onOpen() {
    }
    /**
     * @param {?} menuItem
     * @return {?}
     */
    doCommand(menuItem) {
        if (!menuItem) {
            console.log('NOT doCommand for ContextMenu!');
            return;
        }
        if (this.hasSubItems(menuItem)) {
            return;
        }
        this.eventDataService.command.emit(menuItem.id);
        console.log('doCommand OK!');
        this.onToggle();
    }
    /**
     * @return {?}
     */
    onToggle() {
        this.show = !this.show;
        if (!this.show && this.currentItem !== null && this.currentItem !== undefined) {
            this.currentItem.showMySub = false;
        }
    }
    /**
     * @return {?}
     */
    closePopupIf() {
        if (this.isMouseDown) {
            this.isMouseDown = false;
            document.getElementById('anchor').focus();
            return;
        }
        this.outView(this.currentItem);
    }
    /**
     * @return {?}
     */
    setMouseDown() {
        this.isMouseDown = true;
    }
    /**
     * @param {?} item
     * @return {?}
     */
    hasSubItems(item) {
        const /** @type {?} */ y = item.subItems;
        return y !== null && y.length > 0;
    }
    /**
     * @param {?} open
     * @param {?} item
     * @return {?}
     */
    openSubItems(open, item) {
        if (!this.hasSubItems(item) || item === null || item === undefined) {
            return;
        }
        item.showMySub = open;
        this.currentItem = item;
    }
    /**
     * @param {?} item
     * @return {?}
     */
    outView(item) {
        if (item !== null && item !== undefined) {
            item.showMySub = false;
        }
        this.show = false;
        this.currentItem = null;
        this.isMouseDown = false;
    }
}
ContextMenuComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-context-menu',
                template: "<div id=\"anchor\" #anchor tabindex=\"0\" (blur)=\"closePopupIf()\"> <md-icon (click)=\"onToggle()\" [ngClass]=\"{ 'borderOn': show }\">{{fontIcon}}</md-icon> <kendo-popup *ngIf=\"show\" popupClass='{{popupClass}}' [anchor]=\"anchor\" (anchorViewportLeave)=\"outView(null)\" (open)=\"onOpen()\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign\" [popupAlign]=\"popupAlign\"> <button kendoButton *ngFor=\"let menuItem of contextMenu\" class=\"content\" [disabled]=\"!menuItem?.enabled\" (mouseenter)=\"openSubItems(true, menuItem)\" (mouseleave)=\"openSubItems(false, menuItem)\" (mousedown)=\"setMouseDown()\" (click)=\"doCommand(menuItem)\"> <div #anchor2 class=\"divW\"> <md-icon *ngIf=\"hasSubItems(menuItem)\" class=\"subsIcon\">chevron_left</md-icon> <span [ngClass]=\"{ 'spaceOn': !hasSubItems(menuItem)}\"> {{menuItem?.text}} </span> <md-icon *ngIf=\"menuItem?.checked\" class=\"checkIcon\">check</md-icon>  </div> <kendo-popup *ngIf=\"menuItem?.showMySub\" [anchor]=\"anchor2\" (anchorViewportLeave)=\"outView(menuItem)\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign2\" [popupAlign]=\"popupAlign2\" [popupClass]=\"'content popup'\"> <button kendoButton *ngFor=\"let subItem of menuItem.subItems\" class=\"content\" (click)=\"doCommand(subItem)\" [disabled]=\"!subItem?.enabled\"> <div class=\"divW\"> <md-icon class=\"checkIcon\" *ngIf=\"subItem?.checked\">check</md-icon> <span class=\"spaceOn\"> {{subItem?.text}} </span> </div> </button> </kendo-popup> </button> </kendo-popup> </div>",
                styles: [".contextMenuClass { width: 5%; margin-right: 0px; margin-left: auto; padding-top: 5px; outline: none; } .material-icons { font-size: 1.3rem; cursor: pointer; flex-direction: row; } .cm-content { display: flex; flex-direction: column; flex-wrap: wrap; width: 200px; height: 24px; } .divW { width: inherit; flex-direction: row; flex-wrap: wrap; text-align: left; font-size: smaller; } .checkIcon { float: right; } .subsIcon { float: left; } .borderOn { border: 1px solid blue; } .spaceOn { padding-left: 24px; } span { vertical-align: sub; } "]
            },] },
];
/**
 * @nocollapse
 */
ContextMenuComponent.ctorParameters = () => [
    { type: WebSocketService, },
    { type: EventDataService, },
];
ContextMenuComponent.propDecorators = {
    'fontIcon': [{ type: Input },],
    'contextMenu': [{ type: Input },],
    'popupClass': [{ type: Input },],
    'divFocus': [{ type: ViewChild, args: ['anchor',] },],
};
function ContextMenuComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ContextMenuComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ContextMenuComponent.ctorParameters;
    /** @type {?} */
    ContextMenuComponent.propDecorators;
    /** @type {?} */
    ContextMenuComponent.prototype.anchorAlign;
    /** @type {?} */
    ContextMenuComponent.prototype.popupAlign;
    /** @type {?} */
    ContextMenuComponent.prototype.collision;
    /** @type {?} */
    ContextMenuComponent.prototype.anchorAlign2;
    /** @type {?} */
    ContextMenuComponent.prototype.popupAlign2;
    /** @type {?} */
    ContextMenuComponent.prototype.show;
    /** @type {?} */
    ContextMenuComponent.prototype.isMouseDown;
    /** @type {?} */
    ContextMenuComponent.prototype.contextMenuBinding;
    /** @type {?} */
    ContextMenuComponent.prototype.currentItem;
    /** @type {?} */
    ContextMenuComponent.prototype.fontIcon;
    /** @type {?} */
    ContextMenuComponent.prototype.contextMenu;
    /** @type {?} */
    ContextMenuComponent.prototype.popupClass;
    /** @type {?} */
    ContextMenuComponent.prototype.divFocus;
    /** @type {?} */
    ContextMenuComponent.prototype.webSocketService;
    /** @type {?} */
    ContextMenuComponent.prototype.eventDataService;
}
