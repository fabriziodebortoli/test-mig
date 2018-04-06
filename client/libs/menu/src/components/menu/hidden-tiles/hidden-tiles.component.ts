import { TbComponent, UtilsService, SettingsService, TbComponentService } from '@taskbuilder/core';
import { Component, OnInit, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-hidden-tiles',
  templateUrl: './hidden-tiles.component.html',
  styleUrls: ['./hidden-tiles.component.scss']
})
export class HiddenTilesComponent extends TbComponent {

  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  constructor(
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
  }

}