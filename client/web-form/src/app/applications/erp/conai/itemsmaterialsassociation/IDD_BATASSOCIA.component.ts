import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BATASSOCIAService } from './IDD_BATASSOCIA.service';

@Component({
    selector: 'tb-IDD_BATASSOCIA',
    templateUrl: './IDD_BATASSOCIA.component.html',
    providers: [IDD_BATASSOCIAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BATASSOCIAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BATASSOCIAService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','Material','Type','PrimaryPackage','ConsiderAsImporter','UseWeight','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BATASSOCIAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BATASSOCIAComponent, resolver);
    }
} 