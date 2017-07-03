import { Component, Output, EventEmitter } from '@angular/core';
import { TbComponent } from '../../../../core/index';
export class MenuTabberComponent extends TbComponent {
    constructor() {
        super(...arguments);
        this.tabs = [];
        this.selectedTab = new EventEmitter(true);
    }
    /**
     * @param {?} tab
     * @return {?}
     */
    selectTab(tab) {
        this.tabs.forEach((t) => {
            t.active = false;
        });
        tab.active = true;
        this.selectedTab.emit(this.tabs.indexOf(tab));
    }
    /**
     * @param {?} tab
     * @return {?}
     */
    addTab(tab) {
        this.tabs.push(tab);
        this.selectTab(tab);
    }
    /**
     * @param {?} tab
     * @return {?}
     */
    removeTab(tab) {
        this.tabs.splice(this.tabs.indexOf(tab), 1);
        if (tab.active && this.tabs.length > 0) {
            this.tabs[0].active = true;
            this.selectedTab.emit(0);
        }
    }
}
MenuTabberComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-menu-tabber',
                template: "<ul class=\"menu-tabber\"> <li (click)=\"selectTab(tab)\" *ngFor=\"let tab of tabs\" [ngClass]=\"tab.active ? 'active' : ''\" title=\"Activate\"> <span>{{tab.title}}</span> </li> </ul>",
                styles: ["ul.menu-tabber { list-style: none; padding: 0; display: flex; flex-direction: row; flex-wrap: nowrap; margin: -1px 0 0 0; height: 30px; line-height: 30px; background-color: #fff; padding-left: 20px; border-bottom: 1px solid #ddd; } ul.menu-tabber > li { line-height: 30px; padding: 0 10px; font-size: 12px; position: relative; cursor: pointer; color: #000; font-weight: 500; } ul.menu-tabber > li.active { /* background-color: #b8b8b8;*/ border-bottom: 2px solid #0277bd; } "]
            },] },
];
/**
 * @nocollapse
 */
MenuTabberComponent.ctorParameters = () => [];
MenuTabberComponent.propDecorators = {
    'selectedTab': [{ type: Output },],
};
function MenuTabberComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MenuTabberComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MenuTabberComponent.ctorParameters;
    /** @type {?} */
    MenuTabberComponent.propDecorators;
    /** @type {?} */
    MenuTabberComponent.prototype.tabs;
    /** @type {?} */
    MenuTabberComponent.prototype.selectedTab;
}
