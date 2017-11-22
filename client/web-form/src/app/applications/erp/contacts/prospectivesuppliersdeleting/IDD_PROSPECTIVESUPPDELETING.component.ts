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
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllProspSupp','ProspSuppSel','FromProspSupp','ToProspSupp','AllConv','NotConv','Conv','AllConversionDate','ConversionDateSel','FromConversionDate','ToConversionDate','Enabled','Disabled','Both','nCurrentElement','GaugeDescription']});

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