import { TbComponent, ContextMenuItem, ComponentService, EventDataService, InfoService, TbComponentService, CommandEventArgs } from '@taskbuilder/core';
import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';

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
