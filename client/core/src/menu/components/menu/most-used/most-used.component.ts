import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { SettingsService } from './../../../../core/services/settings.service';
import { Component, Output, EventEmitter, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { TbComponent } from './../../../../shared/components/tb.component';

@Component({
  selector: 'tb-most-used',
  templateUrl: './most-used.component.html',
  styleUrls: ['./most-used.component.scss'],
  encapsulation: ViewEncapsulation.None
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