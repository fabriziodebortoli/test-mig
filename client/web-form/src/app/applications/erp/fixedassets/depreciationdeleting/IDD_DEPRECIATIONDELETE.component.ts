import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEPRECIATIONDELETEService } from './IDD_DEPRECIATIONDELETE.service';

@Component({
    selector: 'tb-IDD_DEPRECIATIONDELETE',
    templateUrl: './IDD_DEPRECIATIONDELETE.component.html',
    providers: [IDD_DEPRECIATIONDELETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DEPRECIATIONDELETEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEPRECIATIONDELETEService,
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
		boService.appendToModelStructure({'global':['FromDate','ToDate','FiscalYearDescri','PostDate','Reason','RsnDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEPRECIATIONDELETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEPRECIATIONDELETEComponent, resolver);
    }
} 