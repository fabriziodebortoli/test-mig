import { OnInit, EventEmitter } from '@angular/core';
import { UtilsService } from '../../../../core/services/utils.service';
import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';
import { LocalizationService } from '../../../services/localization.service';
export declare class FavoritesComponent implements OnInit {
    private menuService;
    private imageService;
    private utilsService;
    private localizationService;
    private favorites;
    itemSelected: EventEmitter<any>;
    constructor(menuService: MenuService, imageService: ImageService, utilsService: UtilsService, localizationService: LocalizationService);
    ngOnInit(): void;
    runFunction(object: any): void;
}
