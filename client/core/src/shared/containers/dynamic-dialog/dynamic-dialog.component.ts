import { WebSocketService } from './../../../core/services/websocket.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentComponent } from './../../components/document.component';
import { DocumentService } from './../../../core/services/document.service';
import { ComponentInfo } from './../../models/component-info.model';
import { Component, OnInit, Type, Input, ViewChild, ViewContainerRef, ComponentRef, OnDestroy } from '@angular/core';

@Component({
    selector: 'tb-dynamic-dialog',
    templateUrl: './dynamic-dialog.component.html',
    styleUrls: ['./dynamic-dialog.component.scss']
})
export class DynamicDialogComponent implements OnDestroy {

    opened = false;
    componentInfo: ComponentInfo;
    subscriptions = [];

    constructor(private webSocketService: WebSocketService) {


    }

    open(componentInfo: ComponentInfo) {
        this.componentInfo = componentInfo;
        this.opened = true;

        this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
            if (this.componentInfo && data && data.id && data.id === this.componentInfo.id) {
                this.opened = false;
                this.componentInfo = null;
                this.subscriptions.forEach(subs => subs.unsubscribe());
            }
        }));
    }

    close() {
        this.webSocketService.doClose(this.componentInfo.id);
    }

    ngOnDestroy(): void {
        throw new Error("Method not implemented.");
    }
}
