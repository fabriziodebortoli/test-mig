import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_CONTACTSService } from './IDD_DELETE_CONTACTS.service';

@Component({
    selector: 'tb-IDD_DELETE_CONTACTS',
    templateUrl: './IDD_DELETE_CONTACTS.component.html',
    providers: [IDD_DELETE_CONTACTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DELETE_CONTACTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_CONTACTSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllContact','ContactSel','FromContact','ToContact','AllConv','NotConv','Conv','AllConversionDate','ConversionDateSel','FromConversionDate','ToConversionDate','Enabled','Disabled','Both','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_CONTACTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_CONTACTSComponent, resolver);
    }
} 