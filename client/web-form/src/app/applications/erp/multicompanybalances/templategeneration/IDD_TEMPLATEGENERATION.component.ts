import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TEMPLATEGENERATIONService } from './IDD_TEMPLATEGENERATION.service';

@Component({
    selector: 'tb-IDD_TEMPLATEGENERATION',
    templateUrl: './IDD_TEMPLATEGENERATION.component.html',
    providers: [IDD_TEMPLATEGENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TEMPLATEGENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TEMPLATEGENERATIONService,
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
		boService.appendToModelStructure({'global':['Template','Description','AllAccount','AcconuntsSel','FromAccount','ToAccount','Level','CodeCopy','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TEMPLATEGENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TEMPLATEGENERATIONComponent, resolver);
    }
} 