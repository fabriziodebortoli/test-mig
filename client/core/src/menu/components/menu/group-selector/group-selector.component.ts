import { Component, Output, EventEmitter, Input, ViewEncapsulation } from '@angular/core';

import { UtilsService } from './../../../../core/services/utils.service';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';

@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.scss']
})
export class GroupSelectorComponent {

    @Output() itemSelected: EventEmitter<any> = new EventEmitter();
    constructor(
        public menuService: MenuService,
        public imageService: ImageService,
        public utilsService: UtilsService
    ) {
     }

    selectGroup(group) {
        this.menuService.setSelectedGroup(group);
        this.itemSelected.emit();
    }
}



