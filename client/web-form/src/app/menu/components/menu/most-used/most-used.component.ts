import { Component, Output, EventEmitter } from '@angular/core';

import { UtilsService } from '@taskbuilder/core';

import { MenuService } from '@taskbuilder/core';
import { HttpMenuService } from '@taskbuilder/core';
import { ImageService } from '@taskbuilder/core';
import { LocalizationService } from '@taskbuilder/core';
import { SettingsService } from '@taskbuilder/core';

@Component({
  selector: 'tb-most-used',
  templateUrl: './most-used.component.html',
  styleUrls: ['./most-used.component.scss']
})
export class MostUsedComponent {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private localizationService: LocalizationService,
    private settingsService : SettingsService
  ) { }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }

}