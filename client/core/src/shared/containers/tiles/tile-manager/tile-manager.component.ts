import { Component, ContentChildren, QueryList, AfterContentInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { Subscription } from '../../../../rxjs.imports';

import { LayoutService } from './../../../../core/services/layout.service';
import { TileManagerTabComponent } from './tile-manager-tab/tile-manager-tab.component';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-tile-manager',
  templateUrl: './tile-manager.component.html',
  styleUrls: ['./tile-manager.component.scss']
})

export class TileManagerComponent implements AfterContentInit {

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;

  @ContentChildren(TileManagerTabComponent) tilegroups: QueryList<TileManagerTabComponent>;
  getTilegroups() {
    return this.tilegroups.toArray();
  }

  ngAfterContentInit() {
    resolvedPromise.then(() => {
      let tilegroups = this.tilegroups.toArray();
      let internalTabComponents = [];
      for (let i = 0; i < tilegroups.length; i++) {
        internalTabComponents.push(tilegroups[i].tabComponent);
      }
      this.kendoTabStripInstance.tabs.reset(internalTabComponents);
    });
  }

  changeTilegroupByIndex(i) {
    console.log('changeTilegroupByIndex', i)
    this.kendoTabStripInstance.selectTab(i)
  }

/** OLD */

  // @ContentChildren(TileGroupComponent) tiles: QueryList<TileGroupComponent>;
  // getTiles() {
  //   return this.tiles.toArray();
  // }

  public viewHeightSubscription: Subscription;
  viewHeight: number;

  constructor(public layoutService: LayoutService) { }

  ngOnInit() {
    // this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);//TODO riattivare nel caso
  }

  ngOnDestroy() {
    // this.viewHeightSubscription.unsubscribe(); //TODO riattivare nel caso
  }

  // ngAfterContentInit() {
  //   // get all active tiles
  //   let activeTiles = this.tiles.filter((tile) => tile.active);

  //   //if there is no active tab set, activate the first
  //   if (activeTiles.length === 0 && this.tiles.toArray().length > 0) {
  //     this.selectTile(this.tiles.first);
  //   }
  // }

  // selectTile(tile: TileGroupComponent) {
  //   if (tile.active) return;

  //   // deactivate all tabs
  //   this.tiles.toArray().forEach(tile => tile.active = false);

  //   // activate the tab the user has clicked on.
  //   tile.active = true;
  // }

  // changeTabByIndex(event) {
  //   let index = event.index;

  //   let currentTile = this.tiles.toArray()[index];
  //   this.selectTile(currentTile);
  // }
}