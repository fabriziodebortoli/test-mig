import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';
import { Component, OnInit } from '@angular/core';
@Component({
  selector: 'tb-menu-selector',
  templateUrl: './menu-selector.component.html',
  styleUrls: ['./menu-selector.component.css']
})
export class MenuSelectorComponent implements OnInit {

  constructor(private menuService: MenuService, private utilsService: UtilsService, private settingsService: SettingsService) {
  }

  ngOnInit() {
    this.initTab();
  }

  initTab() {
    if (this.menuService.selectedGroup == undefined)
      return;

    var tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);

    var found = false;
    for (var i = 0; i < tempMenuArray.length; i++) {
      if (tempMenuArray[i].name.toLowerCase() == this.settingsService.lastMenuName.toLowerCase()) {
        this.menuService.setSelectedMenu(tempMenuArray[i]);
        return;
      }
    }

    if (!found) {
      this.menuService.setSelectedMenu(tempMenuArray[0]);
    }

  }

  changeTab = function (tab) {
    this.menuService.setSelectedMenu(tab);
  }
}