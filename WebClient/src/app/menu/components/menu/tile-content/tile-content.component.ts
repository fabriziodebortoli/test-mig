import { Component, Input, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-tile-content',
  templateUrl: './tile-content.component.html',
  styleUrls: ['./tile-content.component.css']
})
export class TileContentComponent implements OnInit {

   constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService
  ) {
  }
  ngOnInit() {
  }


  private menu: any ;
    get Menu(): any {
        return this.menu;
    }
    
    @Input()
    set Menu(menu: any) {
        this.menu = menu;
    }

  getFavoriteClass  (object) {
    return object.isFavorite ? 'favorite' : 'unfavorite';
  }
}
