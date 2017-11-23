import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DISPOSALENTRYService } from './IDD_DISPOSALENTRY.service';

@Component({
    selector: 'tb-IDD_DISPOSALENTRY',
    templateUrl: './IDD_DISPOSALENTRY.component.html',
    providers: [IDD_DISPOSALENTRYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DISPOSALENTRYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DISPOSALENTRYService,
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
        
        		this.bo.appendToModelStructure({'global':['FromCode','ToCode','FromCtg','ToCtg','ParentType','Parent','FromClass','ToClass','FromLoc','ToLoc','PurchDate','Supplier','DisposalType','PostDate','DocDate','NrDoc','Customer','Accntng','Detail','StatusDescri'],'HKLCustomer':['CompanyName'],'Detail':['l_TEnhFixAssetDisposal_P01','l_TEnhFixAssetDisposal_P34','CodeType','FixedAsset','Description','l_TEnhFixAssetDisposal_P04','l_TEnhFixAssetDisposal_P35','l_TEnhFixAssetDisposal_P02','l_TEnhFixAssetDisposal_P05','l_TEnhFixAssetDisposal_P09','l_TEnhFixAssetDisposal_P06','l_TEnhFixAssetDisposal_P07','l_TEnhFixAssetDisposal_P10','l_TEnhFixAssetDisposal_P11','l_TEnhFixAssetDisposal_P25','l_TEnhFixAssetDisposal_P31','l_TEnhFixAssetDisposal_P27','l_TEnhFixAssetDisposal_P08','l_TEnhFixAssetDisposal_P03','l_TEnhFixAssetDisposal_P18','l_TEnhFixAssetDisposal_P19','l_TEnhFixAssetDisposal_P29','l_TEnhFixAssetDisposal_P20','l_TEnhFixAssetDisposal_P24','Notes','l_TEnhFixAssetDisposal_P33']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DISPOSALENTRYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DISPOSALENTRYComponent, resolver);
    }
} 