import { Component, OnInit, ElementRef, ViewEncapsulation, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state, HostBinding, ChangeDetectorRef, Input } from '@angular/core';

import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { DockpaneComponent } from './../dockpane.component';

@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  animations: [
    trigger('collapsing', [
      state('expanded', style({ width: '580px', overflow: 'hidden' })),
      state('collapsed', style({ width: '40px', overflow: 'hidden' })),
      transition('expanded <=> collapsed', animate('400ms ease')),
    ])
  ]
})
export class DockpaneContainerComponent implements AfterContentInit {

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;

  @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;
  getDockpanes() {
    return this.dockpanes.filter(dock => dock.activated);
  }

  dockState: string = 'collapsed';
  idxActive: number = null;

  @HostBinding('class.pinned') pinned: boolean = false;
  getPinIcon() {
    return this.pinned ? 'tb-unpin' : 'tb-classicpin';
  }

  ngAfterContentInit() {
    
    /**
     * TODO - Usare store per pushare il component dockpane solo quando arriva effettivamente l'activation
     */
    setTimeout(() => {
      let dockpanes = this.dockpanes.toArray();
      let internalTabComponents = [];
      for (let i = 0; i < dockpanes.length; i++) {
        // console.log("Dock: ", dockpanes[i].title, " - activated: ", dockpanes[i].activated, " - ", dockpanes[i]);
        if (dockpanes[i].activated)
          internalTabComponents.push(dockpanes[i].tabComponent);
      }
      this.kendoTabStripInstance.tabs.reset(internalTabComponents);
    }, 1000);

  }

  changeDockpaneByIndex(i) {

    if (this.idxActive === i) {
      this.idxActive = null;
      this.dockState = 'collapsed';
    } else {
      this.dockState = 'expanded';
      this.idxActive = i;
      this.kendoTabStripInstance.selectTab(i);
    }

    this.pinned = false;

  }

}