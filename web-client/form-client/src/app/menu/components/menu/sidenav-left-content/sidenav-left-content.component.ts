import { UtilsService } from './../../../../core/utils.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';

@Component({
 selector: 'tb-sidenav-left-content',
  templateUrl: './sidenav-left-content.component.html',
  styleUrls: ['./sidenav-left-content.component.css']
})
export class LeftSidenavComponent implements OnInit {

  constructor(
    private menuService:MenuService,
    private utilsService: UtilsService
  ) { }

  ngOnInit() {
  }
}
