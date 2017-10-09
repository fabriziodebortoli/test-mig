import { Component, OnInit, Type, Input } from '@angular/core';

import { DiagnosticData, DiagnosticDlgResult, Message } from './../../models/message-dialog.model';

import { EventDataService } from './../../../core/services/eventdata.service';

@Component({
    selector: 'tb-diagnostic-dialog',
    templateUrl: './diagnostic-dialog.component.html',
    styleUrls: ['./diagnostic-dialog.component.scss']
})
export class DiagnosticDialogComponent implements OnInit {

    opened = false;
    eventData: EventDataService;
    data: DiagnosticData;
    constructor() { }

    ngOnInit() { }

    open(data: DiagnosticData, eventData?: EventDataService) {
        this.eventData = eventData;
        this.opened = true;
        this.data = data;
    }

    close(ok: boolean) {
        this.opened = false;
        if (this.eventData) {
            const res = new DiagnosticDlgResult();
            res.ok = ok;
            this.eventData.closeDiagnosticDialog.emit(res);
        }
    }

}

@Component({
    selector: 'tb-diagnostic-item',
    templateUrl: './diagnostic-item.component.html',
    styleUrls: ['./diagnostic-item.component.scss']
})
export class DiagnosticItemComponent {

    constructor() { }
    @Input() message: Message;
    @Input() level: number = 0;
    collapsed = false;
    toggle() {
        this.collapsed = !this.collapsed;
    }
}
