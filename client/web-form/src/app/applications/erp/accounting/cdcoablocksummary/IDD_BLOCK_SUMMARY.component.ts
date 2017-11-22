import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BLOCK_SUMMARYService } from './IDD_BLOCK_SUMMARY.service';

@Component({
    selector: 'tb-IDD_BLOCK_SUMMARY',
    templateUrl: './IDD_BLOCK_SUMMARY.component.html',
    providers: [IDD_BLOCK_SUMMARYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BLOCK_SUMMARYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BLOCK_SUMMARYService,
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
		boService.appendToModelStructure({'global':['Date','Template','TemplateDescr','Reason','ReasonDescr','AccountSummary','AccountSummaryDescr','AccountChild','AccountChildDescr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BLOCK_SUMMARYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BLOCK_SUMMARYComponent, resolver);
    }
} 