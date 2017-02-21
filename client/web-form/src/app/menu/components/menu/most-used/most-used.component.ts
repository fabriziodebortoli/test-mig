import { Component, Output, EventEmitter } from '@angular/core';

import { UtilsService } from 'tb-core';

import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { LocalizationService } from './../../../services/localization.service';

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
    private localizationService: LocalizationService
  ) { }

  runFunction(object) {
    this.menuService.runFunction(object);
    this.itemSelected.emit();
  }

}