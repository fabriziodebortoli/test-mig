import { Component, Input, OnInit } from '@angular/core';

import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-menu-stepper',
  template: "<div *ngIf=\"menuService?.selectedApplication?.title != undefined\"> <md-menu #apps=\"mdMenu\" x-position=\"after\" y-position=\"below\"> <button md-menu-item *ngFor=\"let application of applications\" (click)=menuService.setSelectedApplication(application) class=\"applicationItem\" [ngClass]=\"application?.title == menuService?.selectedApplication?.title ? 'active' : ''\"> {{application?.title}} </button> </md-menu> <md-menu #groups=\"mdMenu\" x-position=\"after\" y-position=\"below\"> <button md-menu-item *ngFor=\"let group of utilsService.toArray(menuService?.selectedApplication?.Group)\" (click)=menuService.setSelectedGroup(group) class=\"applicationItem\" [ngClass]=\"group?.title == menuService?.selectedGroup?.title ? 'active' : ''\"> <img [src]=\"imageService.getStaticImage(group)\" /> {{group?.title}} </button> </md-menu> <md-menu #menus=\"mdMenu\" x-position=\"after\" y-position=\"below\"> <button md-menu-item *ngFor=\"let menuTab of utilsService.toArray(menuService.selectedGroup?.Menu)\" (click)=menuService.setSelectedMenu(menuTab) class=\"applicationItem\" [ngClass]=\"menuTab?.title == menuService?.selectedMenu?.title ? 'active' : ''\"> {{menuTab?.title}} </button> </md-menu> <ul class=\"breadcrumb\"> <li [md-menu-trigger-for]=\"apps\"> <div> {{menuService?.selectedApplication?.title}} </div> </li> <li [md-menu-trigger-for]=\"groups\"> <div> {{menuService?.selectedGroup?.title}} </div> </li> <li [md-menu-trigger-for]=\"menus\"> <div> {{menuService?.selectedMenu?.title}} </div> </li> <li class=\"current\">{{menuService?.selectedMenu?.title}}</li> </ul> </div>",
  styles: [".breadcrumb { list-style: none; font-size: 12px; display: flex; flex-direction: row; margin: 2px 0; padding: 0; } .breadcrumb li div { padding: 0 3px; cursor: pointer; } .breadcrumb li div::after { content: '\00bb'; } .breadcrumb li.current { font-style: italic; font-weight: bold; } "]
})
export class MenuStepperComponent implements OnInit {

  applications: any;
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private localizationService: LocalizationService
  ) {
  }
  ngOnInit() {

  
  }

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
}