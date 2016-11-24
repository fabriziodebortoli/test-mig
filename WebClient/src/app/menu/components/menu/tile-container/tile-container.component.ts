import { UtilsService, HttpService } from 'tb-core';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';
import { DocumentInfo } from 'tb-shared';
@Component({
  selector: 'tb-tile-container',
  templateUrl: './tile-container.component.html',
  styleUrls: ['./tile-container.component.css']
})
export class TileContainerComponent implements OnInit {

 constructor(private httpService: HttpService, private menuService: MenuService, private utilService: UtilsService) {
    }

  ngOnInit() {
  }

runFunction = function(object)
{
  this.httpService.runObject(new DocumentInfo(0, object.target, this.utilService.generateGUID()));

  }

  getImageClass(object){
    return this.menuService.getObjectIcon(object); //": !object.isLoading, 'loading': object.isLoading}
  }

}
