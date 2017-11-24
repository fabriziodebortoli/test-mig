import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INITIALVALUESPOSTINGService } from './IDD_INITIALVALUESPOSTING.service';

@Component({
    selector: 'tb-IDD_INITIALVALUESPOSTING',
    templateUrl: './IDD_INITIALVALUESPOSTING.component.html',
    providers: [IDD_INITIALVALUESPOSTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INITIALVALUESPOSTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INITIALVALUESPOSTINGService,
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
        
        		this.bo.appendToModelStructure({'global':['AllCtgs','CtgSel','FromCtg','ToCtg','AllFA','FASel','FromFA','ToFA','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INITIALVALUESPOSTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INITIALVALUESPOSTINGComponent, resolver);
    }
} 