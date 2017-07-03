import { OnInit } from '@angular/core';
import { EventDataService } from './../../services/eventdata.service';
export declare class MessageDialogComponent implements OnInit {
    opened: boolean;
    args: MessageDlgArgs;
    eventData: EventDataService;
    constructor();
    ngOnInit(): void;
    open(args: MessageDlgArgs, eventData?: EventDataService): void;
    close(result: string): void;
}
export declare class MessageDlgArgs {
    cmpId: string;
    text: string;
    ok: boolean;
    cancel: boolean;
    yes: boolean;
    no: boolean;
    abort: boolean;
    ignore: boolean;
    retry: boolean;
    continue: boolean;
}
export declare class MessageDlgResult {
}
