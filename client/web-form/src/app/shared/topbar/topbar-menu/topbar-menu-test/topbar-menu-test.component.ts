import { Component, OnInit,ViewEncapsulation } from '@angular/core';

import { ComponentService } from './../../../../core/component.service';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.css']
})
export class TopbarMenuTestComponent implements OnInit {

data: Array<any> = [{
        actionName: 'Data Service',
        click: (dataItem) => {
           this.openDataService();
        }
    }, {
        actionName: 'Reporting Studio',
        click: (dataItem) => {
           this.openRS();
        }
    }, {
        actionName: 'TB Explorer',
         click: (dataItem) => {
           this.openTBExplorer();
        }
    }, {
        actionName: 'Test Grid Component',
         click: (dataItem) => {
           this.openTestGrid();
        }
    }, {
        actionName: 'Test Icons',
        click: (dataItem) => {
           this.openTestIcons();
        }
    }];

  private title: string = "Test menu";

  constructor(private componentService: ComponentService) { }
  ngOnInit() {
  }

  openDataService() {
    this.componentService.createComponentFromUrl('test/dataservice', true);
  }

  openRS() {
    this.componentService.createReportComponent('', true);
  }

  openTBExplorer() {
    this.componentService.createComponentFromUrl('test/explorer', true);
  }

  openTestGrid() {
    this.componentService.createComponentFromUrl('test/grid', true);
  }

  openTestIcons() {
    this.componentService.createComponentFromUrl('test/icons', true);
  }

}
