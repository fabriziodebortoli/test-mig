import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SEARCHBANKBRANCHService } from './IDD_SEARCHBANKBRANCH.service';

@Component({
    selector: 'tb-IDD_SEARCHBANKBRANCH',
    templateUrl: './IDD_SEARCHBANKBRANCH.component.html',
    providers: [IDD_SEARCHBANKBRANCHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SEARCHBANKBRANCHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SEARCHBANKBRANCHService,
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
		boService.appendToModelStructure({'global':['ABI','CAB','SearchBankBranch'],'SearchBankBranch':['ABI','CAB','Bank','Description','Address','City','ZIPCode','County','Counter','Swift']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SEARCHBANKBRANCHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SEARCHBANKBRANCHComponent, resolver);
    }
} 