import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_ROMANIEOService } from './IDD_BR_ROMANIEO.service';

@Component({
    selector: 'tb-IDD_BR_ROMANIEO',
    templateUrl: './IDD_BR_ROMANIEO.component.html',
    providers: [IDD_BR_ROMANIEOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_ROMANIEOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_ROMANIEOService,
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
		boService.appendToModelStructure({'DBTBRRomaneio':['RomaneioNo','RomaneioDate','Driver','Status','Tractor','TractorLicensePlate','TractorFuelType','Trailer','trailerLicensePlate','trailerFuelType','DepartureDate','DepartureKm','ArrivalDate','ArrivalKm'],'HKLWorkers':['NameComplete'],'global':['BRRomaneioDetail','BRRomaneioPausesDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'BRRomaneioDetail':['DeliveryOrder','ArrivalTime','DepartureTime','Series','Model','DocNo','CustSuppType','CustSupp','CustSuppCompanyName','CustSuppFederalState','CustSuppCity','DeliveryToCompanyName','DeliveryToFederalState','DeliveryToCity','GrossWeight','NetWeight','NoOfPacks','Event'],'BRRomaneioSummary':['OutboundNetWeight','InboundNetWeight','TheoreticalTotWeight','TotWeight','TotNoOfPacks','TotVolumM3','TotalKm','ShipOutwardVoyage','ShipBackWay','ShipTotCharges','GrossTime','NetTime'],'BRRomaneioPausesDetail':['StartTime','EndTime','TotalPauseTime','Reason'],'BRRomaneioNotes':['Notes'],'BRRomaneioExpenses':['Tolls','Meals','Overnight','Mainteinance','Fuel','TotalRefLiters','OtherCharges','StartingCash','EndingCash','EstimatedValue','ActualValue','Variation']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_ROMANIEOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_ROMANIEOComponent, resolver);
    }
} 