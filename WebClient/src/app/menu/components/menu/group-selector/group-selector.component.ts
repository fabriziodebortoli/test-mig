import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'tb-group-selector',
    templateUrl: './group-selector.component.html',
    styleUrls: ['./group-selector.component.css']
})

export class GroupSelectorComponent implements OnInit {

    constructor(private menuService: MenuService, private utilService: UtilsService) {
    }

    ngOnInit() {
        // this.menu = this.menuService.applicationMenu;
        // this.applications = this.utilService.toArray(this.menuService.applicationMenu.Application);
    }
}