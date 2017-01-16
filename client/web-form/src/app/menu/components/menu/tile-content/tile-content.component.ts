import { EventManagerService } from './../../../services/event-manager.service';
import { Component, Input, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-tile-content',
  templateUrl: './tile-content.component.html',
  styleUrls: ['./tile-content.component.css']
})
export class TileContentComponent implements OnInit {

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService,
    private eventManagerService: EventManagerService
  ) {
  }
  ngOnInit() {
  }


  private tile: any;
  get Tile(): any {
    return this.tile;
  }

  @Input()
  set Tile(menu: any) {
    this.tile = menu;
  }

  getFavoriteClass(object) {
    return object.isFavorite ? 'star' : 'star_border';
  }

  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}

