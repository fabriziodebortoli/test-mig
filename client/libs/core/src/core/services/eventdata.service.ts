import { Injectable, EventEmitter, OnDestroy } from '@angular/core';

import { MessageDlgArgs, DiagnosticData, MessageDlgResult, DiagnosticDlgResult } from './../../shared/models/message-dialog.model';
import { CommandEventArgs } from './../../shared/models/eventargs.model';
import { ComponentInfo } from './../../shared/models/component-info.model';
import { BehaviorSubject } from './../../rxjs.imports';

@Injectable()
export class EventDataService implements OnDestroy {
    public command: EventEmitter<CommandEventArgs> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
    public openDropdown: EventEmitter<any> = new EventEmitter();
    public checkListBoxAction: EventEmitter<any> = new EventEmitter();
    public radarRecordSelected: EventEmitter<any> = new EventEmitter();
    public behaviours: EventEmitter<any> = new EventEmitter();
    public openRadar: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public controlCommand: EventEmitter<string> = new EventEmitter();

    public get showRadar() { return this.openRadar; }

    public openMessageDialog: EventEmitter<MessageDlgArgs> = new EventEmitter();
    public openDiagnosticDialog: EventEmitter<DiagnosticData> = new EventEmitter();
    public openDynamicDialog: EventEmitter<ComponentInfo> = new EventEmitter();
    public closeMessageDialog: EventEmitter<MessageDlgResult> = new EventEmitter();
    public closeDiagnosticDialog: EventEmitter<DiagnosticDlgResult> = new EventEmitter();

    public activationChanged: EventEmitter<string> = new EventEmitter();

    public oldModel: any = {}; // model before user changes (I need it for delta construction)
    public model: any = {}; // current model

    public activation: any = {}; // contains activation data
    public buttonsState: any = {};

    constructor() { }

    public raiseCommand(componentId: string, commandId: string, controlId: string = '') {
        const evt = new CommandEventArgs();
        evt.commandId = commandId;
        evt.componentId = componentId;
        evt.controlId = controlId;
        this.command.emit(evt);
    }
    public raiseControlCommand(controlId: string) {
        this.controlCommand.emit(controlId);
    }
    ngOnDestroy() {
        this.change.complete();
        this.openRadar.complete();
        this.radarRecordSelected.complete();
    }
}

