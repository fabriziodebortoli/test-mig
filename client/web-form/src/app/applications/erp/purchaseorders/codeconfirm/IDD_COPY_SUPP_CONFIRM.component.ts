import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COPY_SUPP_CONFIRMService } from './IDD_COPY_SUPP_CONFIRM.service';

@Component({
    selector: 'tb-IDD_COPY_SUPP_CONFIRM',
    templateUrl: './IDD_COPY_SUPP_CONFIRM.component.html',
    providers: [IDD_COPY_SUPP_CONFIRMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COPY_SUPP_CONFIRMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COPY_SUPP_CONFIRMService,
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
		boService.appendToModelStructure({'global':['Code']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COPY_SUPP_CONFIRMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COPY_SUPP_CONFIRMComponent, resolver);
    }
} 