import { Component, Input, OnInit } from '@angular/core';
import { UtilsService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { HttpMenuService } from './../../../services/http-menu.service';
import { ImageService } from './../../../services/image.service';


@Component({
  selector: 'tb-tile-element',
  templateUrl: './tile-element.component.html',
  styleUrls: ['./tile-element.component.css']
})
export class TileElementComponent implements OnInit {

  constructor(
    private httpMenuService: HttpMenuService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private imageService: ImageService
  ) {
  }

  ngOnInit() {
  }


  private object: any;
  get Object(): any {
    return this.object;
  }

  @Input()
  set Object(object: any) {
    this.object = object;
  }
}
