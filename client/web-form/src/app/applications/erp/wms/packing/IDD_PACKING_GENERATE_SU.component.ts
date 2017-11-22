import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PACKING_GENERATE_SUService } from './IDD_PACKING_GENERATE_SU.service';

@Component({
    selector: 'tb-IDD_PACKING_GENERATE_SU',
    templateUrl: './IDD_PACKING_GENERATE_SU.component.html',
    providers: [IDD_PACKING_GENERATE_SUService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PACKING_GENERATE_SUComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PACKING_GENERATE_SUService,
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
		boService.appendToModelStructure({'global':['SUToGenerate','StorageUnitType','bPrintLabelSU'],'HKLWMStorageUnitType':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PACKING_GENERATE_SUFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PACKING_GENERATE_SUComponent, resolver);
    }
} 