import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';
import { CommandEventArgs } from './../../../../models/eventargs.model';
import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

import { EventDataService } from './../../../../../core/services/eventdata.service';
import { ComponentService } from './../../../../../core/services/component.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';
import { TbComponent } from './../../../../../shared/components/tb.component';

@Component({
    selector: 'tb-topbar-menu-test',
    templateUrl: './topbar-menu-test.component.html',
    styleUrls: ['./topbar-menu-test.component.scss']
})
export class TopbarMenuTestComponent  extends TbComponent{

    menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();

    constructor(
        public componentService: ComponentService,
        public eventDataService: EventDataService,
        public infoService: InfoService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();

        const item1 = new ContextMenuItem('Data Service', 'idDataServiceButton', true, false);
        const item2 = new ContextMenuItem('Reporting Studio', 'idReportingStudioButton', true, false);
        const item3 = new ContextMenuItem('TB Explorer', 'idTBExplorerButton', true, false);
        const item4 = new ContextMenuItem('Test Grid Component', 'idTBExplorerButton', true, false);
        const item5 = new ContextMenuItem('Test Icons', 'idTestIconsButton', true, false);
        const item6 = new ContextMenuItem('Test Radar', 'idTestRadar', true, false);
        const item7 = new ContextMenuItem('Test Tree', 'idTestTree', true, false);
        const item8 = new ContextMenuItem('Test Layout', 'idTestLayout', true, false);
        this.menuElements.push(item1, item2, item3, item4, item5, item6, item7, item8);
        

        this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
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
                case 'idTestRadar':
                    return this.openTestRadar();
                case 'idTestTree':
                    return this.openTestTree();
                case 'idTestLayout':
                    return this.openTestLayout();
                default:
                    break;
            }
        });
    }
    onTranslationsReady(){
        super.onTranslationsReady();
        this.menuElements.splice(0,this.menuElements.length);
        const item1 = new ContextMenuItem(this._TB('Data Service'), 'idDataServiceButton', true, false);
        const item2 = new ContextMenuItem(this._TB('Reporting Studio'), 'idReportingStudioButton', true, false);
        const item3 = new ContextMenuItem(this._TB('TB Explorer'), 'idTBExplorerButton', true, false);
        const item4 = new ContextMenuItem(this._TB('Test Grid Component'), 'idTBExplorerButton', true, false);
        const item5 = new ContextMenuItem(this._TB('Test Icons'), 'idTestIconsButton', true, false);
        const item6 = new ContextMenuItem(this._TB('Test Radar'), 'idTestRadar', true, false);
        const item7 = new ContextMenuItem(this._TB('Test Tree'), 'idTestTree', true, false);
        this.menuElements.push(item1, item2, item3, item4, item5, item6, item7);
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

    openTestRadar() {
        this.componentService.createComponentFromUrl('test/radar', true);
    }

    openTestTree() {
        this.componentService.createComponentFromUrl('test/tree', true);
    }

    openTestLayout() {
        this.componentService.createComponentFromUrl('layout/document', true);
    }
}
