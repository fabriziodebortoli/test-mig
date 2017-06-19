import { Component, Input, OnInit } from '@angular/core';


import { UtilsService } from '../../../../core/services/utils.service';
import { ImageService } from '../../../services/image.service';
import { MenuService } from '../../../services/menu.service';

@Component({
    selector: 'tb-application-selector',
    template: "<div class=\"application-selector\" *ngIf=\"menuService != undefined\"> <ul class=\"application-list\"> <li *ngFor=\"let application of applications\" (click)=\"selecteApplication(application)\"> <div *ngIf=\"application != undefined\"> <img [src]=\"imageService.getApplicationIcon(application)\" /><br /> <span>{{application.title}}</span> </div> </li> </ul> </div>",
    styles: [".application-selector { margin-bottom: 20px; } ul.application-list { list-style: none; padding: 0; margin: 0; display: flex; padding-top: 10px; padding-bottom: 10px; background: #3e3e3e; } ul.application-list > li { width: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; flex-direction: column; color: #9f9f9f; background: #3e3e3e; text-align: center; } ul.application-list > li:hover { color: #fff; } ul.application-list > li img { max-width: 120px; } ul.application-list > li span { font-size: 12px; } "]
})
export class ApplicationSelectorComponent {

    private menu: any;

    get Menu(): any {
        return this.menu;
    }

    @Input()
    set Menu(menu: any) {
        if (menu == undefined)
            return;

        this.menu = menu;
        this.applications = this.utilsService.toArray(menu.Application);
    }

    public applications: any;

    constructor(
        private menuService: MenuService,
        private utilsService: UtilsService,
        private imageService: ImageService
    ) { }

    selecteApplication(application) {
        this.menuService.setSelectedApplication(application)
    }

}