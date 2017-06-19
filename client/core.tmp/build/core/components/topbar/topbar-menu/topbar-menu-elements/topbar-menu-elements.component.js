import { Component, Input, ViewChild } from '@angular/core';
import { WebSocketService } from '../../../../services/websocket.service';
import { EventDataService } from '../../../../services/eventdata.service';
export class TopbarMenuElementsComponent {
    /**
     * @param {?} webSocketService
     * @param {?} eventDataService
     */
    constructor(webSocketService, eventDataService) {
        this.webSocketService = webSocketService;
        this.eventDataService = eventDataService;
        this.anchorAlign = { horizontal: 'right', vertical: 'bottom' };
        this.popupAlign = { horizontal: 'right', vertical: 'top' };
        this.collision = { horizontal: 'flip', vertical: 'fit' };
        this.anchorAlign2 = { horizontal: 'left', vertical: 'top' };
        this.popupAlign2 = { horizontal: 'right', vertical: 'top' };
        this.show = false;
        this.isMouseDown = false;
        this.fontIcon = 'more_vert';
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
            console.log('NOT doCommand for menuElements!');
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
TopbarMenuElementsComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-topbar-menu-elements',
                template: "<div id=\"anchor\" #anchor tabindex=\"0\" (blur)=\"closePopupIf()\"> <md-icon (click)=\"onToggle()\" [ngClass]=\"{ 'borderOn': show }\">{{fontIcon}}</md-icon> <kendo-popup *ngIf=\"show\" popupClass='' [anchor]=\"anchor\" (anchorViewportLeave)=\"outView(null)\" (open)=\"onOpen()\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign\" [popupAlign]=\"popupAlign\"> <button kendoButton *ngFor=\"let menuItem of menuElements\" class=\"content\" [disabled]=\"!menuItem?.enabled\" (mouseenter)=\"openSubItems(true, menuItem)\" (mouseleave)=\"openSubItems(false, menuItem)\" (mousedown)=\"setMouseDown()\" (click)=\"doCommand(menuItem)\"> <div #anchor2 class=\"divW\"> <md-icon *ngIf=\"hasSubItems(menuItem)\" class=\"subsIcon\">chevron_left</md-icon> <span [ngClass]=\"{ 'spaceOn': !hasSubItems(menuItem)}\"> {{menuItem?.text}} </span> <md-icon *ngIf=\"menuItem?.checked\" class=\"checkIcon\">check</md-icon>  </div> <kendo-popup *ngIf=\"menuItem?.showMySub\" [anchor]=\"anchor2\" (anchorViewportLeave)=\"outView(menuItem)\" [collision]=\"collision\" [anchorAlign]=\"anchorAlign2\" [popupAlign]=\"popupAlign2\" [popupClass]=\"'content popup'\"> <button kendoButton *ngFor=\"let subItem of menuItem.subItems\" class=\"content\" (click)=\"doCommand(subItem)\" [disabled]=\"!subItem?.enabled\"> <div class=\"divW\"> <md-icon class=\"checkIcon\" *ngIf=\"subItem?.checked\">check</md-icon> <span class=\"spaceOn\"> {{subItem?.text}} </span> </div> </button> </kendo-popup> </button> </kendo-popup> </div>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TopbarMenuElementsComponent.ctorParameters = () => [
    { type: WebSocketService, },
    { type: EventDataService, },
];
TopbarMenuElementsComponent.propDecorators = {
    'fontIcon': [{ type: Input },],
    'menuElements': [{ type: Input },],
    'divFocus': [{ type: ViewChild, args: ['anchor',] },],
};
function TopbarMenuElementsComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TopbarMenuElementsComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TopbarMenuElementsComponent.ctorParameters;
    /** @type {?} */
    TopbarMenuElementsComponent.propDecorators;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.anchorAlign;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.popupAlign;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.collision;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.anchorAlign2;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.popupAlign2;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.show;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.isMouseDown;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.currentItem;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.fontIcon;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.menuElements;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.divFocus;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.webSocketService;
    /** @type {?} */
    TopbarMenuElementsComponent.prototype.eventDataService;
}
