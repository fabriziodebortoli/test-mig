import { UtilsService } from './../../../../core/utils.service';
import { Component, Input, OnInit } from '@angular/core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-menu-stepper',
  templateUrl: './menu-stepper.component.html',
  styleUrls: ['./menu-stepper.component.scss']
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