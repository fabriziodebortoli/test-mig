import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PROSPECTIVESUPPDELETINGService } from './IDD_PROSPECTIVESUPPDELETING.service';

@Component({
    selector: 'tb-IDD_PROSPECTIVESUPPDELETING',
    templateUrl: './IDD_PROSPECTIVESUPPDELETING.component.html',
    providers: [IDD_PROSPECTIVESUPPDELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PROSPECTIVESUPPDELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PROSPECTIVESUPPDELETINGService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        		this.bo.appendToModelStructure({'global':['AllProspSupp','ProspSuppSel','FromProspSupp','ToProspSupp','AllConv','NotConv','Conv','AllConversionDate','ConversionDateSel','FromConversionDate','ToConversionDate','Enabled','Disabled','Both','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PROSPECTIVESUPPDELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROSPECTIVESUPPDELETINGComponent, resolver);
    }
} 