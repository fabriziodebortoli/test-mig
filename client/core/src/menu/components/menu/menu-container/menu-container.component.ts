import { Subscription } from "rxjs/Rx";
import { SettingsService } from './../../../../core/services/settings.service';
import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation, AfterViewInit, AfterContentInit, ViewContainerRef, ChangeDetectionStrategy } from '@angular/core';
import { UtilsService } from './../../../../core/services/utils.service';
import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.scss']
})

export class MenuContainerComponent implements AfterContentInit, OnDestroy {
  public subscriptions: Subscription[] = [];
  public selectedGroupChangedSubscription;
  public tiles: any[];

  @ViewChild('tabber') tabber;

  constructor(
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingsService: SettingsService
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
  }

  ngAfterContentInit() {
  }

  initTab() {

    if (this.menuService.selectedGroup == undefined) {
      return;
    }

    let found = false;
    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      for (let i = 0; i < tempMenuArray.length; i++) {
        if (tempMenuArray[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
          this.menuService.setSelectedMenu(tempMenuArray[i]);
          return;
        }
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
    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      for (let i = 0; i < tempMenuArray.length; i++) {
        if (tempMenuArray[i].title == this.menuService.selectedMenu.title)
          return i;
      }
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

    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      let tab = tempMenuArray[index];
      if (tab != undefined) {
        this.menuService.setSelectedMenu(tab);
      }
    }
  }

  getTiles() {
    if (this.menuService.selectedMenu) {
      let array = this.menuService.selectedMenu.Menu;
      let newArray = [];
      if (array) {
        for (let i = 0; i < array.length; i++) {
          if (this.tileIsVisible(array[i]) && !array[i].hiddenTile)
            newArray.push(array[i]);
        }
      }

      // //aggiunto per menù a tre livelli
      // let olstyleMenu = this.menuService.selectedGroup.Menu;
      // for (let i = 0; i < olstyleMenu.length; i++) {
      //   if (this.tileIsVisible(olstyleMenu[i]) && !olstyleMenu[i].hiddenTile)
      //     newArray.push(olstyleMenu[i]);
      // }
      return newArray;
    }

  }

  //---------------------------------------------------------------------------------------------
  ifTileHasObjects(tile) {
    if (tile == undefined || tile.Object == undefined)
      return false;

    return tile.Object.length > 0
  }

  tileIsVisible(tile) {
    return this.ifTileHasObjects(tile);
  }
}