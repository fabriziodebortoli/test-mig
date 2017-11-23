import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONVPRECISIONTIMEService } from './IDD_CONVPRECISIONTIME.service';

@Component({
    selector: 'tb-IDD_CONVPRECISIONTIME',
    templateUrl: './IDD_CONVPRECISIONTIME.component.html',
    providers: [IDD_CONVPRECISIONTIMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CONVPRECISIONTIMEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CONVPRECISIONTIME_CURRENT_itemSource: any;
public IDC_CONVPRECISIONTIME_NEW_itemSource: any;

    constructor(document: IDD_CONVPRECISIONTIMEService,
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
        this.IDC_CONVPRECISIONTIME_CURRENT_itemSource = {
  "name": "lCurrentPrecisionIntCombo",
  "namespace": "ERP.Company.Services.PrecisionSecondParamCombo"
}; 
this.IDC_CONVPRECISIONTIME_NEW_itemSource = {
  "name": "lNewPrecisionIntCombo",
  "namespace": "ERP.Company.Services.PrecisionSecondParamCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['lCurrentPrecision','lNewPrecision','bMatRoundSel','bCuttSel','bSupSel','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONVPRECISIONTIMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONVPRECISIONTIMEComponent, resolver);
    }
} 