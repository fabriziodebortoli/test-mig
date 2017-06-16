import { Component, Output, EventEmitter } from '@angular/core';
import { UtilsService } from '../../../../core/services/utils.service';
import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';
import { LocalizationService } from '../../../services/localization.service';
export class FavoritesComponent {
    /**
     * @param {?} menuService
     * @param {?} imageService
     * @param {?} utilsService
     * @param {?} localizationService
     */
    constructor(menuService, imageService, utilsService, localizationService) {
        this.menuService = menuService;
        this.imageService = imageService;
        this.utilsService = utilsService;
        this.localizationService = localizationService;
        this.favorites = [];
        this.itemSelected = new EventEmitter();
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.favorites = this.menuService.getFavorites();
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
FavoritesComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-favorites',
                template: "<div class=\"favorites\" *ngIf=\"menuService?.favoritesCount > 0\"> <ul class=\"favorites-list\"> <li *ngFor=\"let favorite of favorites\" class=\"favorite-item\"> <md-icon class=\"type\">{{imageService.getObjectIcon(favorite)}}</md-icon> <span class=\"truncate\" (click)=\"runFunction(favorite)\">{{favorite.title}}</span> <md-icon class=\"close\" (click)=\"menuService.toggleFavorites(favorite)\" title=\"Remove\">close</md-icon> </li> </ul> </div>",
                styles: [".favorites { margin-bottom: 20px; } ul.favorites-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; background: #3e3e3e; } .favorite-item { display: flex; flex-direction: row; color: #9f9f9f; background: #3e3e3e; line-height: 30px; position: relative; } .favorite-item > md-icon.type { margin: 0 2px 0 7px; line-height: 30px; font-size: 20px; } .favorite-item > span { font-size: 12px; cursor: pointer; } .favorite-item > span:hover { color: #fff; } .favorite-item > md-icon.close { position: absolute; right: 0; font-size: 14px; line-height: 30px; cursor: pointer; color: #646464; } .favorite-item > md-icon.close:hover { color: #fff; } "]
            },] },
];
/**
 * @nocollapse
 */
FavoritesComponent.ctorParameters = () => [
    { type: MenuService, },
    { type: ImageService, },
    { type: UtilsService, },
    { type: LocalizationService, },
];
FavoritesComponent.propDecorators = {
    'itemSelected': [{ type: Output },],
};
function FavoritesComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    FavoritesComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    FavoritesComponent.ctorParameters;
    /** @type {?} */
    FavoritesComponent.propDecorators;
    /** @type {?} */
    FavoritesComponent.prototype.favorites;
    /** @type {?} */
    FavoritesComponent.prototype.itemSelected;
    /** @type {?} */
    FavoritesComponent.prototype.menuService;
    /** @type {?} */
    FavoritesComponent.prototype.imageService;
    /** @type {?} */
    FavoritesComponent.prototype.utilsService;
    /** @type {?} */
    FavoritesComponent.prototype.localizationService;
}
