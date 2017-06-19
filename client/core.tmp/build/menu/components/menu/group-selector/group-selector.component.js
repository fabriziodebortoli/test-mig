import { Component, Output, EventEmitter } from '@angular/core';
import { UtilsService } from '../../../../core/services/utils.service';
import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';
export class GroupSelectorComponent {
    /**
     * @param {?} menuService
     * @param {?} imageService
     * @param {?} utilsService
     */
    constructor(menuService, imageService, utilsService) {
        this.menuService = menuService;
        this.imageService = imageService;
        this.utilsService = utilsService;
        this.itemSelected = new EventEmitter();
    }
    /**
     * @param {?} group
     * @return {?}
     */
    selectGroup(group) {
        this.menuService.setSelectedGroup(group);
        this.itemSelected.emit();
    }
}
GroupSelectorComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-group-selector',
                template: "<div class=\"groups\" *ngIf=\"menuService != undefined\"> <ul class=\"groups-list\"> <li *ngFor=\"let group of utilsService.toArray(menuService?.selectedApplication?.Group)\" (click)=selectGroup(group)> <div *ngIf=\"group != undefined\"> <img [src]=\"imageService.getStaticImage(group)\" /><br /> <span>{{group?.title}}</span> </div> </li> </ul> </div>",
                styles: [".groups { margin-bottom: 20px; } ul.groups-list { list-style: none; padding: 0; margin: 0; padding-top: 10px; padding-bottom: 10px; display: flex; flex-wrap: wrap; background: #3e3e3e; } ul.groups-list > li { flex-grow: 1; width: 33%; height: 60px; cursor: pointer; display: flex; align-items: center; justify-content: center; flex-direction: column; color: #9f9f9f; background: #3e3e3e; text-align: center; margin-top: 15px; margin-bottom: 15px; } ul.groups-list > li:hover { color: #fff; } ul.groups-list > li > img { max-width: 120px; } ul.groups-list > li > div > span { font-size: 12px; } "]
            },] },
];
/**
 * @nocollapse
 */
GroupSelectorComponent.ctorParameters = () => [
    { type: MenuService, },
    { type: ImageService, },
    { type: UtilsService, },
];
GroupSelectorComponent.propDecorators = {
    'itemSelected': [{ type: Output },],
};
function GroupSelectorComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    GroupSelectorComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    GroupSelectorComponent.ctorParameters;
    /** @type {?} */
    GroupSelectorComponent.propDecorators;
    /** @type {?} */
    GroupSelectorComponent.prototype.itemSelected;
    /** @type {?} */
    GroupSelectorComponent.prototype.menuService;
    /** @type {?} */
    GroupSelectorComponent.prototype.imageService;
    /** @type {?} */
    GroupSelectorComponent.prototype.utilsService;
}
