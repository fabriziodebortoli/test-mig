import { Component, Input } from '@angular/core';
import { UtilsService } from '../../../../core/services/utils.service';
import { ImageService } from '../../../services/image.service';
import { MenuService } from '../../../services/menu.service';
export class ApplicationSelectorComponent {
    /**
     * @param {?} menuService
     * @param {?} utilsService
     * @param {?} imageService
     */
    constructor(menuService, utilsService, imageService) {
        this.menuService = menuService;
        this.utilsService = utilsService;
        this.imageService = imageService;
    }
    /**
     * @return {?}
     */
    get Menu() {
        return this.menu;
    }
    /**
     * @param {?} menu
     * @return {?}
     */
    set Menu(menu) {
        if (menu == undefined)
            return;
        this.menu = menu;
        this.applications = this.utilsService.toArray(menu.Application);
    }
    /**
     * @param {?} application
     * @return {?}
     */
    selecteApplication(application) {
        this.menuService.setSelectedApplication(application);
    }
}
ApplicationSelectorComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-application-selector',
                template: "<div class=\"application-selector\" *ngIf=\"menuService != undefined\"> <ul class=\"application-list\"> <li *ngFor=\"let application of applications\" (click)=\"selecteApplication(application)\"> <div *ngIf=\"application != undefined\"> <img [src]=\"imageService.getApplicationIcon(application)\" /><br /> <span>{{application.title}}</span> </div> </li> </ul> </div>",
                styles: [".application-selector { margin-bottom: 20px; } ul.application-list { list-style: none; padding: 0; margin: 0; display: flex; padding-top: 10px; padding-bottom: 10px; background: #3e3e3e; } ul.application-list > li { width: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; flex-direction: column; color: #9f9f9f; background: #3e3e3e; text-align: center; } ul.application-list > li:hover { color: #fff; } ul.application-list > li img { max-width: 120px; } ul.application-list > li span { font-size: 12px; } "]
            },] },
];
/**
 * @nocollapse
 */
ApplicationSelectorComponent.ctorParameters = () => [
    { type: MenuService, },
    { type: UtilsService, },
    { type: ImageService, },
];
ApplicationSelectorComponent.propDecorators = {
    'Menu': [{ type: Input },],
};
function ApplicationSelectorComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ApplicationSelectorComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ApplicationSelectorComponent.ctorParameters;
    /** @type {?} */
    ApplicationSelectorComponent.propDecorators;
    /** @type {?} */
    ApplicationSelectorComponent.prototype.menu;
    /** @type {?} */
    ApplicationSelectorComponent.prototype.applications;
    /** @type {?} */
    ApplicationSelectorComponent.prototype.menuService;
    /** @type {?} */
    ApplicationSelectorComponent.prototype.utilsService;
    /** @type {?} */
    ApplicationSelectorComponent.prototype.imageService;
}
