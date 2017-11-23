import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DMSCATEGORIESService } from './IDD_DMSCATEGORIES.service';

@Component({
    selector: 'tb-IDD_DMSCATEGORIES',
    templateUrl: './IDD_DMSCATEGORIES.component.html',
    providers: [IDD_DMSCATEGORIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DMSCATEGORIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DMSCATEGORIESService,
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
        
        		this.bo.appendToModelStructure({'global':['bTreeView','Name','Description','bDisabled','Values','CategoryInUseMsg','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Values':['VValue','VIsDefault']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DMSCATEGORIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DMSCATEGORIESComponent, resolver);
    }
} 