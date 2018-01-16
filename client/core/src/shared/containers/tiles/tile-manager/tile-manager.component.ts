import { Component, ContentChildren, QueryList, AfterContentInit, ViewChild, ViewEncapsulation, Input } from '@angular/core';
import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { TileManagerTabComponent } from './tile-manager-tab/tile-manager-tab.component';

import { LayoutService } from './../../../../core/services/layout.service';
import { Subscription } from '../../../../rxjs.imports';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-tile-manager',
  templateUrl: './tile-manager.component.html',
  styleUrls: ['./tile-manager.component.scss'],
  animations: [
    trigger('collapsing', [
        state('expanded', style({ width: '220px', overflow:'hidden' })),
        state('collapsed', style({ width: '40px', overflow:'hidden' })),
        transition('expanded <=> collapsed', animate('400ms ease')),
    ])
  ]
})
export class TileManagerComponent implements AfterContentInit {

  selectorCollapsed:string = localStorage.getItem('selectorCollapsed') ? localStorage.getItem('selectorCollapsed') : 'expanded';
  idxActive:number = 0;

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
      this.changeTilegroupByIndex(0);
    });
  }

  changeTilegroupByIndex(i) {
    this.idxActive = i;
    this.kendoTabStripInstance.selectTab(i)
    console.log("this.idxActive", this.idxActive)
  }

  getSelectorIcon(){
        return this.selectorCollapsed ? 'tb-circledrightfilled': 'tb-gobackfilled';
  }
  
  toggleSelector(){
    this.selectorCollapsed = this.selectorCollapsed === 'expanded' ? 'collapsed' : 'expanded';
    localStorage.setItem('selectorCollapsed', this.selectorCollapsed);
  }

  // public viewHeightSubscription: Subscription;
  // viewHeight: number;

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

}