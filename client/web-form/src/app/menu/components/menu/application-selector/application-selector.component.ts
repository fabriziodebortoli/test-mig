import { UtilsService } from '@taskbuilder/core';
import { Component, Input, OnInit } from '@angular/core';

import { ImageService } from '@taskbuilder/core';
import { MenuService } from '@taskbuilder/core';

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