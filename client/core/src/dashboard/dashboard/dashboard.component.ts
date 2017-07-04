import { Component, OnInit } from '@angular/core';

import { LocalizationService } from './../../menu/services/localization.service';
import { SettingsService } from './../../menu/services/settings.service';
import { UtilsService } from './../../core/services/utils.service';
import { ImageService } from './../../menu/services/image.service';
import { MenuService } from './../../menu/services/menu.service';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  private favorites: Array<any> = [];

  constructor(
    private menuService: MenuService,
    private imageService: ImageService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService

  ) { }

  ngOnInit() {
  }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

}
