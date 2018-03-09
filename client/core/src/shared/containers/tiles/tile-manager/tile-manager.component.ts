import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { Component, ContentChildren, QueryList, AfterContentInit, ViewChild, ViewEncapsulation, Input, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';

import { Subscription } from '../../../../rxjs.imports';

import { TileManagerTabComponent } from './tile-manager-tab/tile-manager-tab.component';
import { TbComponent } from '../../../components/tb.component';

import { LayoutService } from './../../../../core/services/layout.service';
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
    public layoutService: LayoutService,
    public logger: Logger,
    public tbComponentService: TbComponentService,
    protected changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => { s.unsubscribe(); });
  }

  getTilegroups() {
    return this.tilegroups.toArray();
  }

  ngAfterContentInit() {
    resolvedPromise.then(() => {
      this.resetTileManagerTabs();
    });

    this.subscriptions.push(this.tilegroups.changes.subscribe((changes) => {
      if (this.numberOfTabs != changes.length) {
        this.numberOfTabs = changes.length;
        this.resetTileManagerTabs();
      }
    }));
  }

  resetTileManagerTabs() {
    let tilegroups = this.tilegroups.toArray();
    let internalTabComponents = [];
    for (let i = 0; i < tilegroups.length; i++) {
      internalTabComponents.push(tilegroups[i].tabComponent);
    }
    this.kendoTabStripInstance.tabs.reset(internalTabComponents);
    this.changeTilegroupByIndex(0);
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