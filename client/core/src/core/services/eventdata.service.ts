import { Injectable, EventEmitter } from '@angular/core';

import { MessageDlgArgs, DiagnosticData, MessageDlgResult, DiagnosticDlgResult } from './../../shared/models/message-dialog.model';
import { CommandEventArgs } from './../../shared/models/eventargs.model';
import { ComponentInfo } from './../../shared/models/component-info.model';

@Injectable()
export class EventDataService {

    public command: EventEmitter<CommandEventArgs> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
    public openDropdown: EventEmitter<any> = new EventEmitter();

    public radarInfos: EventEmitter<MessageDlgArgs> = new EventEmitter();
    public radarRecordSelected: EventEmitter<any> = new EventEmitter();
    public behaviours: EventEmitter<any> = new EventEmitter();

    public openMessageDialog: EventEmitter<MessageDlgArgs> = new EventEmitter();
    public openDiagnosticDialog: EventEmitter<DiagnosticData> = new EventEmitter();
    public openDynamicDialog: EventEmitter<ComponentInfo> = new EventEmitter();
    public closeMessageDialog: EventEmitter<MessageDlgResult> = new EventEmitter();
    public closeDiagnosticDialog: EventEmitter<DiagnosticDlgResult> = new EventEmitter();

    public oldModel: any = {}; // model before user changes (I need it for delta construction)
    public model: any = {}; // current model

    public activation: any = {}; // contains activation data
    public buttonsState: any = {};

    constructor() { }

    public raiseCommand(componentId: string, commandId: string) {
        const evt = new CommandEventArgs();
        evt.commandId = commandId;
        evt.componentId = componentId;
        this.command.emit(evt);
    }
}

