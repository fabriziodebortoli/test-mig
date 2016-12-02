import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { ImageService } from './../../../services/image.service';

import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.css']
})

export class GroupSelectorComponent {

    constructor(private menuService: MenuService, private imageService: ImageService, private utilsService: UtilsService) {
    }
}