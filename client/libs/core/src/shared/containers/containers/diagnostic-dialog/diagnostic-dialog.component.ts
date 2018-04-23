import { Component, OnInit, Type, Input, ChangeDetectorRef, EventEmitter, Output } from '@angular/core';

import { DiagnosticData, DiagnosticDlgResult, Message } from './../../models/message-dialog.model';
import { TbComponent } from './../../../shared/components/tb.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EventDataService } from './../../../core/services/eventdata.service';

@Component({
    selector: 'tb-diagnostic-dialog',
    templateUrl: './diagnostic-dialog.component.html',
    styleUrls: ['./diagnostic-dialog.component.scss']
})
export class DiagnosticDialogComponent extends TbComponent {
    @Output()
    public onClose = new EventEmitter();
    eventData: EventDataService;
    _opened = false;
    @Input()
    public set opened(value: boolean) {
        if (this._opened === value) {
            return;
        }
        this._opened = value;
        if (!value) {
            this.onClose.emit();
        }
    }

    public get opened(): boolean {
        return this._opened;
    }

    @Input() public data = new DiagnosticData();
    constructor(
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
    ) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

    public open(data: DiagnosticData, eventData?: EventDataService) {
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
    selector: 'tb-diagnostic-dialog-item',
    templateUrl: './diagnostic-item.component.html',
    styleUrls: ['./diagnostic-item.component.scss']
})
export class DiagnosticDialogItemComponent {

    constructor() { }
    @Input() public message: Message;
    @Input() public level: number = 0;
    public collapsed = false;
    toggle() {
        this.collapsed = !this.collapsed;
    }
    getBanner(): string {
        return this.message.text
            ? this.message.text
            : "Messages";
    }
}
