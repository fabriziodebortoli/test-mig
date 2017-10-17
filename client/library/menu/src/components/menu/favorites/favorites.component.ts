import { LocalizationService } from '@taskbuilder/core';
import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { SettingsService } from './../../../services/settings.service';

import { UtilsService } from '@taskbuilder/core';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent implements OnInit {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public localizationService: LocalizationService,
    public settingsService: SettingsService,
  ) { }

  ngOnInit() {
  }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }
}
