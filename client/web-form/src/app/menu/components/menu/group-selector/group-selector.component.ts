import { UtilsService } from '@taskbuilder/core';
import { Component, Output, EventEmitter, Input } from '@angular/core';

import { MenuService } from '@taskbuilder/core';
import { ImageService } from '@taskbuilder/core';

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



