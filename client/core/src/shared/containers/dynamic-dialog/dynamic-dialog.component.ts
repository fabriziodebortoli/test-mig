import { ComponentService } from './../../../core/services/component.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DynamicCmpComponent } from './../../components/dynamic-cmp.component';
import { WebSocketService } from './../../../core/services/websocket.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentComponent } from './../../components/document.component';
import { DocumentService } from './../../../core/services/document.service';
import { ComponentInfo } from './../../models/component-info.model';
import { Component, OnInit, Type, Input, ViewChild, ViewContainerRef, ComponentRef, ChangeDetectorRef, forwardRef, ContentChildren, AfterContentInit, OnDestroy } from '@angular/core';
import { TbComponent } from './../../components/tb.component';

@Component({
    selector: 'tb-dynamic-dialog',
    templateUrl: './dynamic-dialog.component.html',
    styleUrls: ['./dynamic-dialog.component.scss']
})
export class DynamicDialogComponent extends TbComponent implements OnDestroy {

    opened = false;
    componentInfo: ComponentInfo;
    subscriptions = [];
    public title = "";

    constructor(
        public componentService: ComponentService,
        public webSocketService: WebSocketService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

    open(componentInfo: ComponentInfo) {
        this.componentInfo = componentInfo;
        this.opened = true;
        this.subscriptions.push(this.componentService.componentInfoCreated.subscribe(arg => {
            this.title = this.componentInfo.title;
        }));
        this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
            if (this.componentInfo && data && data.id && data.id === this.componentInfo.id) {
                this.opened = false;
                this.componentInfo = null;
                this.subscriptions.forEach(subs => subs.unsubscribe());
                this.subscriptions = [];
            }
        }));
    }

    close() {
        this.webSocketService.doClose(this.componentInfo.id);
    }
    ngOnDestroy() {
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }

}
