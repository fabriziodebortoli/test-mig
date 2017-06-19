import { Component, Input, ViewEncapsulation } from '@angular/core';

import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';

@Component({
  selector: 'tb-menu-element',
  templateUrl: './menu-element.component.html',
  styleUrls: ['./menu-element.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class MenuElementComponent {

  @Input() object: any;

  lorem: string = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam';

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService
  ) {
    this.lorem = this.lorem.slice(0, Math.floor((Math.random() * 147) + 55));
  }

  getFavoriteClass(object) {
    return object.isFavorite ? 'star' : 'star_border';
  }

  runFunction(object) {
    event.stopPropagation();
    this.menuService.runFunction(object);
  }


}
