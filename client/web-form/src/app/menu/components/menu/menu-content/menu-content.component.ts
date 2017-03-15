import { UtilsService } from './../../../../core/utils.service';
import { EventManagerService } from './../../../services/event-manager.service';
import { Component, Input, OnInit } from '@angular/core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-menu-content',
  templateUrl: './menu-content.component.html',
  styleUrls: ['./menu-content.component.scss']
})
export class MenuContentComponent implements OnInit {

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


  getPinnedClass(tile) {
    return tile.pinned ? 'hdr_strong' : 'hdr_weak';
  }
}