import { TbComponent } from './../../../../shared/components/tb.component';
import { SettingsService } from './../../../../core/services/settings.service';
import { Component, OnInit, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';


import { UtilsService } from './../../../../core/services/utils.service';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';

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

