import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SDDMANDATEService } from './IDD_SDDMANDATE.service';

@Component({
    selector: 'tb-IDD_SDDMANDATE',
    templateUrl: './IDD_SDDMANDATE.component.html',
    providers: [IDD_SDDMANDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SDDMANDATEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SDDMANDATE_TYPE_itemSource: any;

    constructor(document: IDD_SDDMANDATEService,
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
        this.IDC_SDDMANDATE_TYPE_itemSource = {
  "name": "TypeEnumComboSDD",
  "namespace": "ERP.AP_AR.Components.TypeEnumComboSDD"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'SDDMandate':['MandateCode','Draft','MandateType','MandateDate','DDMandate','UMRCode','MandateOneOff','MandateFirstDate','MandateLastDate','Notes','Printed','PrintDate','Customer','CustomerBank','CustomerCA','CustomerIBANIsManual','CustomerIBAN'],'HKLCustSupp':['CompNameComplete'],'HKLCustSuppBank':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SDDMANDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SDDMANDATEComponent, resolver);
    }
} 