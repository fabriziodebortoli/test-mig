import { Component, Input } from '@angular/core';

import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';

@Component({
  selector: 'tb-menu-element',
  templateUrl: './menu-element.component.html',
  styleUrls: ['./menu-element.component.scss']
})
export class MenuElementComponent {

  @Input() object: any;

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService
  ) { }

  getFavoriteClass(object) {
    return object.isFavorite ? 'star' : 'star_border';
  }

  runFunction(object) {
    this.menuService.runFunction(object);
  }
}
