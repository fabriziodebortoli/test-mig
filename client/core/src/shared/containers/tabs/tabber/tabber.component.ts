import { AfterViewInit, Component, Output, EventEmitter, OnInit, AfterContentInit, Input, ViewChild, ElementRef, ContentChildren, QueryList, HostListener } from '@angular/core';

import { LayoutService } from './../../../../core/services/layout.service';

import { TabComponent } from '../tab/tab.component';

@Component({
  selector: 'tb-tabber',
  templateUrl: './tabber.component.html',
  styleUrls: ['./tabber.component.scss']
})
export class TabberComponent implements AfterContentInit {

  @ContentChildren(TabComponent) tabs: QueryList<TabComponent>;

  @ViewChild('tabContent') tabContent: ElementRef;
  viewHeight: number;

  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() selectedTab: EventEmitter<any> = new EventEmitter(true);

  constructor(public layoutService: LayoutService) {
  }

  getTabs() {
    return this.tabs.toArray();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcViewHeight();
  }
  calcViewHeight() {
    // this.viewHeight = this.tabContent ? this.tabContent.nativeElement.offsetHeight : 0;
    this.layoutService.setViewHeight(this.viewHeight);
    console.log("viewHeight", this.viewHeight);
  }

  ngAfterContentInit() {

    setTimeout(() => this.calcViewHeight(), 0);

    // get all active tabs
    let activeTabs = this.tabs.filter((tab) => tab.active);

    // if there is no active tab set, activate the first
    if (activeTabs.length === 0) {
      this.selectTab(this.tabs.first);
    }
  }

  selectTab(tab: TabComponent) {
    if (tab.active) return;

    // deactivate all tabs
    this.tabs.toArray().forEach(tab => tab.active = false);

    // activate the tab the user has clicked on.
    tab.active = true;

    this.selectedTab.emit(this.tabs.toArray().indexOf(tab));
  }

  closeTab(tab: TabComponent) {
    this.close.emit(tab);
    tab.close.emit(tab);
    this.selectTab(this.tabs.first);
  }

  /*

  

  

  

  removeTab(tab: TabComponent) {
    this.tabs.splice(this.tabs.indexOf(tab), 1);
    if (tab.active && this.tabs.length > 0) {
      this.tabs[0].active = true;
      this.selectedTab.emit(0);
    }
  }

  */
}
