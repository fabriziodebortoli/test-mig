import { UtilsService } from './../../../../core/utils.service';
import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';
import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation } from '@angular/core';
import { MasonryOptions } from "angular2-masonry";

@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.scss'],
  encapsulation: ViewEncapsulation.None
})

export class MenuContainerComponent implements OnInit, OnDestroy {
  private selectedMenuChangedSubscription;
  private selectedGroupChangedSubscription;
  private tiles: any[];

  @ViewChild('tabber') tabber;

  constructor(
    private menuService: MenuService,
    private utilsService: UtilsService,
    private settingsService: SettingsService,
    private localizationService: LocalizationService
  ) {
  }

  public masonryOptions: MasonryOptions = {
    transitionDuration: '0.8s',
    columnWidth: '.brick-sizer',
    itemSelector: '.brick',
    percentPosition: true
  };

  ngOnInit() {

    this.selectedMenuChangedSubscription = this.menuService.selectedMenuChanged.subscribe(() => {
      this.changeTabWhenMenuChanges();
      this.tiles = this.getTiles();
    });

    this.selectedGroupChangedSubscription = this.menuService.selectedGroupChanged.subscribe(() => {
      this.initTab();
    });
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
    this.selectedMenuChangedSubscription.unsubscribe();
    this.selectedGroupChangedSubscription.unsubscribe();
  }

  changeTabByIndex(event) {
    let index = event.index;
    if (index < 0 || this.menuService.selectedGroup == undefined)
      return;

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);
    let tab = tempMenuArray[index];
    if (tab != undefined)
      this.menuService.setSelectedMenu(tab);
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