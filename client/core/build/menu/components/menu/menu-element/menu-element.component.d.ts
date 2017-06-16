import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
export declare class MenuElementComponent {
    private httpMenuService;
    private menuService;
    private utilsService;
    private imageService;
    object: any;
    lorem: string;
    constructor(httpMenuService: HttpMenuService, menuService: MenuService, utilsService: UtilsService, imageService: ImageService);
    getFavoriteClass(object: any): "star" | "star_border";
    runFunction(object: any): void;
}
