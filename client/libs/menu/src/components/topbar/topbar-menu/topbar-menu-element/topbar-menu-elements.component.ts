import { ContextMenuItem, WebSocketService, EventDataService } from '@taskbuilder/core';
import { Component, Input, ViewChild, ElementRef, HostListener } from '@angular/core';

import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

@Component({
    selector: 'tb-topbar-menu-elements',
    templateUrl: './topbar-menu-elements.component.html',
    styleUrls: ['./topbar-menu-elements.component.scss']
})
export class TopbarMenuElementsComponent {
    show = false;

    @ViewChild('anchor') public anchor: ElementRef;
    @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

    @Input() fontIcon = 'tb-menu2';
    @Input() menuElements: ContextMenuItem[];

    constructor(public webSocketService: WebSocketService, public eventDataService: EventDataService) {
    }

    onOpen() {
    }

    public doCommand(menuItem: any) {
        if (!menuItem) {
            return;
        }

        this.eventDataService.raiseCommand('', menuItem.id);
        this.toggle();
    }

    @HostListener('document:click', ['$event'])
    public documentClick(event: any): void {
        if (!this.contains(event.target)) {
            this.toggle(false);
        }
    }

    @HostListener('keydown', ['$event'])
    public keydown(event: any): void {
        if (event.keyCode === 27) {
            this.toggle(false);
        }
    }

    private contains(target: any): boolean {
        return this.anchor.nativeElement.contains(target) ||
            (this.popup ? this.popup.nativeElement.contains(target) : false);
    }

    public toggle(show?: boolean): void {
        this.show = show !== undefined ? show : !this.show;
    }
}
