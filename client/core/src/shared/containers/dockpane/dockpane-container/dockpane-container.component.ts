import { EventDataService } from './../../../../core/services/eventdata.service';
import { Component, OnInit, ElementRef, ViewEncapsulation, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state, HostBinding, ChangeDetectorRef, Input, HostListener, OnDestroy } from '@angular/core';

import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { DockpaneComponent } from './../dockpane.component';

@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  animations: [
    trigger('collapsing', [
      state('expanded', style({ width: '580px', overflow: 'hidden' })),
      state('collapsed', style({ width: '24px', overflow: 'hidden' })),
      transition('expanded <=> collapsed', animate('200ms ease')),
    ])
  ]
})
export class DockpaneContainerComponent implements AfterContentInit, OnDestroy {

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('kendoTabStripInstance', { read: ElementRef }) k: ElementRef;
  @ViewChild('selector') public selector: ElementRef;

  @ContentChildren(DockpaneComponent) dockpanes: QueryList<DockpaneComponent>;
  getDockpanes() {
    return this.dockpanes.filter(dock => dock.activated);
  }

  dockState: string = 'collapsed';
  @HostBinding('class.collapsed') get collapsed() { return this.dockState === 'collapsed' }
  @HostBinding('class.expanded') get expanded() { return this.dockState === 'expanded' }
  idxActive: number = null;

  subscriptions = [];

  constructor(public eventData: EventDataService) {

  }

  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      if (!this.pinned)
        this.closeDock();
    }
  }

  @HostBinding('class.pinned') pinned: boolean = false;
  getPinIcon() {
    return this.pinned ? 'tb-unpin' : 'tb-classicpin';
  }

  ngAfterContentInit() {

    /**
     * TODO - Usare store per pushare il component dockpane solo quando arriva effettivamente l'activation
     */

    this.resetDockingPane();

    this.subscriptions.push(this.eventData.activationChanged.subscribe(() => {
      this.resetDockingPane();
    }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach((sub) => { sub.unsubscribe(); });
  }

  resetDockingPane() {
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

  }

  closeDock() {
    this.idxActive = null;
    this.dockState = 'collapsed';
  }

  private contains(target: any): boolean {
    return this.selector.nativeElement.contains(target) ||
      (this.k ? this.k.nativeElement.contains(target) : false);
  }

}