import { Component, Input, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';
import { DocumentInfo } from 'tb-shared';

@Component({
  selector: 'tb-tile-content',
  templateUrl: './tile-content.component.html',
  styleUrls: ['./tile-content.component.css']
})
export class TileContentComponent implements OnInit {

   constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilService: UtilsService,
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



  runFunction = function (object) {
    this.httpMenuService.runObject(new DocumentInfo(0, object.target, this.utilService.generateGUID()));

  }

  getImageClass(object) {
    return this.imageService.getObjectIcon(object); //": !object.isLoading, 'loading': object.isLoading}
  }
}
