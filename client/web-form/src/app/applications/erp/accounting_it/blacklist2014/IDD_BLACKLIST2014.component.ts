import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BLACKLIST2014Service } from './IDD_BLACKLIST2014.service';

@Component({
    selector: 'tb-IDD_BLACKLIST2014',
    templateUrl: './IDD_BLACKLIST2014.component.html',
    providers: [IDD_BLACKLIST2014Service, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BLACKLIST2014Component extends BOComponent implements OnInit, OnDestroy {
     public IDC_BLACKLIST2014_HEIR_CODE_itemSource: any;

    constructor(document: IDD_BLACKLIST2014Service,
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
        this.IDC_BLACKLIST2014_HEIR_CODE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','Month','Quarter','LimitBlackList','FileNameComplete','Year','Month','SM_RevCharge','FileNameComplete','Intermediary','CAF','CommitDate','MadeTaxpayer','Heir_FiscalCode','Heir_Code_XML','Heir_From','Heir_To','Sostitutive','Cancellation','nCurrentElement','ProtocolTel','ProtocolDoc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BLACKLIST2014FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BLACKLIST2014Component, resolver);
    }
} 