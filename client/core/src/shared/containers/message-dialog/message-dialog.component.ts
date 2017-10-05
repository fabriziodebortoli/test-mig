import { Component, OnInit } from '@angular/core';

import { MessageDlgArgs, MessageDlgResult } from './../../models/message-dialog.model';

import { EventDataService } from './../../../core/services/eventdata.service';

@Component({
    selector: 'tb-message-dialog',
    templateUrl: './message-dialog.component.html',
    styleUrls: ['./message-dialog.component.scss']
})
export class MessageDialogComponent implements OnInit {

    opened = false;
    args: MessageDlgArgs;
    eventData: EventDataService;
    constructor() { }

    ngOnInit() { }

    open(args: MessageDlgArgs, eventData?: EventDataService) {
        this.eventData = eventData;
        this.args = args;
        this.opened = true;
    }

    close(result: string) {
        const res = new MessageDlgResult();
        res[result] = true;
        this.opened = false;
        if (this.eventData) {
            this.eventData.closeMessageDialog.emit(res);
        }
    }

}
