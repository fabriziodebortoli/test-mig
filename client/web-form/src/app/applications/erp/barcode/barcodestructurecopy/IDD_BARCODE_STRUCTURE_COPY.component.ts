import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BARCODE_STRUCTURE_COPYService } from './IDD_BARCODE_STRUCTURE_COPY.service';

@Component({
    selector: 'tb-IDD_BARCODE_STRUCTURE_COPY',
    templateUrl: './IDD_BARCODE_STRUCTURE_COPY.component.html',
    providers: [IDD_BARCODE_STRUCTURE_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BARCODE_STRUCTURE_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BARCODE_STRUCTURE_COPYService,
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
		boService.appendToModelStructure({'global':['Code']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BARCODE_STRUCTURE_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BARCODE_STRUCTURE_COPYComponent, resolver);
    }
} 