import { MenuService } from './../../services/menu.service';
import { HttpMenuService } from './../../services/http-menu.service';
import { UtilsService } from 'tb-core';
import { DocumentInfo } from 'tb-shared';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})

export class MenuComponent implements OnInit {

  private menu: undefined;
  private applications: undefined;

  constructor(private httpMenuService: HttpMenuService, private menuService: MenuService, private utilsService: UtilsService) {
  }
  ngOnInit() {

    this.httpMenuService.getMenuElements().subscribe(result => {
      this.menuService.applicationMenu = result.Root.ApplicationMenu.AppMenu;
      this.menuService.environmentMenu = result.Root.EnvironmentMenu.AppMenu;

      this.menuService.loadFavoriteObjects();
      this.menuService.LoadMostUsedObjects();

    });
  }

  runDocument(ns: string) {
    this.httpMenuService.runObject(new DocumentInfo(0, ns, this.utilsService.generateGUID()));
  }
}
