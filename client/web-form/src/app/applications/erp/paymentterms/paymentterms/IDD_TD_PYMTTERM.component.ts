import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_PYMTTERMService } from './IDD_TD_PYMTTERM.service';

@Component({
    selector: 'tb-IDD_TD_PYMTTERM',
    templateUrl: './IDD_TD_PYMTTERM.component.html',
    providers: [IDD_TD_PYMTTERMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TD_PYMTTERMComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PYMTTERM_COMBO_CODETYPEPYMTSCHEDULE_itemSource: any;
public IDC_TITLE_PYMTTERM_DUEDATETYPE_itemSource: any;

    constructor(document: IDD_TD_PYMTTERMService,
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
        this.IDC_PYMTTERM_COMBO_CODETYPEPYMTSCHEDULE_itemSource = {
  "name": "DueDateTypeEnumCombo",
  "namespace": "ERP.PaymentTerms.Documents.DueDateTypeEnumCombo"
}; 
this.IDC_TITLE_PYMTTERM_DUEDATETYPE_itemSource = {
  "name": "PercDueDateTypeEnumCombo",
  "namespace": "ERP.PaymentTerms.Documents.PercDueDateTypeEnumCombo"
}; 

        		this.bo.appendToModelStructure({'PaymentTerms':['Payment','Disabled','Description','InstallmentType','CreditCard','Notes','Offset','AtSight','BusinessYear','WorkingDays','TaxInstallment','CollectionCharges','DiscountFormula','PymtCash','IntrastatCollectionType','DueDateType','FixedDay','FixedDayRounding','ExcludedMonth1','ExcludedMonth2','DeferredDayMonth1','DeferredDayMonth2','DeferredDayMonth1Same','DeferredDayMonth2Same','NoOfInstallments','FirstPaymentDays','DaysBetweenInstallments','PercInstallment','SettingsOnPercInstallment'],'HKLCreditCard':['Description'],'HKLAccount':['Description'],'HKLPymtCash':['Description'],'global':['DeferredDayMonth1Next','DeferredDayMonth2Next','PercInstallment','Date','TotalAmount','TaxAmount','PymtSchedule','__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PercInstallment':['InstallmentNo','Days','Perc','InstallmentType','DueDateType','FixedDay','FixedDayRounding','l_TEnhPaymentTermsPercInst_P1'],'PymtSchedule':['l_TEnhPymtSchedule_P1','l_TEnhPymtSchedule_P2','l_TEnhPymtSchedule_P3','l_TEnhPymtSchedule_P4','l_TEnhPymtSchedule_P5'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_PYMTTERMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_PYMTTERMComponent, resolver);
    }
} 