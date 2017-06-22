import { EventDataService } from './../../../../core/eventdata.service';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MenuItem } from './../../../context-menu/menu-item.model';
import { ComponentService } from '@taskbuilder/core';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.scss']
})
export class TopbarMenuTestComponent {

  menuElements: MenuItem[] = new Array<MenuItem>();

  constructor(private componentService: ComponentService, private eventDataService: EventDataService) {

    const item1 = new MenuItem('Data Service', 'idDataServiceButton', true, false);
    const item2 = new MenuItem('Reporting Studio', 'idReportingStudioButton', true, false);
    const item3 = new MenuItem('TB Explorer', 'idTBExplorerButton', true, false);
    const item4 = new MenuItem('Test Grid Component', 'idTBExplorerButton', true, false);
    const item5 = new MenuItem('Test Icons', 'idTestIconsButton', true, false);
    this.menuElements.push(item1, item2, item3, item4, item5);

    this.eventDataService.command.subscribe((cmpId: string) => {
            switch (cmpId) {
              case 'idDataServiceButton':
                return this.openDataService();
              case 'idReportingStudioButton':
                return this.openRS();
              case 'idTBExplorerButton':
                return this.openTBExplorer();
              case 'idTBExplorerButton':
                return this.openTestGrid();
              case 'idTestIconsButton':
                return this.openTestIcons();
              default:
                break;
            }
        });
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
