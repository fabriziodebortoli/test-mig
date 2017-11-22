import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STRATEGYVIEWERTESTERService } from './IDD_STRATEGYVIEWERTESTER.service';

@Component({
    selector: 'tb-IDD_STRATEGYVIEWERTESTER',
    templateUrl: './IDD_STRATEGYVIEWERTESTER.component.html',
    providers: [IDD_STRATEGYVIEWERTESTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_STRATEGYVIEWERTESTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STRATEGYVIEWERTESTERService,
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
		boService.appendToModelStructure({});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STRATEGYVIEWERTESTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STRATEGYVIEWERTESTERComponent, resolver);
    }
} 