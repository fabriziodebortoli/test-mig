import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
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

