import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation } from '@angular/core';
import { Subscription } from 'rxjs';

import { MasonryOptions } from 'angular2-masonry';

import { UtilsService } from './../../../../core/services/utils.service';
import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';

@Component({
  selector: 'tb-menu-container',
  template: "<tb-frame> <tb-toolbar-top></tb-toolbar-top> <tb-frame-content> <div class=\"menu-page\"> <kendo-tabstrip #tabber (tabSelect)=\"changeTabByIndex($event)\" [height]=\"31\"> <kendo-tabstrip-tab [title]=\"menuTab?.title\" *ngFor=\"let menuTab of utilsService.toArray(menuService?.selectedGroup?.Menu)\"> <ng-template kendoTabContent> <div class=\"wrap\"> <tb-header-strip [title]=\"menuService?.selectedMenu?.title\"> <div class=\"header-strip-content\"> <button class=\"show-description\" title=\"Show Descriptions\" kendoButton (click)=\"menuService?.toggleDecription()\" [bare]=\"!menuService?.showDescription\" [primary]=\"menuService?.showDescription\"><md-icon>subject</md-icon></button> </div> <div class=\"header-strip-under\"> <tb-menu-stepper [Menu]=\"menuService.applicationMenu\"></tb-menu-stepper> </div> </tb-header-strip> <div class=\"menu-container\" *ngIf=\"menuService.selectedMenu != undefined\"> <masonry [options]=\"masonryOptions\" #masonryContainer> <div class=\"brick-sizer\"></div> <tb-menu-content *ngFor=\"let tile of tiles\" [tile]=\"tile\" class=\"col-xs-12 col-md-6\" [ngClass]=\"{'col-lg-3': tiles.length > 3, 'col-lg-4': tiles.length < 4}\"></tb-menu-content> </masonry> </div> </div> </ng-template> </kendo-tabstrip-tab> </kendo-tabstrip> </div> </tb-frame-content> </tb-frame>",
  styles: [".menu-page { display: flex; flex-direction: column; flex: 1; } .menu-container div.toolbar-top { border-bottom: 0; } .menu-page kendo-tabstrip { background: #fff; flex: 1; } .menu-page .k-tabstrip-top > .k-tabstrip-items { min-height: 30px; } .menu-page .k-tabstrip-top > .k-tabstrip-items .k-item { border: none; border-radius: 0; margin-bottom: 0; } .menu-page .k-tabstrip > .k-tabstrip-items .k-item .k-link { font-weight: 700; font-size: 13px; color: #000; } .menu-page .k-tabstrip-top > .k-tabstrip-items .k-item.k-state-active { border: none; } .menu-page .k-tabstrip-top > .k-tabstrip-items .k-item.k-state-active .k-link { color: #0277bd; } .menu-page .k-content { background: #F1F4F7; flex: 1; } .menu-page .show-description md-icon { padding: 0; margin: 0; width: 18px; height: 18px; font-size: 18px; } "],
  encapsulation: ViewEncapsulation.None
})

export class MenuContainerComponent implements OnInit, OnDestroy {
  private subscriptions: Subscription[] = [];
  private selectedGroupChangedSubscription;
  private tiles: any[];

  @ViewChild('tabber') tabber;
  @ViewChild('masonryContainer') masonryContainer: any;
  constructor(
    private menuService: MenuService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService
  ) {

    this.subscriptions.push(this.menuService.menuActivated.subscribe(() => {
      this.refreshLayout();
    }));
  }

  refreshLayout() {
    if (this.masonryContainer != undefined) {
      this.masonryContainer.layout();
    }
  }

  public masonryOptions: MasonryOptions = {
    transitionDuration: '0.2s'
  };

  ngOnInit() {

    this.subscriptions.push(this.menuService.selectedMenuChanged.subscribe(() => {
      this.changeTabWhenMenuChanges();
      this.tiles = this.getTiles();
    }));

    this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe(() => {
      this.initTab();
    }));
  }

  initTab() {
    if (this.menuService.selectedGroup == undefined) {
      return;
    }

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);

    let found = false;
    for (let i = 0; i < tempMenuArray.length; i++) {
      if (tempMenuArray[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
        this.menuService.setSelectedMenu(tempMenuArray[i]);
        return;
      }
    }

    if (!found) {
      this.menuService.setSelectedMenu(tempMenuArray[0]);
    }

  }

  isSelected(tab) {
    if (this.menuService.selectedMenu == undefined || tab == undefined)
      return false;
    return tab.title == this.menuService.selectedMenu.title;
  }

  changeTabWhenMenuChanges() {
    if (this.menuService.selectedMenu == undefined)
      return;

    let idx = this.findTabIndexByMenu();
    // if (idx >= 0 && !this.tabber.tabs[idx].active)
    this.tabber.selectTab(idx);
  }

  findTabIndexByMenu(): number {

    for (let i = 0; i < this.menuService.selectedGroup.Menu.length; i++) {
      if (this.menuService.selectedGroup.Menu[i].title == this.menuService.selectedMenu.title)
        return i;
    }
    return -1;
  }

  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }

  changeTabByIndex(event) {
    let index = event.index;
    if (index < 0 || this.menuService.selectedGroup == undefined)
      return;

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);
    let tab = tempMenuArray[index];
    if (tab != undefined) {
      this.menuService.setSelectedMenu(tab);
    }
  }

  getTiles() {
    let array = this.utilsService.toArray(this.menuService.selectedMenu.Menu);
    let newArray = [];
    for (let i = 0; i < array.length; i++) {
      if (this.tileIsVisible(array[i]))
        newArray.push(array[i]);
    }
    return newArray;
  }

  //---------------------------------------------------------------------------------------------
  ifTileHasObjects(tile) {
    if (tile == undefined || tile.Object == undefined)
      return false;
    var array = this.utilsService.toArray(tile.Object);

    return array.length > 0;
  }

  tileIsVisible(tile) {
    return this.ifTileHasObjects(tile);
  }
}