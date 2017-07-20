import { DiagnosticData, DiagnosticDlgResult } from './../../shared/models';
import { Injectable, EventEmitter } from '@angular/core';

import { MessageDlgArgs, MessageDlgResult } from './../../shared/models';

@Injectable()
export class EventDataService {

    public command: EventEmitter<string> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
    public openDropdown: EventEmitter<any> = new EventEmitter();

    public radarInfos: EventEmitter<MessageDlgArgs> = new EventEmitter();

    public openMessageDialog: EventEmitter<MessageDlgArgs> = new EventEmitter();
    public openDiagnosticDialog: EventEmitter<DiagnosticData> = new EventEmitter();
    public closeMessageDialog: EventEmitter<MessageDlgResult> = new EventEmitter();
    public closeDiagnosticDialog: EventEmitter<DiagnosticDlgResult> = new EventEmitter();

    public oldModel: any = {}; // model before user changes (I need it for delta construction)
    public model: any = {}; // current model

    public activation: any = {}; // contains activation data
    public buttonsState: any = {}; // contains activation data

    constructor() {
        console.log('EventDataService created');
    }

}

