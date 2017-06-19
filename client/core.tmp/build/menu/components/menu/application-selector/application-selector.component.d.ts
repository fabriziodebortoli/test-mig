import { UtilsService } from '../../../../core/services/utils.service';
import { ImageService } from '../../../services/image.service';
import { MenuService } from '../../../services/menu.service';
export declare class ApplicationSelectorComponent {
    private menuService;
    private utilsService;
    private imageService;
    private menu;
    Menu: any;
    applications: any;
    constructor(menuService: MenuService, utilsService: UtilsService, imageService: ImageService);
    selecteApplication(application: any): void;
}
