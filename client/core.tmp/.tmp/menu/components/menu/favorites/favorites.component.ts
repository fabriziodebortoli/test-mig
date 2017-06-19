import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { UtilsService } from '../../../../core/services/utils.service';

import { MenuService } from '../../../services/menu.service';
import { ImageService } from '../../../services/image.service';
import { LocalizationService } from '../../../services/localization.service';

@Component({
  selector: 'tb-favorites',
  template: "<div class=\"favorites\" *ngIf=\"menuService?.favoritesCount > 0\"> <ul class=\"favorites-list\"> <li *ngFor=\"let favorite of favorites\" class=\"favorite-item\"> <md-icon class=\"type\">{{imageService.getObjectIcon(favorite)}}</md-icon> <span class=\"truncate\" (click)=\"runFunction(favorite)\">{{favorite.title}}</span> <md-icon class=\"close\" (click)=\"menuService.toggleFavorites(favorite)\" title=\"Remove\">close</md-icon> </li> </ul> </div>",
  styles: [".favorites { margin-bottom: 20px; } ul.favorites-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; background: #3e3e3e; } .favorite-item { display: flex; flex-direction: row; color: #9f9f9f; background: #3e3e3e; line-height: 30px; position: relative; } .favorite-item > md-icon.type { margin: 0 2px 0 7px; line-height: 30px; font-size: 20px; } .favorite-item > span { font-size: 12px; cursor: pointer; } .favorite-item > span:hover { color: #fff; } .favorite-item > md-icon.close { position: absolute; right: 0; font-size: 14px; line-height: 30px; cursor: pointer; color: #646464; } .favorite-item > md-icon.close:hover { color: #fff; } "]
})
export class FavoritesComponent implements OnInit {

  private favorites: Array<any> = [];

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    private menuService: MenuService,
    private imageService: ImageService,
    private utilsService: UtilsService,
    private localizationService: LocalizationService
  ) { }

  ngOnInit() {
    this.favorites = this.menuService.getFavorites();
  }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }

}

