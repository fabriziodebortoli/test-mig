import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEM_CHOOSE_IMPORTService } from './IDD_ITEM_CHOOSE_IMPORT.service';

@Component({
    selector: 'tb-IDD_ITEM_CHOOSE_IMPORT',
    templateUrl: './IDD_ITEM_CHOOSE_IMPORT.component.html',
    providers: [IDD_ITEM_CHOOSE_IMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEM_CHOOSE_IMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEM_CHOOSE_IMPORTService,
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
		boService.appendToModelStructure({'global':['ItemsBookForImport'],'ItemsBookForImport':['Selected','Items','Description','QtyToImport']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEM_CHOOSE_IMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEM_CHOOSE_IMPORTComponent, resolver);
    }
} 