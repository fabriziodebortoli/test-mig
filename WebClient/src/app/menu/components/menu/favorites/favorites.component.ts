import { Component, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { ImageService } from './../../../services/image.service';
@Component({
  selector: 'tb-favorites',
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.css']
})
export class FavoritesComponent implements OnInit {

  private menuService: MenuService;
  constructor(public menuServiceTemp: MenuService, private utilService: UtilsService) {
    this.menuService = menuServiceTemp;
  }

  ngOnInit() {
  }

}

