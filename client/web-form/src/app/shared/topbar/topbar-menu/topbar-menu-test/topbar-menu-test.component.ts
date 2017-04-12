import { Component, OnInit } from '@angular/core';

import { ComponentService } from './../../../../core/component.service';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.css']
})
export class TopbarMenuTestComponent implements OnInit {

  private title: string = "Test menu";

  constructor(private componentService: ComponentService) { }

  ngOnInit() {
  }

  openDataService() {
    this.componentService.createComponentFromUrl('test/dataservice');
  }

  openRS() {
    this.componentService.createComponentFromUrl('rs/reportingstudio/');
  }

  openTBExplorer() {
    this.componentService.createComponentFromUrl('test/explorer');
  }

  openTestGrid() {
    this.componentService.createComponentFromUrl('proxy/test/grid');
  }

}
