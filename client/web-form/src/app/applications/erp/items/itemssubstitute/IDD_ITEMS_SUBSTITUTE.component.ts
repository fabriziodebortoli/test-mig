import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_SUBSTITUTEService } from './IDD_ITEMS_SUBSTITUTE.service';

@Component({
    selector: 'tb-IDD_ITEMS_SUBSTITUTE',
    templateUrl: './IDD_ITEMS_SUBSTITUTE.component.html',
    providers: [IDD_ITEMS_SUBSTITUTEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEMS_SUBSTITUTEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_SUBSTITUTEService,
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
        
        		this.bo.appendToModelStructure({'global':['Biuniqueness','SubstituteItems','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SubstituteItems':['Substitute','ItemQty','ItemUoM','SubstituteQty','SubsItemUoM','Notes'],'HKLItemsSubstitute':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_SUBSTITUTEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_SUBSTITUTEComponent, resolver);
    }
} 