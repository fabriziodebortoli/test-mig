import { SettingsService } from './../../../../core/services/settings.service';
import { Component, Output, EventEmitter } from '@angular/core';

import { LocalizationService } from './../../../../core/services/localization.service';
import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-most-used',
  templateUrl: './most-used.component.html',
  styleUrls: ['./most-used.component.scss']
})
export class MostUsedComponent {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
    public localizationService: LocalizationService,
    public settingsService: SettingsService
  ) { }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }

}