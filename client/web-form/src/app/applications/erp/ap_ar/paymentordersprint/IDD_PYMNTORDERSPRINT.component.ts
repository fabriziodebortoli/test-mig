import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PYMNTORDERSPRINTService } from './IDD_PYMNTORDERSPRINT.service';

@Component({
    selector: 'tb-IDD_PYMNTORDERSPRINT',
    templateUrl: './IDD_PYMNTORDERSPRINT.component.html',
    providers: [IDD_PYMNTORDERSPRINTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PYMNTORDERSPRINTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PYMNTORDERSPRINTService,
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
		boService.appendToModelStructure({'global':['AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','IssueDate','IssueBank','DefPrint','NoticeOfPayments'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PYMNTORDERSPRINTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PYMNTORDERSPRINTComponent, resolver);
    }
} 