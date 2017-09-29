import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';
import { TabberComponent } from './../../tabs/tabber/tabber.component';
import { DockpaneComponent } from './../dockpane.component';
import { Component, OnInit, ElementRef, ViewEncapsulation, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state } from '@angular/core';

@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  animations: [
    trigger('slideInOut', [
      state('closed', style({ width: '0' })),
      state('opened', style({ width: '400px' })),
      transition('closed <=> opened', animate('250ms ease-in-out'))
    ]),
  ],
  encapsulation: ViewEncapsulation.None
})
export class DockpaneContainerComponent implements AfterContentInit {
  public dockState: string = 'closed';

  @ViewChild('dockpaneTabber') dockpaneTabber: TabStripComponent;
  @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;

  ngAfterContentInit() {
    // get all active tiles
    let activeTiles = this.dockpanes.filter((tile) => tile.active);
    console.log('activeTiles', activeTiles)
    console.log('this.dockpanes.toArray()', this.dockpanes.toArray())

    //if there is no active tab set, activate the first
    if (activeTiles.length === 0 && this.dockpanes.toArray().length > 0) {
      this.selectDock(this.dockpanes.first);
    }
  }

  selectDock(dockpane: DockpaneComponent) {
    if (dockpane.active) return;

    // deactivate all tabs
    this.dockpanes.toArray().forEach(dockpane => dockpane.active = false);

    // activate the tab the user has clicked on.
    dockpane.active = true;
  }

  getDockpanes() {
    return this.dockpanes.toArray();
  }

  toggleDockpane(dockpane: DockpaneComponent) {

    if (dockpane.active) {
      this.dockState = 'opened';
    }


    this.dockState = dockpane.active ? 'opened' : this.dockState === 'opened' ? 'closed' : 'opened';
  }
}