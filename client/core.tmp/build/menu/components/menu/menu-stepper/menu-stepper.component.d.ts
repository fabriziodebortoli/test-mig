import { OnInit } from '@angular/core';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';
export declare class MenuStepperComponent implements OnInit {
    private httpMenuService;
    private menuService;
    private utilsService;
    private imageService;
    private localizationService;
    applications: any;
    constructor(httpMenuService: HttpMenuService, menuService: MenuService, utilsService: UtilsService, imageService: ImageService, localizationService: LocalizationService);
    ngOnInit(): void;
    private menu;
    Menu: any;
}
