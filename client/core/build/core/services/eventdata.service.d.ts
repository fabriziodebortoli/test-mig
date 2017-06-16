import { EventEmitter } from '@angular/core';
import { MessageDlgArgs, MessageDlgResult } from './../containers/message-dialog/message-dialog.component';
export declare class EventDataService {
    command: EventEmitter<string>;
    change: EventEmitter<string>;
    openDropdown: EventEmitter<any>;
    openMessageDialog: EventEmitter<MessageDlgArgs>;
    closeMessageDialog: EventEmitter<MessageDlgResult>;
    oldModel: any;
    model: any;
    activation: any;
    buttonsState: any;
    constructor();
}
