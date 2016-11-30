import { MenuService  } from './../../services/menu.service';
import { HttpMenuService  } from './../../services/http-menu.service';
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
  private menuService: MenuService;
  constructor(private httpMenuService: HttpMenuService, private menuServiceTemp: MenuService, private utilService: UtilsService) { 
    this.menuService = menuServiceTemp;
  }
  ngOnInit() {
  
    this.httpMenuService.getMenuElements().subscribe(result => {
      this.menuService.applicationMenu = result.Root.ApplicationMenu.AppMenu;
      this.menuService.environmentMenu = result.Root.EnvironmentMenu.AppMenu;

        this.menuService.loadFavoriteObjects();
      
    });
  }

  runDocument(ns: string) {
    this.httpMenuService.runObject(new DocumentInfo(0, ns, this.utilService.generateGUID()));
  }

  getFavoritesCount (){
    return this.menuService.favoritesCount;
  }
}
