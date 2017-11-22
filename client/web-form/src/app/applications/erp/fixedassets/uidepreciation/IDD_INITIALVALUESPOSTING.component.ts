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
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllCtgs','CtgSel','FromCtg','ToCtg','AllFA','FASel','FromFA','ToFA','Process']});

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