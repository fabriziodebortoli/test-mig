import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';


import { ImageService } from './../../../services/image.service';
import { Component, OnInit } from '@angular/core';
import { DocumentInfo } from 'tb-shared';
@Component({
  selector: 'tb-tile-container',
  templateUrl: './tile-container.component.html',
  styleUrls: ['./tile-container.component.css']
})
export class TileContainerComponent implements OnInit {

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilService: UtilsService,
    private imageService: ImageService
  ) {
  }

  ngOnInit() {
  }

  runFunction = function (object) {
    this.httpMenuService.runObject(new DocumentInfo(0, object.target, this.utilService.generateGUID()));

  }

  getImageClass(object) {
    return this.imageService.getObjectIcon(object); //": !object.isLoading, 'loading': object.isLoading}
  }

}
