import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ASLET770_FEESService } from './IDD_ASLET770_FEES.service';

@Component({
    selector: 'tb-IDD_ASLET770_FEES',
    templateUrl: './IDD_ASLET770_FEES.component.html',
    providers: [IDD_ASLET770_FEESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ASLET770_FEESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ASLET770_FEESService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bFrame','bLetter','bAnyFee','bOnlyPeriod','FromMonth','ToMonth','BigStateProc','MovementCounter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ASLET770_FEESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ASLET770_FEESComponent, resolver);
    }
} 