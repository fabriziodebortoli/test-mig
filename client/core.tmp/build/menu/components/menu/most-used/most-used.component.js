import { Component, Output, EventEmitter } from '@angular/core';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';
export class MostUsedComponent {
    /**
     * @param {?} httpMenuService
     * @param {?} menuService
     * @param {?} utilsService
     * @param {?} imageService
     * @param {?} localizationService
     */
    constructor(httpMenuService, menuService, utilsService, imageService, localizationService) {
        this.httpMenuService = httpMenuService;
        this.menuService = menuService;
        this.utilsService = utilsService;
        this.imageService = imageService;
        this.localizationService = localizationService;
        this.itemSelected = new EventEmitter();
    }
    /**
     * @param {?} object
     * @return {?}
     */
    runFunction(object) {
        this.menuService.runFunction(object);
        this.itemSelected.emit();
    }
}
MostUsedComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-most-used',
                template: "<div class=\"most-used\" *ngIf=\"menuService?.mostUsedCount > 0\"> <ul class=\"most-used-list\"> <li *ngFor=\"let object of utilsService.toArray(menuService?.mostUsed) | slice:0:9;\" class=\"most-used-item\"> <md-icon class=\"type\">{{imageService.getObjectIcon(object)}}</md-icon> <span class=\"truncate\" (click)=\"runFunction(object)\">{{object.title}}</span> </li> </ul> </div>",
                styles: [".most-used { margin-bottom: 20px; } ul.most-used-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; background: #3e3e3e; } .most-used-item { display: flex; flex-direction: row; color: #9f9f9f; background: #3e3e3e; line-height: 30px; position: relative; } .most-used-item > md-icon.type { margin: 0 2px 0 7px; line-height: 30px; font-size: 20px; } .most-used-item > span { font-size: 12px; cursor: pointer; } .most-used-item > span:hover { color: #fff; } "]
            },] },
];
/**
 * @nocollapse
 */
MostUsedComponent.ctorParameters = () => [
    { type: HttpMenuService, },
    { type: MenuService, },
    { type: UtilsService, },
    { type: ImageService, },
    { type: LocalizationService, },
];
MostUsedComponent.propDecorators = {
    'itemSelected': [{ type: Output },],
};
function MostUsedComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MostUsedComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MostUsedComponent.ctorParameters;
    /** @type {?} */
    MostUsedComponent.propDecorators;
    /** @type {?} */
    MostUsedComponent.prototype.itemSelected;
    /** @type {?} */
    MostUsedComponent.prototype.httpMenuService;
    /** @type {?} */
    MostUsedComponent.prototype.menuService;
    /** @type {?} */
    MostUsedComponent.prototype.utilsService;
    /** @type {?} */
    MostUsedComponent.prototype.imageService;
    /** @type {?} */
    MostUsedComponent.prototype.localizationService;
}
