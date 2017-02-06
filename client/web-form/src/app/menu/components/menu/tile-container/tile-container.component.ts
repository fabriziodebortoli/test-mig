import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-tile-container',
  templateUrl: './tile-container.component.html',
  styleUrls: ['./tile-container.component.css']
})
export class TileContainerComponent implements OnInit {

  private tiles: any = [];
  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService
  ) {
  }

  ngOnInit() {
    this.getTiles();
  }

  private getTiles() {
    let array = this.utilsService.toArray(this.menuService.selectedMenu.Menu);

    for (let i = 0; i < array.length; i++) {
      if (this.tileIsVisible(array[i]))
        this.tiles.push(array[i]);
    }
    return this.tiles;
  }

  //---------------------------------------------------------------------------------------------
  ifTileHasObjects(tile) {
    if (tile == undefined || tile.Object == undefined)
      return false;
    var array = this.utilsService.toArray(tile.Object);

    return array.length > 0;
  }

  tileIsVisible(tile) {
    return this.ifTileHasObjects(tile) && !tile.hiddenTile;
  }

}
