import { DiagnosticData } from './../../../core/services/websocket.service';
import { Component, OnInit } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';
import { MessageDlgArgs, MessageDlgResult } from "../../models";

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

    close(result: string) {
        this.opened = false;
        if (this.eventData) {
            this.eventData.closeMessageDialog.emit({});
        }
    }

}
