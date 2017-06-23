import { ViewContainerRef, OnInit, OnDestroy, ComponentRef } from '@angular/core';
import { ComponentInfo } from '../../shared/models/component.info';
import { ComponentService } from '../services/component.service';
import { DocumentComponent } from './document.component';
import { MessageDialogComponent, MessageDlgArgs } from '../containers/message-dialog/message-dialog.component';
export declare class DynamicCmpComponent implements OnInit, OnDestroy {
    private componentService;
    cmpRef: ComponentRef<DocumentComponent>;
    componentInfo: ComponentInfo;
    cmpContainer: ViewContainerRef;
    messageDialog: MessageDialogComponent;
    messageDialogOpenSubscription: any;
    constructor(componentService: ComponentService);
    ngOnInit(): void;
    createComponent(): void;
    ngOnDestroy(): void;
    openMessageDialog(mainCmpId: string, args: MessageDlgArgs): void;
}
