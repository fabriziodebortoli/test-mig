import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';
@Component({
  selector: 'tb-menu-selector',
  templateUrl: './menu-selector.component.html',
  styleUrls: ['./menu-selector.component.css']
})
export class MenuSelectorComponent implements OnInit {

  constructor(private menuService: MenuService, private utilService: UtilsService) {
  }

  ngOnInit() {
  }

  changeTab = function (tab) {
    this.menuService.setSelectedMenu(tab);
  }
}