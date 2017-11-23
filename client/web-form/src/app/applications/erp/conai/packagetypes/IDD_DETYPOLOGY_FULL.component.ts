import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DETYPOLOGY_FULLService } from './IDD_DETYPOLOGY_FULL.service';

@Component({
    selector: 'tb-IDD_DETYPOLOGY_FULL',
    templateUrl: './IDD_DETYPOLOGY_FULL.component.html',
    providers: [IDD_DETYPOLOGY_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DETYPOLOGY_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DETYPOLOGY_FULLService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'Materials':['Material','Offset','Disabled','Offset','Disabled','Description'],'global':['PackageTypes','UnitValue','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DETYPOLOGY_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DETYPOLOGY_FULLComponent, resolver);
    }
} 