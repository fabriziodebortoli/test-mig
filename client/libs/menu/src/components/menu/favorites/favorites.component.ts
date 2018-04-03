import { Component, OnInit, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

import { MenuService, ImageService, UtilsService, SettingsService, TbComponentService, TbComponent } from '@taskbuilder/core';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent extends TbComponent {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public menuService: MenuService,
    public imageService: ImageService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }
}

