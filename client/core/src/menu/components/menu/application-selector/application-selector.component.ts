import { Component, Input, OnInit } from '@angular/core';

import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';

@Component({
    selector: 'tb-application-selector',
    templateUrl: './application-selector.component.html',
    styleUrls: ['./application-selector.component.scss']
})
export class ApplicationSelectorComponent {

    public applications: any;

    constructor(
        private menuService: MenuService,
        private utilsService: UtilsService,
        private imageService: ImageService
    ) {
    }

    selecteApplication(application) {
        this.menuService.setSelectedApplication(application)
    }

}