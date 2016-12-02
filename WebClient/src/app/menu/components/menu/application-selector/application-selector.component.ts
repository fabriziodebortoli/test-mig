import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'tb-application-selector',
    templateUrl: './application-selector.component.html',
    styleUrls: ['./application-selector.component.css']
})

export class ApplicationSelectorComponent {


    private menu: any ;
    get Menu(): any {
        return this.menu;
    }
    
    @Input()
    set Menu(menu: any) {
        this.menu = menu;
        this.applications =  this.utilsService.toArray(menu.Application);
    }

   
    public applications: any;

    constructor(private menuService: MenuService, private utilsService: UtilsService) {
    }
}