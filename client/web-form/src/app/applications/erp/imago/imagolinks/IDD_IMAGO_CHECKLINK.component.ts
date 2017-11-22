import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMAGO_CHECKLINKService } from './IDD_IMAGO_CHECKLINK.service';

@Component({
    selector: 'tb-IDD_IMAGO_CHECKLINK',
    templateUrl: './IDD_IMAGO_CHECKLINK.component.html',
    providers: [IDD_IMAGO_CHECKLINKService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMAGO_CHECKLINKComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMAGO_CHECKLINKService,
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
export class IDD_IMAGO_CHECKLINKFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMAGO_CHECKLINKComponent, resolver);
    }
} 