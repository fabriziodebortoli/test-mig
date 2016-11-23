import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../services/menu.service';
import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'tb-application-selector',
    templateUrl: './application-selector.component.html',
    styleUrls: ['./application-selector.component.css']
})

export class ApplicationSelectorComponent implements OnInit {


    private menu: any ;
    get Menu(): any {
        return this.menu;
    }
    
    @Input()
    set Menu(menu: any) {
        this.menu = menu;
        this.applications =  this.utilService.toArray(menu.Application);
    }

   
    public applications: any;

    constructor(private menuService: MenuService, private utilService: UtilsService) {
    }

    ngOnInit() {
        // this.menu = this.menuService.applicationMenu;
        // this.applications = this.utilService.toArray(this.menuService.applicationMenu.Application);
    }
}