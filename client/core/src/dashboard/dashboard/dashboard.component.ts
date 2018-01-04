import { Component, ChangeDetectorRef } from '@angular/core';

import { SettingsService } from './../../core/services/settings.service';
import { ThemeService } from './../../core/services/theme.service';
import { UtilsService } from './../../core/services/utils.service';
import { ImageService } from './../../menu/services/image.service';
import { MenuService } from './../../menu/services/menu.service';
import { TbComponent } from './../../shared/components/tb.component';
import { TbComponentService } from './../../core/services/tbcomponent.service';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent extends TbComponent {

  favorites: Array<any> = [];

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public themeService: ThemeService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) { 
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

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
