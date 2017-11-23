import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_OK_CANCELService } from './IDD_PD_OK_CANCEL.service';

@Component({
    selector: 'tb-IDD_PD_OK_CANCEL',
    templateUrl: './IDD_PD_OK_CANCEL.component.html',
    providers: [IDD_PD_OK_CANCELService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PD_OK_CANCELComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_OK_CANCELService,
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
		boService.appendToModelStructure({'global':['UpdateManagerbDeletion','UpdateManagerbCancellation','UpdateManagerAppDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_OK_CANCELFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_OK_CANCELComponent, resolver);
    }
} 