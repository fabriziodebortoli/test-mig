import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALService } from './IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCAL.service';

@Component({
    selector: 'tb-IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCAL',
    templateUrl: './IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCAL.component.html',
    providers: [IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALService,
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
		boService.appendToModelStructure({'global':['CompleteMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALComponent, resolver);
    }
} 