import { Component, Input, OnInit } from '@angular/core';

import { ImageService } from './../../../services/image.service';
import { UtilsService } from '@taskbuilder/core';
import { MenuService } from './../../../services/menu.service';

@Component({
    selector: 'tb-application-selector',
    templateUrl: './application-selector.component.html',
    styleUrls: ['./application-selector.component.scss']
})
export class ApplicationSelectorComponent {

    public applications: any;

    constructor(
        public menuService: MenuService,
        public utilsService: UtilsService,
        public imageService: ImageService
    ) {
    }

    selectApplication(application) {
        this.menuService.setSelectedApplication(application)
    }

}