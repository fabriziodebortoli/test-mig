import { TbComponent } from './../../../../shared/components/tb.component';
import { SettingsService } from './../../../../core/services/settings.service';
import { Component, OnInit, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';


import { UtilsService } from './../../../../core/services/utils.service';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';

@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class FavoritesComponent extends TbComponent  {

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

