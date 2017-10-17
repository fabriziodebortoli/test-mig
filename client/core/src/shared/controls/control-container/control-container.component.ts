import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';


@Component({
    selector: 'tb-control-container',
    templateUrl: './control-container.component.html',
    styleUrls: ['./control-container.component.scss']
})
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {

    @Input('readonly') readonly: boolean = false;
    @Input() public hotLink: any = undefined;

    @ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;
    // public  contextMenuRef;

    public mask: string = '';

    constructor(
        public eventData: EventDataService,
        public vcr: ViewContainerRef,
        public componentResolver: ComponentFactoryResolver,
        layoutService: LayoutService,
        tbComponentService: TbComponentService
    ) {
        super(layoutService, tbComponentService);
    }

    onBlur() {
        this.eventData.change.emit(this.cmpId);
        this.blur.emit(this);
    }

    ngAfterContentInit() {
    }
}
