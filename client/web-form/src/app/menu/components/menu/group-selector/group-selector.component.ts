import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { ImageService } from './../../../services/image.service';

import { Component, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.scss']
})



export class GroupSelectorComponent {


    @Output() groupSelected: EventEmitter<any> = new EventEmitter();
    constructor(
        private menuService: MenuService, 
        private imageService: ImageService, 
        private utilsService: UtilsService
        ) {
    }

    selectGroup(group) {
        this.menuService.setSelectedGroup(group);
        this.groupSelected.emit();
    }
}



