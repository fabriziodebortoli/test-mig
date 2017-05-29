import { ImageService } from './../../menu/services/image.service';
import { MenuService } from './../../menu/services/menu.service';
import { UtilsService } from './../../core/utils.service';
import { Component, OnInit } from '@angular/core';

import { EventDataService } from './../../core/eventdata.service';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  providers: [EventDataService]
})
export class DashboardComponent implements OnInit {

  private favorites: Array<any> = [];

  constructor(
    private menuService: MenuService,
    private imageService: ImageService,
    private utilsService: UtilsService) { }

  ngOnInit() {
    this.favorites = this.menuService.getFavorites();
  }

  runFunction(object) {
    this.menuService.runFunction(object);
  }

}
