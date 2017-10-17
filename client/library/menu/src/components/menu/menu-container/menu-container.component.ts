import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation, AfterViewInit, AfterContentInit, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';

import { LocalizationService } from '@taskbuilder/core';
import { SettingsService } from './../../../services/settings.service';
import { UtilsService } from '@taskbuilder/core';
import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class MenuContainerComponent implements AfterContentInit, OnDestroy {
  public subscriptions: Subscription[] = [];
  public selectedGroupChangedSubscription;
  public tiles: any[];

  @ViewChild('tabber') tabber;

  constructor(
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public localizationService: LocalizationService
  ) {

    this.subscriptions.push(this.menuService.menuActivated.subscribe(() => {
      this.tiles = this.getTiles();
      this.changeTabWhenMenuChanges();
      this.refreshLayout();
    }));

    this.subscriptions.push(this.menuService.selectedMenuChanged.subscribe(() => {
      this.tiles = this.getTiles();
      this.changeTabWhenMenuChanges();
    }));

    this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe(() => {
      this.initTab();
    }));
  }

  refreshLayout() {
    console.log("menuContainer.refreshLayout()");
  }


  ngAfterContentInit() {


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
    this.tabber.selectTab(idx);
  }

  findTabIndexByMenu(): number {
    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);

    for (let i = 0; i < tempMenuArray.length; i++) {
      if (tempMenuArray[i].title == this.menuService.selectedMenu.title)
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
    if (this.menuService.selectedMenu) {
      let array = this.utilsService.toArray(this.menuService.selectedMenu.Menu);
      let newArray = [];
      for (let i = 0; i < array.length; i++) {
        if (this.tileIsVisible(array[i]))
          newArray.push(array[i]);
      }
      return newArray;
    }

  }

  //---------------------------------------------------------------------------------------------
  ifTileHasObjects(tile) {
    if (tile == undefined || tile.Object == undefined)
      return false;

    var array = this.utilsService.toArray(tile.Object);
    return array.length > 0
  }

  tileIsVisible(tile) {
    return this.ifTileHasObjects(tile);
  }
}