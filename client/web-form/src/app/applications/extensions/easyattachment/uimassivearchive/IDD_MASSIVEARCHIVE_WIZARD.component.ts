import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MASSIVEARCHIVE_WIZARDService } from './IDD_MASSIVEARCHIVE_WIZARD.service';

@Component({
    selector: 'tb-IDD_MASSIVEARCHIVE_WIZARD',
    templateUrl: './IDD_MASSIVEARCHIVE_WIZARD.component.html',
    providers: [IDD_MASSIVEARCHIVE_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MASSIVEARCHIVE_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MASSIVEARCHIVE_WIZARDService,
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
		boService.appendToModelStructure({'global':['bSplitFile','DBTArchivedFiles','ElaborationMessage','nCurrentElement'],'DBTArchivedFiles':['VResultBmp','VFileName','VMassiveAction','VArchivedDocID']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MASSIVEARCHIVE_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MASSIVEARCHIVE_WIZARDComponent, resolver);
    }
} 