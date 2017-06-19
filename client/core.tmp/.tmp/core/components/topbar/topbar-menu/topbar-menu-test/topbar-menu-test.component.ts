import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { ContextMenuItem } from '../../../../../shared';
import { ComponentService } from '../../../../services/component.service';
import { EventDataService } from '../../../../services/eventdata.service';

@Component({
  selector: 'tb-topbar-menu-test',
  template: "<tb-topbar-menu-elements [menuElements]=\"menuElements\" [fontIcon]=\"'build'\" popupClass='popup'></tb-topbar-menu-elements>",
  styles: [""]
})
export class TopbarMenuTestComponent {

  menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();

  constructor(private componentService: ComponentService, private eventDataService: EventDataService) {

    const item1 = new ContextMenuItem('Data Service', 'idDataServiceButton', true, false);
    const item2 = new ContextMenuItem('Reporting Studio', 'idReportingStudioButton', true, false);
    const item3 = new ContextMenuItem('TB Explorer', 'idTBExplorerButton', true, false);
    const item4 = new ContextMenuItem('Test Grid Component', 'idTBExplorerButton', true, false);
    const item5 = new ContextMenuItem('Test Icons', 'idTestIconsButton', true, false);
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
