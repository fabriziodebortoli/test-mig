import { SettingsService } from './../../core/services/settings.service';
import { LocalizationService } from './../../core/services/localization.service';
import { Component, OnInit } from '@angular/core';

import { UtilsService } from './../../core/services/utils.service';
import { ImageService } from './../../menu/services/image.service';
import { MenuService } from './../../menu/services/menu.service';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {

  favorites: Array<any> = [];

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public localizationService: LocalizationService
  ) { }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

}
