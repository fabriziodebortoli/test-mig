import { Component, Output, EventEmitter } from '@angular/core';

import { UtilsService } from '../../../../core/services/utils.service';

import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';


@Component({
    selector: 'tb-group-selector',
    template: "<div class=\"groups\" *ngIf=\"menuService != undefined\"> <ul class=\"groups-list\"> <li *ngFor=\"let group of utilsService.toArray(menuService?.selectedApplication?.Group)\" (click)=selectGroup(group)> <div *ngIf=\"group != undefined\"> <img [src]=\"imageService.getStaticImage(group)\" /><br /> <span>{{group?.title}}</span> </div> </li> </ul> </div>",
    styles: [".groups { margin-bottom: 20px; } ul.groups-list { list-style: none; padding: 0; margin: 0; padding-top: 10px; padding-bottom: 10px; display: flex; flex-wrap: wrap; background: #3e3e3e; } ul.groups-list > li { flex-grow: 1; width: 33%; height: 60px; cursor: pointer; display: flex; align-items: center; justify-content: center; flex-direction: column; color: #9f9f9f; background: #3e3e3e; text-align: center; margin-top: 15px; margin-bottom: 15px; } ul.groups-list > li:hover { color: #fff; } ul.groups-list > li > img { max-width: 120px; } ul.groups-list > li > div > span { font-size: 12px; } "]
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



