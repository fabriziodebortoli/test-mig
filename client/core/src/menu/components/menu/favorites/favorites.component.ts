import { SettingsService } from './../../../../core/services/settings.service';
import { OldLocalizationService } from './../../../../core/services/oldlocalization.service';
import { Component, OnInit, Output, EventEmitter, ViewEncapsulation } from '@angular/core';


import { UtilsService } from './../../../../core/services/utils.service';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class FavoritesComponent implements OnInit {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public localizationService: OldLocalizationService,
    public settingsService: SettingsService,
  ) { }

  ngOnInit() {
  }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }
}

