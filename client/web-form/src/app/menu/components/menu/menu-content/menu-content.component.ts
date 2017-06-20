import { Component, Input } from '@angular/core';

import { UtilsService } from '@taskbuilder/core';
import { EventManagerService } from './../../../services/event-manager.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-menu-content',
  templateUrl: './menu-content.component.html',
  styleUrls: ['./menu-content.component.scss']
})
export class MenuContentComponent {

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private eventManagerService: EventManagerService
  ) {
  }

  @Input('tile') tile: any;

  getObjects() {
    return this.utilsService.toArray(this.tile.Object);
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}