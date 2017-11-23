import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINTFILEFORBEOService } from './IDD_PRINTFILEFORBEO.service';

@Component({
    selector: 'tb-IDD_PRINTFILEFORBEO',
    templateUrl: './IDD_PRINTFILEFORBEO.component.html',
    providers: [IDD_PRINTFILEFORBEOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRINTFILEFORBEOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRINTFILEFORBEOService,
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
		boService.appendToModelStructure({'global':['FiscalYear','Month','Schema','FileName','AskNature','AllNatureChoose','NatureSelection'],'HKLSchema':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINTFILEFORBEOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINTFILEFORBEOComponent, resolver);
    }
} 