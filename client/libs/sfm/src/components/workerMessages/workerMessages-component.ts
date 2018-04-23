import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { CoreService } from './../../core/sfm-core.service';
import { MessagesService } from './../../core/sfm-message.service';

@Component({
    selector: 'workerMessages',
    templateUrl: './workerMessages-component.html',
    styleUrls: ['./workerMessages-component.scss']
})

export class workerMessagesComponent implements OnInit, OnDestroy {

    messagesList: any[] = [];
    subsMessages: any;
    messageTitle: string;

    worker: any;
    subsWorker: any;
    workerName: string;

    constructor(private coreService: CoreService,
        private messagesService: MessagesService) { }

    ngOnInit() {
        this.subsWorker = this.coreService.getWorker().subscribe(row => {
            this.worker = row;
            this.workerName = this.coreService.workerName;
        });
        this.subsMessages = this.messagesService.getMessages().subscribe(rows => {
            this.messagesList = rows;
            this.setTitle();
        });
    }

    ngOnDestroy(): void {
        this.subsMessages.unsubscribe();
        this.subsWorker.unsubscribe();
    }

    setTitle() {
        if (this.messagesList.length > 0) {
            if (this.messagesList.length > 1)
                this.messageTitle = this.messagesList.length + " Messages";
            else
                this.messageTitle = "1 Message";
        }
        else
            this.messageTitle = "No Messages";
    }
}


