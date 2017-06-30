
import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { MenuService } from './../../../services/menu.service';
import { ImageService } from '@taskbuilder/core';
import { UtilsService } from '@taskbuilder/core';
import { LocalizationService } from './../../../services/localization.service';
import { SettingsService } from './../../../services/settings.service';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent implements OnInit {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    private menuService: MenuService,
    private imageService: ImageService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService
  ) { }

  ngOnInit() {
  }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }
}

