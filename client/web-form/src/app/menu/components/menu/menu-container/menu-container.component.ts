import { LocalizationService } from './../../../services/localization.service';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';
import { Component, Input, OnInit } from '@angular/core';
@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.css']
})
export class MenuContainerComponent implements OnInit {

  constructor(private menuService: MenuService, private utilsService: UtilsService, private settingsService: SettingsService, private localizationService: LocalizationService) {
  }


  private group: any;
  get Group(): any {
    return this.group;
  }

  @Input()
  set Group(group: any) {
    this.group = group;
    this.initTab();
  }


  ngOnInit() {
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

  isGroupOpened(group) {
    if (group == undefined || this.menuService.getSelectedGroup() == undefined)
      return false;

    return group.title == this.menuService.getSelectedGroup().title;
  }

  selectGroupAndMenu(group, menu) {
    if (this.menuService.getSelectedGroup() != group)
      this.menuService.setSelectedGroup(group);

    this.menuService.setSelectedMenu(menu);
  }

  
  getTiles() {
    let array = this.utilsService.toArray(this.menuService.getSelectedMenu().Menu);
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