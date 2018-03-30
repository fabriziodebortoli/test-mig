import { EventDataService } from './../../../../core/services/eventdata.service';
import { Store } from './../../../../core/services/store.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { Component, ContentChildren, QueryList, AfterContentInit, ViewChild, ViewEncapsulation, Input, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { TileManagerTabComponent } from './tile-manager-tab/tile-manager-tab.component';
import { TbComponent } from '../../../components/tb.component';

import { Logger } from './../../../../core/services/logger.service';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-tile-manager',
  templateUrl: './tile-manager.component.html',
  styleUrls: ['./tile-manager.component.scss'],
  animations: [
    trigger('collapsing', [
      state('expanded', style({ width: '220px', overflow: 'hidden' })),
      state('collapsed', style({ width: '40px', overflow: 'hidden' })),
      transition('expanded <=> collapsed', animate('400ms ease')),
    ])
  ]
})
export class TileManagerComponent extends TbComponent implements AfterContentInit, OnDestroy {

  subscriptions = [];
  selectorCollapsed: string = localStorage.getItem('selectorCollapsed') ? localStorage.getItem('selectorCollapsed') : 'expanded';
  idxActive: number = 0;
  numberOfTabs: number = 0;
  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ContentChildren(TileManagerTabComponent) tilegroups: QueryList<TileManagerTabComponent>;
  constructor(
    public logger: Logger,
    public eventData: EventDataService,
    public tbComponentService: TbComponentService,
    protected changeDetectorRef: ChangeDetectorRef,
    public store: Store
  ) {
    super(tbComponentService, changeDetectorRef);
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => { s.unsubscribe(); });
  }

  getTilegroups() {
    return this.tilegroups.filter(
      (currentTab) => {
        return currentTab.activated;
      });
  }

  ngAfterContentInit() {
    this.resetTileManagerTabs();

    this.subscriptions.push(this.eventData.activationChanged.subscribe(() => {
      this.resetTileManagerTabs();
    }));
  }

  resetTileManagerTabs() {

    setTimeout(() => {
      let tilegroups = this.getTilegroups();
      let internalTabComponents = [];
      for (let i = 0; i < tilegroups.length; i++) {
        let currentTab = tilegroups[i];
        if (currentTab.activated) {
          internalTabComponents.push(currentTab.tabComponent);
        }
      }
      this.kendoTabStripInstance.tabs.reset(internalTabComponents);
      this.changeTilegroupByIndex(0);
    }, 1);
  }

  changeTilegroupByIndex(i) {
    this.idxActive = i;
    this.kendoTabStripInstance.selectTab(i)
  }

  getSelectorIcon() {
    return this.selectorCollapsed ? 'tb-circledrightfilled' : 'tb-gobackfilled';
  }

  toggleSelector() {
    this.selectorCollapsed = this.selectorCollapsed === 'expanded' ? 'collapsed' : 'expanded';
    localStorage.setItem('selectorCollapsed', this.selectorCollapsed);
  }
}