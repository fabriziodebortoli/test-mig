import { LocalizationService } from './../../../services/localization.service';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { SettingsService } from './../../../services/settings.service';
import { Component, Input, OnInit } from '@angular/core';
@Component({
  selector: 'tb-menu-selector',
  templateUrl: './menu-selector.component.html',
  styleUrls: ['./menu-selector.component.css']
})
export class MenuSelectorComponent implements OnInit {

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

  changeTabByIndex(index) {
    if (index < 0)
      return;

    let tempMenuArray = this.utilsService.toArray(this.menuService.selectedGroup.Menu);
    let tab = tempMenuArray[index];
    if (tab != undefined)
      this.menuService.setSelectedMenu(tab);
  }
}