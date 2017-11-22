import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_ITEM_CFOPGROUPService } from './IDD_BR_ITEM_CFOPGROUP.service';

@Component({
    selector: 'tb-IDD_BR_ITEM_CFOPGROUP',
    templateUrl: './IDD_BR_ITEM_CFOPGROUP.component.html',
    providers: [IDD_BR_ITEM_CFOPGROUPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_ITEM_CFOPGROUPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_ITEM_CFOPGROUPService,
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
		boService.appendToModelStructure({'DBTBRCFOPGroup':['CFOPGroup','Description','Disabled'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_ITEM_CFOPGROUPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_ITEM_CFOPGROUPComponent, resolver);
    }
} 