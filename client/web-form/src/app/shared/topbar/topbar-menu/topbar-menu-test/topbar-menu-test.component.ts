import { text } from './../../../../reporting-studio/reporting-studio.model';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { MenuItem } from './../../../context-menu/menu-item.model';
import { ComponentService } from './../../../../core/component.service';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.scss']
})
export class TopbarMenuTestComponent implements OnInit {

  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  contextMenu: MenuItem[] = new Array<MenuItem>();
  private show = false;
  private title: string = "Test menu";

  constructor(private componentService: ComponentService) {
    const item1 = new MenuItem('Data Service', 'idDataServiceButton', false, false);
    const item2 = new MenuItem('Reporting Studio', 'idReportingStudioButton', false, false);
    const item3 = new MenuItem('TB Explorer', 'idTBExplorerButton', false, false);
    const item4 = new MenuItem('Test Grid Component', 'idTBExplorerButton', false, false);
    const item5 = new MenuItem('Test Icons', 'idTestIconsButton', false, false);
    this.contextMenu.push(item1, item2, item3, item4, item5);
  }

  ngOnInit() {
  }

  chooseAction(buttonName: string) {
    switch (buttonName) {
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

  // public closePopup(): void {
  //     this.show = false;
  //   }
}
