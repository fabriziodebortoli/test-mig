import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { MessageDlgArgs, MessageDlgResult } from './../../models/message-dialog.model';

import { EventDataService } from './../../../core/services/eventdata.service';
import { TbComponent } from './../../../shared/components/tb.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';

@Component({
    selector: 'tb-message-dialog',
    templateUrl: './message-dialog.component.html',
    styleUrls: ['./message-dialog.component.scss']
})
export class MessageDialogComponent extends TbComponent {

    opened = false;
    args: MessageDlgArgs;
    eventData: EventDataService;
    constructor(
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

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
