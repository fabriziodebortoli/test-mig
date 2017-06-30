import { Component, Input } from '@angular/core';

import { UtilsService } from '@taskbuilder/core';
import { EventManagerService } from '@taskbuilder/core';
import { MenuService } from '@taskbuilder/core';
import { HttpMenuService } from '@taskbuilder/core';
import { ImageService } from '@taskbuilder/core';

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