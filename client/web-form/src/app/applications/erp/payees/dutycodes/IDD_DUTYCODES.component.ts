import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DUTYCODESService } from './IDD_DUTYCODES.service';

@Component({
    selector: 'tb-IDD_DUTYCODES',
    templateUrl: './IDD_DUTYCODES.component.html',
    providers: [IDD_DUTYCODESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DUTYCODESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DUTYCODES_LETTER770_itemSource: any;

    constructor(document: IDD_DUTYCODESService,
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
        this.IDC_DUTYCODES_LETTER770_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.ReasonCU"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'ContributionRsn':['DutyType','Reason','Description','WithholdingTaxDebitForDuty','Form770Letter','NeedMonth'],'HKLPdWithholdingTaxDebit':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DUTYCODESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DUTYCODESComponent, resolver);
    }
} 