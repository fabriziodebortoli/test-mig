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
export class TopbarMenuTestComponent extends TbComponent {

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

        this.eventDataService.command.subscribe((args: CommandEventArgs) => {
            switch (args.commandId) {
                case 'idTestLayout':
                    return this.openTestLayout();
                case 'idTestMenu':
                    return this.openTestMenu();
                default:
                    break;
            }
        });
    }
    onTranslationsReady() {
        super.onTranslationsReady();
        this.menuElements.splice(0, this.menuElements.length);
        const item8 = new ContextMenuItem(this._TB('Test Layout'), 'idTestLayout', true, false);
        const item9 = new ContextMenuItem(this._TB('Test New Toolbar'), 'idTestMenu', true, false);
        this.menuElements.push(item8, item9);
    }

    openRS() {
        this.componentService.createReportComponent('', true);
    }

    openTestLayout() {
        this.componentService.createComponentFromUrl('layout/document', true);
    }
    openTestMenu() {
        this.componentService.createComponentFromUrl('layout/document-menu', true);
    }
}
