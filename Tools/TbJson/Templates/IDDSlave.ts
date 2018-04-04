import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, EventDataService, BOSlaveComponent, ControlComponent, ComponentInfoService, BOService, ContextMenuItem } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

@Component({
    selector: 'tb-@@NAME@@',
    templateUrl: './@@NAME@@.component.html',
    providers: [ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class @@NAME@@Component extends BOSlaveComponent implements OnInit, OnDestroy {
     /*dichiarazione variabili*/
	constructor(eventData: EventDataService,
        ciService: ComponentInfoService,
        private store: Store,
		changeDetectorRef: ChangeDetectorRef) {
        super(eventData, ciService, changeDetectorRef);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        /*definizione variabili*/
        @@INITCODE@@
    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class @@NAME@@FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(@@NAME@@Component, resolver);
    }
} 