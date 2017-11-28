import { Component } from '@angular/core';

import { OldLocalizationService } from './../../core/services/oldlocalization.service';
import { SettingsService } from './../../core/services/settings.service';
import { ThemeService } from './../../core/services/theme.service';
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
    public themeService: ThemeService,
    public localizationService: OldLocalizationService
  ) { }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

  changeTheme() {
    this.themeService.changeTheme('darcula');
  }

  resetTheme() {
    this.themeService.resetTheme();
  }

}
