import { Component } from '@angular/core';
import { ViewModeType } from '../../../shared/models/view-mode-type.model';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { EventManagerService } from './../../services/event-manager.service';
import { SettingsService } from './../../services/settings.service';
import { LocalizationService } from './../../services/localization.service';
import { MenuService } from './../../services/menu.service';
import { HttpMenuService } from './../../services/http-menu.service';
export class MenuComponent {
    /**
     * @param {?} httpMenuService
     * @param {?} menuService
     * @param {?} localizationService
     * @param {?} settingsService
     * @param {?} eventManagerService
     * @param {?} eventData
     * @param {?} enumsService
     */
    constructor(httpMenuService, menuService, localizationService, settingsService, eventManagerService, eventData, enumsService) {
        this.httpMenuService = httpMenuService;
        this.menuService = menuService;
        this.localizationService = localizationService;
        this.settingsService = settingsService;
        this.eventManagerService = eventManagerService;
        this.eventData = eventData;
        this.enumsService = enumsService;
        this.subscriptions = [];
        this.subscriptions.push(this.eventManagerService.preferenceLoaded.subscribe(result => {
            this.menuService.initApplicationAndGroup(this.menuService.applicationMenu.Application); //qui bisogna differenziare le app da caricare, potrebbero essere app o environment
        }));
        this.eventData.model = {
            Title: {
                value: 'Menu'
            },
            viewModeType: ViewModeType.M
        };
        this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe((title) => {
            this.eventData.model.Title.value = title + ' Menu';
        }));
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.subscriptions.push(this.httpMenuService.getMenuElements().subscribe(result => {
            this.menuService.onAfterGetMenuElements(result.Root);
            this.localizationService.loadLocalizedElements(true);
            this.settingsService.getSettings();
            this.enumsService.getEnumsTable();
        }));
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.subscriptions.forEach((sub) => sub.unsubscribe());
    }
}
MenuComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-menu',
                template: "<div class=\"menu-container\"> <tb-menu-container></tb-menu-container> </div>",
                styles: [""],
                providers: [EventDataService]
            },] },
];
/**
 * @nocollapse
 */
MenuComponent.ctorParameters = () => [
    { type: HttpMenuService, },
    { type: MenuService, },
    { type: LocalizationService, },
    { type: SettingsService, },
    { type: EventManagerService, },
    { type: EventDataService, },
    { type: EnumsService, },
];
function MenuComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MenuComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MenuComponent.ctorParameters;
    /** @type {?} */
    MenuComponent.prototype.subscriptions;
    /** @type {?} */
    MenuComponent.prototype.httpMenuService;
    /** @type {?} */
    MenuComponent.prototype.menuService;
    /** @type {?} */
    MenuComponent.prototype.localizationService;
    /** @type {?} */
    MenuComponent.prototype.settingsService;
    /** @type {?} */
    MenuComponent.prototype.eventManagerService;
    /** @type {?} */
    MenuComponent.prototype.eventData;
    /** @type {?} */
    MenuComponent.prototype.enumsService;
}
