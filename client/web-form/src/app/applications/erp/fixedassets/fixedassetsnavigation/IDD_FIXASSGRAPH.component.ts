import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FIXASSGRAPHService } from './IDD_FIXASSGRAPH.service';

@Component({
    selector: 'tb-IDD_FIXASSGRAPH',
    templateUrl: './IDD_FIXASSGRAPH.component.html',
    providers: [IDD_FIXASSGRAPHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FIXASSGRAPHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FIXASSGRAPHService,
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
		boService.appendToModelStructure({'global':['AllCtgs','CtgSel','FromCtg','ToCtg','bDisabled','DBTNodeDetail'],'DBTNodeDetail':['l_FieldName','l_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FIXASSGRAPHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FIXASSGRAPHComponent, resolver);
    }
} 