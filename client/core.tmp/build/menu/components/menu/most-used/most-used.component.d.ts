import { EventEmitter } from '@angular/core';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';
export declare class MostUsedComponent {
    private httpMenuService;
    private menuService;
    private utilsService;
    private imageService;
    private localizationService;
    itemSelected: EventEmitter<any>;
    constructor(httpMenuService: HttpMenuService, menuService: MenuService, utilsService: UtilsService, imageService: ImageService, localizationService: LocalizationService);
    runFunction(object: any): void;
}
