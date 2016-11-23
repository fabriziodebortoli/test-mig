import { MenuService } from './../../services/menu.service';
import { UtilsService, HttpService } from 'tb-core';
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
  constructor(private httpService: HttpService, private menuService: MenuService, private utilService: UtilsService) { }
  ngOnInit() {
    this.httpService.getMenuElements().subscribe(result => {
      this.menu = result.Root.ApplicationMenu.AppMenu;
      this.applications = this.utilService.toArray(result.Root.ApplicationMenu.AppMenu.Application);
    });
  }

  runDocument(ns: string) {
    this.httpService.runObject(new DocumentInfo(0, ns, this.utilService.generateGUID()));
  }
}
