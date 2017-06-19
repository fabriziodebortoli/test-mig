import { Component } from '@angular/core';
import { ContextMenuItem } from '../../../../../shared/index';
import { ComponentService } from '../../../../services/component.service';
import { EventDataService } from '../../../../services/eventdata.service';
export class TopbarMenuTestComponent {
    /**
     * @param {?} componentService
     * @param {?} eventDataService
     */
    constructor(componentService, eventDataService) {
        this.componentService = componentService;
        this.eventDataService = eventDataService;
        this.menuElements = new Array();
        const item1 = new ContextMenuItem('Data Service', 'idDataServiceButton', true, false);
        const item2 = new ContextMenuItem('Reporting Studio', 'idReportingStudioButton', true, false);
        const item3 = new ContextMenuItem('TB Explorer', 'idTBExplorerButton', true, false);
        const item4 = new ContextMenuItem('Test Grid Component', 'idTBExplorerButton', true, false);
        const item5 = new ContextMenuItem('Test Icons', 'idTestIconsButton', true, false);
        this.menuElements.push(item1, item2, item3, item4, item5);
        this.eventDataService.command.subscribe((cmpId) => {
            switch (cmpId) {
                case 'idDataServiceButton':
                    return this.openDataService();
                case 'idReportingStudioButton':
                    return this.openRS();
                case 'idTBExplorerButton':
                    return this.openTBExplorer();
                case 'idTBExplorerButton':
                    return this.openTestGrid();
                case 'idTestIconsButton':
                    return this.openTestIcons();
                default:
                    break;
            }
        });
    }
    /**
     * @return {?}
     */
    openDataService() {
        this.componentService.createComponentFromUrl('test/dataservice', true);
    }
    /**
     * @return {?}
     */
    openRS() {
        this.componentService.createReportComponent('', true);
    }
    /**
     * @return {?}
     */
    openTBExplorer() {
        this.componentService.createComponentFromUrl('test/explorer', true);
    }
    /**
     * @return {?}
     */
    openTestGrid() {
        this.componentService.createComponentFromUrl('test/grid', true);
    }
    /**
     * @return {?}
     */
    openTestIcons() {
        this.componentService.createComponentFromUrl('test/icons', true);
    }
}
TopbarMenuTestComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-topbar-menu-test',
                template: "<tb-topbar-menu-elements [menuElements]=\"menuElements\" [fontIcon]=\"'build'\" popupClass='popup'></tb-topbar-menu-elements>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
TopbarMenuTestComponent.ctorParameters = () => [
    { type: ComponentService, },
    { type: EventDataService, },
];
function TopbarMenuTestComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TopbarMenuTestComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TopbarMenuTestComponent.ctorParameters;
    /** @type {?} */
    TopbarMenuTestComponent.prototype.menuElements;
    /** @type {?} */
    TopbarMenuTestComponent.prototype.componentService;
    /** @type {?} */
    TopbarMenuTestComponent.prototype.eventDataService;
}
