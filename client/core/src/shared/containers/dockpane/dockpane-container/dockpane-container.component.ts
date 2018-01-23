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
      transition('expanded <=> collapsed', animate('400ms ease')),
    ])
  ]
})
export class DockpaneContainerComponent implements AfterContentInit {

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;

  @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;
  getDockpanes() {
    return this.dockpanes.toArray();
  }

  dockState:string = 'collapsed';
  idxActive:number = null;

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
    }
    
  }

}