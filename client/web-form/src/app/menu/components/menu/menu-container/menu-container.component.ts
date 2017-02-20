import { TabberComponent } from './../../../../shared/containers/tabs/tabber/tabber.component';
import { LocalizationService } from './../../../services/localization.service';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';
import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.scss']
})
export class MenuContainerComponent implements OnInit, OnDestroy {
  private selectedMenuChangedSubscription;
  constructor(
    private menuService: MenuService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService
  ) {
  }

  @ViewChild('tabber') tabber;

  ngOnInit() {

    this.selectedMenuChangedSubscription = this.menuService.selectedMenuChanged.subscribe(() => {
      this.changeTabWhenMenuChanges();
    });

    this.initTab();
  }

  initTab() {
    if (this.menuService.selectedGroup == undefined) {
      return;
    }

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);

    let found = false;
    for (let i = 0; i < tempMenuArray.length; i++) {
      if (tempMenuArray[i].name.toLowerCase() == this.settingsService.lastMenuName.toLowerCase()) {
        this.menuService.setSelectedMenu(tempMenuArray[i]);
        return;
      }
    }

    if (!found) {
      this.menuService.setSelectedMenu(tempMenuArray[0]);
    }

  }

  changeTabWhenMenuChanges() {
    if (this.menuService.selectedMenu == undefined)
      return;

    let idx = this.findTabIndexByMenu();
    if (idx >= 0 && !this.tabber.tabs[idx].active)
      this.tabber.selectTab(this.tabber.tabs[idx]);
  }

  findTabIndexByMenu(): number {

    for (let i = 0; i < this.tabber.tabs.length; i++) {
      if (this.tabber.tabs[i].title == this.menuService.selectedMenu.title)
        return i;
    }
    return -1;
  }

  ngOnDestroy() {
    this.selectedMenuChangedSubscription.unsubscribe();
  }



  changeTabByIndex(index) {

    if (index < 0 || this.menuService.selectedGroup == undefined)
      return;

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);
    let tab = tempMenuArray[index];
    if (tab != undefined)
      this.menuService.setSelectedMenu(tab);
  }

  isGroupOpened(group) {
    if (group == undefined || this.menuService.selectedGroup == undefined)
      return false;

    return group.title == this.menuService.selectedGroup.title;
  }

  selectGroupAndMenu(group, menu) {
    if (this.menuService.selectedGroup != group)
      this.menuService.setSelectedGroup(group);

    this.menuService.setSelectedMenu(menu);
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