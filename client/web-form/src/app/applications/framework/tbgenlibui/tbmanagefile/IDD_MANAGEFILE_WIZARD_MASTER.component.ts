import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { ComponentService, EventDataService, BOSlaveComponent, ControlComponent, ComponentInfoService, BOService  } from '@taskbuilder/core';

@Component({
    selector: 'tb-IDD_MANAGEFILE_WIZARD_MASTER',
    templateUrl: './IDD_MANAGEFILE_WIZARD_MASTER.component.html',
    providers: [ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MANAGEFILE_WIZARD_MASTERComponent extends BOSlaveComponent implements OnInit, OnDestroy {
     
	constructor(eventData: EventDataService,
		ciService: ComponentInfoService,
		changeDetectorRef: ChangeDetectorRef) {
        super(eventData, ciService, changeDetectorRef);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        const boService = this.document as BOService;
		boService.appendToModelStructure({});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
        
    }

}

@Component({
    template: ''
})
export class IDD_MANAGEFILE_WIZARD_MASTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MANAGEFILE_WIZARD_MASTERComponent, resolver);
    }
} 