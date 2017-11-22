import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GENERATEDETAILService } from './IDD_GENERATEDETAIL.service';

@Component({
    selector: 'tb-IDD_GENERATEDETAIL',
    templateUrl: './IDD_GENERATEDETAIL.component.html',
    providers: [IDD_GENERATEDETAILService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_GENERATEDETAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GENERATEDETAILService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['NoDetailDoc','DaysNoDoc','AccrualDateType','YearCommercialDoc','EndMonthDoc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GENERATEDETAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GENERATEDETAILComponent, resolver);
    }
} 