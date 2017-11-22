import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CALC_FISCALCODEService } from './IDD_CALC_FISCALCODE.service';

@Component({
    selector: 'tb-IDD_CALC_FISCALCODE',
    templateUrl: './IDD_CALC_FISCALCODE.component.html',
    providers: [IDD_CALC_FISCALCODEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CALC_FISCALCODEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CALC_FISCALCODEService,
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
		boService.appendToModelStructure({'global':['LastName','Name','Gender','DateOfBirth','ISOCountryCode','CityOfBirth','CadastralCode','FiscalCode','FiscalCodeCalculate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CALC_FISCALCODEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CALC_FISCALCODEComponent, resolver);
    }
} 