import { Component, Input, ViewEncapsulation } from '@angular/core';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
export class MenuElementComponent {
    /**
     * @param {?} httpMenuService
     * @param {?} menuService
     * @param {?} utilsService
     * @param {?} imageService
     */
    constructor(httpMenuService, menuService, utilsService, imageService) {
        this.httpMenuService = httpMenuService;
        this.menuService = menuService;
        this.utilsService = utilsService;
        this.imageService = imageService;
        this.lorem = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam';
        this.lorem = this.lorem.slice(0, Math.floor((Math.random() * 147) + 55));
    }
    /**
     * @param {?} object
     * @return {?}
     */
    getFavoriteClass(object) {
        return object.isFavorite ? 'star' : 'star_border';
    }
    /**
     * @param {?} object
     * @return {?}
     */
    runFunction(object) {
        event.stopPropagation();
        this.menuService.runFunction(object);
    }
}
MenuElementComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-menu-element',
                template: "<div class=\"menu-element\" [ngClass]=\"object.starHover ? 'active' : ''\"> <md-spinner *ngIf=\"object.isLoading\">{{object.title}}</md-spinner> <div class=\"menu-element-content\"> <div class=\"header\"> <md-icon class=\"object-type\" [ngClass]=\"object.objectType\">{{imageService.getObjectIcon(object)}}</md-icon> <span (click)=\"runFunction(object)\">{{object.title}}</span> <md-icon class=\"star\" [class.favorite]=\"object.isFavorite\" (click)=\"menuService.toggleFavorites(object)\" (mouseenter)=\"object.starHover=true\" (mouseleave)=\"object.starHover=false\">{{getFavoriteClass(object)}}</md-icon> </div> <p (click)=\"runFunction(object)\" *ngIf=\"menuService?.showDescription\" class=\"description\">{{lorem}}</p> </div> </div>",
                styles: [".menu-element { display: flex; flex-direction: row; flex-wrap: nowrap; align-content: stretch; align-items: flex-start; background: #f1f4f7; padding: 0.3rem; margin: 0.2rem 0; color: #0277bd; font-weight: 700; } .menu-element:hover { background: #fffbe0; transition: background 0.2s; } .menu-element-content { display: flex; flex-direction: column; justify-content: space-between; width: 100%; } .menu-element-content .header { display: flex; flex-direction: row; align-items: center; } .menu-element-content p.description { font-size: 12px; font-weight: 500; color: #000; margin-bottom: 0; cursor: pointer; display: flex; align-items: flex-end; } .menu-element .object-type { color: #bbb; font-size: 20px; line-height: 24px; } .menu-element .object-type.Document { color: #0277bd; } .menu-element .object-type.Report, .menu-element .object-type.Function { color: #3EAB66; } .menu-element .object-type.Batch { color: #ffa700; } .menu-element md-spinner { width: 30px; height: 21px; margin-top: 2px; } .menu-element span { font-size: 13px; line-height: 16px; cursor: pointer; flex: 1 1 auto; padding: 0 3px; color: #000; font-weight: 500; } .menu-element .isLoading { color: #999; } .menu-element .star { color: #bbb; font-size: 20px; line-height: 24px; } .menu-element .star:hover { color: #0277bd; cursor: pointer; } .menu-element .star.favorite { color: #0277bd; } @media screen and (min-width: 48em) { .menu-element .menu-element-content p.description { min-height: 25px; } .menu-element span { font-size: 14px; } } "],
                encapsulation: ViewEncapsulation.None
            },] },
];
/**
 * @nocollapse
 */
MenuElementComponent.ctorParameters = () => [
    { type: HttpMenuService, },
    { type: MenuService, },
    { type: UtilsService, },
    { type: ImageService, },
];
MenuElementComponent.propDecorators = {
    'object': [{ type: Input },],
};
function MenuElementComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MenuElementComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MenuElementComponent.ctorParameters;
    /** @type {?} */
    MenuElementComponent.propDecorators;
    /** @type {?} */
    MenuElementComponent.prototype.object;
    /** @type {?} */
    MenuElementComponent.prototype.lorem;
    /** @type {?} */
    MenuElementComponent.prototype.httpMenuService;
    /** @type {?} */
    MenuElementComponent.prototype.menuService;
    /** @type {?} */
    MenuElementComponent.prototype.utilsService;
    /** @type {?} */
    MenuElementComponent.prototype.imageService;
}
