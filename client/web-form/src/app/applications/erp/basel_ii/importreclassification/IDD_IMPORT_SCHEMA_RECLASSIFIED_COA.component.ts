import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService } from './IDD_IMPORT_SCHEMA_RECLASSIFIED_COA.service';

@Component({
    selector: 'tb-IDD_IMPORT_SCHEMA_RECLASSIFIED_COA',
    templateUrl: './IDD_IMPORT_SCHEMA_RECLASSIFIED_COA.component.html',
    providers: [IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMPORT_SCHEMA_RECLASSIFIED_COAService,
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
		boService.appendToModelStructure({'global':['Schema']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent, resolver);
    }
} 