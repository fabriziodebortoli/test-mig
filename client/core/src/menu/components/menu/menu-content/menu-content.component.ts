import { Component, Input } from '@angular/core';

import { EventManagerService } from './../../../services/event-manager.service';
import { ImageService } from './../../../services/image.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';

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

  private objects: any;

  private _tile: any;
  @Input()
  get tile(): any {
    return this._tile;
  }

  set tile(tile: any) {
    this._tile = tile;
    this.objects = this._tile.Object;
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}