import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BLANKCHECKSService } from './IDD_BLANKCHECKS.service';

@Component({
    selector: 'tb-IDD_BLANKCHECKS',
    templateUrl: './IDD_BLANKCHECKS.component.html',
    providers: [IDD_BLANKCHECKSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BLANKCHECKSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BLANKCHECKSService,
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
        
        		this.bo.appendToModelStructure({'global':['CompanyBank','CompanyBankCA','BillType','CheckNoFrom','Notes','CheckNoTo','ProcessStatus']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BLANKCHECKSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BLANKCHECKSComponent, resolver);
    }
} 