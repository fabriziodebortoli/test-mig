import { Component, Input } from '@angular/core';

import { HttpMenuService, MenuService, UtilsService, ImageService } from '@taskbuilder/core';
@Component({
  selector: 'tb-menu-stepper',
  templateUrl: './menu-stepper.component.html',
  styleUrls: ['./menu-stepper.component.scss']
})
export class MenuStepperComponent {

  applications: any;

  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService
  ) { }

  public menu: any;

  get Menu(): any {
    return this.menu;
  }

  @Input()
  set Menu(menu: any) {
    if (menu == undefined)
      return;

    this.menu = menu;
    this.applications = menu.Application;
  }
}