import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECALCULATERESERVEDORDEREDService } from './IDD_RECALCULATERESERVEDORDERED.service';

@Component({
    selector: 'tb-IDD_RECALCULATERESERVEDORDERED',
    templateUrl: './IDD_RECALCULATERESERVEDORDERED.component.html',
    providers: [IDD_RECALCULATERESERVEDORDEREDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_RECALCULATERESERVEDORDEREDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECALCULATERESERVEDORDEREDService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECALCULATERESERVEDORDEREDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECALCULATERESERVEDORDEREDComponent, resolver);
    }
} 