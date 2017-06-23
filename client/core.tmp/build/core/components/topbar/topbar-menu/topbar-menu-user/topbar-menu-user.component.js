import { Component } from '@angular/core';
import { ContextMenuItem } from '../../../../../shared/index';
import { LoginSessionService } from '../../../../services/login-session.service';
import { EventDataService } from '../../../../services/eventdata.service';
export class TopbarMenuUserComponent {
    /**
     * @param {?} loginSessionService
     * @param {?} eventDataService
     */
    constructor(loginSessionService, eventDataService) {
        this.loginSessionService = loginSessionService;
        this.eventDataService = eventDataService;
        this.menuElements = new Array();
        const item1 = new ContextMenuItem('Refresh', 'idRefreshButton', true, false);
        const item2 = new ContextMenuItem('Settings', 'idSettingsButton', true, false);
        const item3 = new ContextMenuItem('Help', 'idHelpButton', true, false);
        const item4 = new ContextMenuItem('Sign Out', 'idSignOutButton', true, false);
        this.menuElements.push(item1, item2, item3, item4);
        this.commandSubscription = this.eventDataService.command.subscribe((cmpId) => {
            switch (cmpId) {
                case 'idSignOutButton':
                    return this.logout();
                default:
                    break;
            }
        });
    }
    /**
     * @return {?}
     */
    logout() {
        this.loginSessionService.logout();
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.commandSubscription.unsubscribe();
    }
}
TopbarMenuUserComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-topbar-menu-user',
                template: "<tb-topbar-menu-elements [menuElements]=\"menuElements\" [fontIcon]=\"'person'\" popupClass='popup'></tb-topbar-menu-elements>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TopbarMenuUserComponent.ctorParameters = () => [
    { type: LoginSessionService, },
    { type: EventDataService, },
];
function TopbarMenuUserComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TopbarMenuUserComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TopbarMenuUserComponent.ctorParameters;
    /** @type {?} */
    TopbarMenuUserComponent.prototype.menuElements;
    /** @type {?} */
    TopbarMenuUserComponent.prototype.commandSubscription;
    /** @type {?} */
    TopbarMenuUserComponent.prototype.loginSessionService;
    /** @type {?} */
    TopbarMenuUserComponent.prototype.eventDataService;
}
