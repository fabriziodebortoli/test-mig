import { Component, OnInit, ElementRef, ViewEncapsulation, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state, HostBinding } from '@angular/core';

import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { TabberComponent } from './../../tabs/tabber/tabber.component';
import { DockpaneComponent } from './../dockpane.component';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  animations: [
    trigger('collapsing', [
      state('expanded', style({ width:'400px', overflow:'hidden' })),
      state('collapsed', style({ width:'40px', overflow:'hidden' })),
      transition('expanded <=> collapsed', animate('250ms ease-in-out')),
    ])
  ]
})
export class DockpaneContainerComponent implements AfterContentInit {

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;

  @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;
  getDockpanes() {
    return this.dockpanes.toArray();
  }

  dockState:string = 'expanded';
  idxActive:number;


  ngAfterContentInit() {
    resolvedPromise.then(() => {
      let dockpanes = this.dockpanes.toArray();
      let internalTabComponents = [];
      for (let i = 0; i < dockpanes.length; i++) {
        internalTabComponents.push(dockpanes[i].tabComponent);
      }
      this.kendoTabStripInstance.tabs.reset(internalTabComponents);
    });
  }

  changeDockpaneByIndex(i){

    if(this.idxActive === i){
      this.idxActive = null;
      this.dockState = 'collapsed';
    }else{
      this.dockState = 'expanded';
      this.idxActive = i;
      this.kendoTabStripInstance.selectTab(i);
      console.log("this.idxActive", this.idxActive);
    }
    
  }

  // public dockState: string = 'closed';

  // @ViewChild('dockpaneTabber') dockpaneTabber: TabStripComponent;
  // @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;

  // ngAfterContentInit() {
  //   let activeTiles = this.dockpanes.filter((tile) => tile.active);

  //   //if there is no active tab set, activate the first
  //   if (activeTiles.length === 0 && this.dockpanes.toArray().length > 0) {
  //     this.selectDock(this.dockpanes.first);
  //   }
  // }

  // selectDock(dockpane: DockpaneComponent) {
  //   if (dockpane.active) return;

  //   // deactivate all tabs
  //   this.dockpanes.toArray().forEach(dockpane => dockpane.active = false);

  //   // activate the tab the user has clicked on.
  //   dockpane.active = true;
  // }

  // getDockpanes() {
  //   return this.dockpanes.toArray();
  // }

  // toggleDockpane(dockpane: DockpaneComponent) {

  //   if (dockpane.active) {
  //     this.dockState = 'opened';
  //   }


  //   this.dockState = dockpane.active ? 'opened' : this.dockState === 'opened' ? 'closed' : 'opened';
  // }
}