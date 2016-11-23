import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-tile-container',
  templateUrl: './tile-container.component.html',
  styleUrls: ['./tile-container.component.css']
})
export class TileContainerComponent implements OnInit {

 constructor(private menuService: MenuService, private utilService: UtilsService) {
    }

  ngOnInit() {
  }

}
