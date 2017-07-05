import { Component, Output, EventEmitter, Input } from '@angular/core';

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
    iconType: string = 'M4';
    constructor(
        private menuService: MenuService,
        private imageService: ImageService,
        private utilsService: UtilsService
    ) { }

    selectGroup(group) {
        this.menuService.setSelectedGroup(group);
        this.itemSelected.emit();
    }
}


