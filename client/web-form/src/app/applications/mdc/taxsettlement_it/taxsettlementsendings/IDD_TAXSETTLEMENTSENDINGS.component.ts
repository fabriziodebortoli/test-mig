import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXSETTLEMENTSENDINGSService } from './IDD_TAXSETTLEMENTSENDINGS.service';

@Component({
    selector: 'tb-IDD_TAXSETTLEMENTSENDINGS',
    templateUrl: './IDD_TAXSETTLEMENTSENDINGS.component.html',
    providers: [IDD_TAXSETTLEMENTSENDINGSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXSETTLEMENTSENDINGSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXSETTLEMENTSENDINGSService,
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
        
        		this.bo.appendToModelStructure({'TaxSettlementSendings':['TaxSettlementSendingId','BalanceYear','Quarter','SendingDate','SendingStatus'],'global':['DBTEventViewer','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTEventViewer':['EventDate','Event_Type','Event_Description','Event_XML','Event_String1','Event_String2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXSETTLEMENTSENDINGSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXSETTLEMENTSENDINGSComponent, resolver);
    }
} 