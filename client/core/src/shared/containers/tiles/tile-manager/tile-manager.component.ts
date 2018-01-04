import { Component, ContentChildren, QueryList, AfterContentInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import { Subscription } from '../../../../rxjs.imports';

import { LayoutService } from './../../../../core/services/layout.service';
import { TileGroupComponent } from './../tile-group/tile-group.component';

@Component({
  selector: 'tb-tilemanager',
  templateUrl: './tile-manager.component.html',
  styleUrls: ['./tile-manager.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class TileManagerComponent implements AfterContentInit {

  @ContentChildren(TileGroupComponent) tiles: QueryList<TileGroupComponent>;
  getTiles() {
    return this.tiles.toArray();
  }

  public viewHeightSubscription: Subscription;
  viewHeight: number;

  constructor(public layoutService: LayoutService) { }

  ngOnInit() {
    // this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
  }

  ngOnDestroy() {
    this.viewHeightSubscription.unsubscribe();
  }

  ngAfterContentInit() {
    // get all active tiles
    let activeTiles = this.tiles.filter((tile) => tile.active);

    //if there is no active tab set, activate the first
    if (activeTiles.length === 0 && this.tiles.toArray().length > 0) {
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

  changeTabByIndex(event) {
    let index = event.index;

    let currentTile = this.tiles.toArray()[index];
    this.selectTile(currentTile);
  }
}