import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INV3X_MIGRATION_REASONSService } from './IDD_INV3X_MIGRATION_REASONS.service';

@Component({
    selector: 'tb-IDD_INV3X_MIGRATION_REASONS',
    templateUrl: './IDD_INV3X_MIGRATION_REASONS.component.html',
    providers: [IDD_INV3X_MIGRATION_REASONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INV3X_MIGRATION_REASONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INV3X_MIGRATION_REASONSService,
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
		boService.appendToModelStructure({'global':['bAllReasons','bReasonSel','FromReason','ToReason','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INV3X_MIGRATION_REASONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INV3X_MIGRATION_REASONSComponent, resolver);
    }
} 