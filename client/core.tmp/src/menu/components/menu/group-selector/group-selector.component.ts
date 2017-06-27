import { Component, Output, EventEmitter } from '@angular/core';

import { UtilsService } from '../../../../core/services/utils.service';

import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';


@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.scss']
})
export class GroupSelectorComponent {

    @Output() itemSelected: EventEmitter<any> = new EventEmitter();

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



