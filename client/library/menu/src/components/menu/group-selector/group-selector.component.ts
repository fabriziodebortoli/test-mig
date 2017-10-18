import { Component, Output, EventEmitter, Input } from '@angular/core';

import { UtilsService } from '@taskbuilder/core';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';

@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.scss']
})
export class GroupSelectorComponent {

    @Output() itemSelected: EventEmitter<any> = new EventEmitter();
    iconType: string = 'M4';

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



