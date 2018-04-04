import { Component, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

import { TbComponent, UtilsService, SettingsService, TbComponentService } from '@taskbuilder/core';

import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

@Component({
  selector: 'tb-most-used',
  templateUrl: './most-used.component.html',
  styleUrls: ['./most-used.component.scss']
})
export class MostUsedComponent extends TbComponent {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public httpMenuService: HttpMenuService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public imageService: ImageService,
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