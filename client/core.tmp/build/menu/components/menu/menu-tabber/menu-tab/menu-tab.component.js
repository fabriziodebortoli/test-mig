import { Component, Input } from '@angular/core';
import { MenuTabberComponent } from '../menu-tabber.component';
export class MenuTabComponent {
    /**
     * @param {?} tabs
     */
    constructor(tabs) {
        this.tabs = tabs;
        this.title = '...';
        tabs.addTab(this);
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.tabs.removeTab(this);
    }
}
MenuTabComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-menu-tab',
                template: '',
            },] },
];
/**
 * @nocollapse
 */
MenuTabComponent.ctorParameters = () => [
    { type: MenuTabberComponent, },
];
MenuTabComponent.propDecorators = {
    'title': [{ type: Input },],
};
function MenuTabComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MenuTabComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MenuTabComponent.ctorParameters;
    /** @type {?} */
    MenuTabComponent.propDecorators;
    /** @type {?} */
    MenuTabComponent.prototype.active;
    /** @type {?} */
    MenuTabComponent.prototype.title;
    /** @type {?} */
    MenuTabComponent.prototype.tabs;
}
