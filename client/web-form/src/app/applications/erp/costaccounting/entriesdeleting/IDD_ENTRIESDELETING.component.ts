import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ENTRIESDELETINGService } from './IDD_ENTRIESDELETING.service';

@Component({
    selector: 'tb-IDD_ENTRIESDELETING',
    templateUrl: './IDD_ENTRIESDELETING.component.html',
    providers: [IDD_ENTRIESDELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ENTRIESDELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_RMCOSTACC_ENTRYTYPE_itemSource: any;

    constructor(document: IDD_ENTRIESDELETINGService,
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
        this.IDC_RMCOSTACC_ENTRYTYPE_itemSource = {
  "name": "EntryTypeForecastEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeForecastEnumCombo"
}; 

        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','bResetPostedInCostAccounting','All','TypeSel','EntryType','SelGen','strPostingType','ExtraSel','AllGen','JEAll','JESel','Pure','Issue','Received','Everyone','Nature','DeletedNo','NrRef','PostDate','Account','AccountDescri']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ENTRIESDELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ENTRIESDELETINGComponent, resolver);
    }
} 