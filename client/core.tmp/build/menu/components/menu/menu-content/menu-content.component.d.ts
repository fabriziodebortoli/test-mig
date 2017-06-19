import { UtilsService } from './../../../../core/services/utils.service';
import { EventManagerService } from './../../../services/event-manager.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
export declare class MenuContentComponent {
    private httpMenuService;
    private menuService;
    private utilsService;
    private imageService;
    private eventManagerService;
    constructor(httpMenuService: HttpMenuService, menuService: MenuService, utilsService: UtilsService, imageService: ImageService, eventManagerService: EventManagerService);
    tile: any;
    getObjects(): any;
    getPinnedClass(tile: any): "hdr_strong" | "hdr_weak";
}
