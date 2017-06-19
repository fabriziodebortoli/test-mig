import { EventEmitter } from '@angular/core';
import { UtilsService } from '../../../../core/services/utils.service';
import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';
export declare class GroupSelectorComponent {
    private menuService;
    private imageService;
    private utilsService;
    itemSelected: EventEmitter<any>;
    constructor(menuService: MenuService, imageService: ImageService, utilsService: UtilsService);
    selectGroup(group: any): void;
}
