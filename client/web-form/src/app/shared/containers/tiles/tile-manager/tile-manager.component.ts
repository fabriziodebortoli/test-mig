import { TileGroupComponent } from './../tile-group/tile-group.component';
import { Component, ContentChildren, QueryList, AfterContentInit } from '@angular/core';
import { TabberComponent } from '../../tabs';

@Component({
  selector: 'tb-tilemanager',
  templateUrl: './tile-manager.component.html',
  styleUrls: ['./tile-manager.component.scss']
})

export class TileManagerComponent implements AfterContentInit {
  @ContentChildren(TileGroupComponent) tiles: QueryList<TileGroupComponent>;

  getTiles() {
    return this.tiles.toArray();
  }

  ngAfterContentInit() {

    // get all active tiles
    let activeTiles = this.tiles.filter((tile) => tile.active);

    // if there is no active tab set, activate the first
    if (activeTiles.length === 0) {
      this.selectTile(this.tiles.first);
    }
  }

  selectTile(tile: TileGroupComponent) {
    if (tile.active) return;

    // deactivate all tabs
    this.tiles.toArray().forEach(tile => tile.active = false);

    // activate the tab the user has clicked on.
    tile.active = true;
  }
}