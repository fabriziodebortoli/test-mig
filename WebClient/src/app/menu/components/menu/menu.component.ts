import { MenuService } from './../../services/menu.service';
import { UtilsService } from './../../../core';
import { DocumentInfo } from './../../../shared';
import { HttpService } from './../../../core/';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})

export class MenuComponent implements OnInit {

  private menu: undefined;
  private applications: undefined;
  constructor(private httpService: HttpService, private menuService: MenuService, private utilService: UtilsService) { }
  ngOnInit() {
    this.httpService.getMenuElements().subscribe(result => {
      this.menuService.applicationMenu = result.Root.ApplicationMenu.AppMenu;
      this.menuService.environmentMenu = result.Root.EnvironmentMenu.AppMenu;
    });
  }

  runDocument(ns: string) {
    this.httpService.runObject(new DocumentInfo(0, ns, this.utilService.generateGUID()));
  }
}
