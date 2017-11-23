import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CTGPRO_HEADER_FULLService } from './IDD_CTGPRO_HEADER_FULL.service';

@Component({
    selector: 'tb-IDD_CTGPRO_HEADER_FULL',
    templateUrl: './IDD_CTGPRO_HEADER_FULL.component.html',
    providers: [IDD_CTGPRO_HEADER_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CTGPRO_HEADER_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CTGPRO_HEADER_FULLService,
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
        
        		this.bo.appendToModelStructure({'CommisionCategories':['Category','Description','Disabled'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CTGPRO_HEADER_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CTGPRO_HEADER_FULLComponent, resolver);
    }
} 