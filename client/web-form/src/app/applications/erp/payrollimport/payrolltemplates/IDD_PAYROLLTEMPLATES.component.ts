import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYROLLTEMPLATESService } from './IDD_PAYROLLTEMPLATES.service';

@Component({
    selector: 'tb-IDD_PAYROLLTEMPLATES',
    templateUrl: './IDD_PAYROLLTEMPLATES.component.html',
    providers: [IDD_PAYROLLTEMPLATESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PAYROLLTEMPLATESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PAYROLLTEMPLATES_BE_REASON_itemSource: any;

    constructor(document: IDD_PAYROLLTEMPLATESService,
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
        this.IDC_PAYROLLTEMPLATES_BE_REASON_itemSource = {
  "name": "StrReasonCombo",
  "namespace": "ERP.PayrollImport.Documents.StrReasonCombo"
}; 

        		this.bo.appendToModelStructure({'PayrollTemplates':['Template','Description','AccTpl','ValidityStartingDate','ValidityEndingDate','Priority','Nature'],'HKLAccTpl':['Description'],'global':['PayrollTemplatesDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PayrollTemplatesDetails':['Line','Reason','AccRsn'],'HKLBodyReason':['Description'],'HKLBodyAccRsn':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYROLLTEMPLATESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYROLLTEMPLATESComponent, resolver);
    }
} 