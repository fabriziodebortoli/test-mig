import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RETURNEDMATERIALService } from './IDD_RETURNEDMATERIAL.service';

@Component({
    selector: 'tb-IDD_RETURNEDMATERIAL',
    templateUrl: './IDD_RETURNEDMATERIAL.component.html',
    providers: [IDD_RETURNEDMATERIALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_RETURNEDMATERIALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RETURNEDMATERIALService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllItems','ItemsSel','FromItem','ToItem','Group','Reason','Detail'],'Detail':['l_BoLDetail_Selected','Supplier','DocNo','DocumentDate','Item','UoM','Qty','ReturnReason','l_BoLDetail_Generated','Description'],'HKLReturnReason':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RETURNEDMATERIALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RETURNEDMATERIALComponent, resolver);
    }
} 