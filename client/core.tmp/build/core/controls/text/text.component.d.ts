import { ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { ControlComponent } from '../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class TextComponent extends ControlComponent {
    private eventData;
    private vcr;
    private componentResolver;
    readonly: boolean;
    hotLink: any;
    width: number;
    contextMenu: ViewContainerRef;
    constructor(eventData: EventDataService, vcr: ViewContainerRef, componentResolver: ComponentFactoryResolver);
    onBlur(): void;
}
