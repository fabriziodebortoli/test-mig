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
    private utilsService: UtilsService,
    private imageService: ImageService
  ) {
  }

  ngOnInit() {
  }


}
