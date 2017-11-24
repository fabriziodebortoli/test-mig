import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECALENTRY_FIXEDASSETSService } from './IDD_RECALENTRY_FIXEDASSETS.service';

@Component({
    selector: 'tb-IDD_RECALENTRY_FIXEDASSETS',
    templateUrl: './IDD_RECALENTRY_FIXEDASSETS.component.html',
    providers: [IDD_RECALENTRY_FIXEDASSETSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_RECALENTRY_FIXEDASSETSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECALENTRY_FIXEDASSETSService,
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
        
        		this.bo.appendToModelStructure({'global':['bAnyFA','bSelFA','Type','FixedAsset','Type','FixedAsset','RecalculateInital','PurchaseFiscalYear','BigStateProc'],'HKLFixedAsset':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECALENTRY_FIXEDASSETSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECALENTRY_FIXEDASSETSComponent, resolver);
    }
} 