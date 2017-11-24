import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_SALESPEOPLEService } from './IDD_PARAMETERS_SALESPEOPLE.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_SALESPEOPLE',
    templateUrl: './IDD_PARAMETERS_SALESPEOPLE.component.html',
    providers: [IDD_PARAMETERS_SALESPEOPLEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PARAMETERS_SALESPEOPLEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PARAMETERS_SALESPEOPLEService,
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
        
        		this.bo.appendToModelStructure({'global':['ENASARCOParameters','FIRROneFirm','FIRRMultiFirm','CustomerAllowance','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ENASARCOParameters':['FromAmount','ToAmount','Perc','PercSalePerson','Amount','Description'],'FIRROneFirm':['FromAmount','ToAmount','Perc','Amount','Description'],'FIRRMultiFirm':['FromAmount','ToAmount','Perc','Amount','Description'],'CustomerAllowance':['FromYear','ToYear','Perc','P1','MaxValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERS_SALESPEOPLEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_SALESPEOPLEComponent, resolver);
    }
} 