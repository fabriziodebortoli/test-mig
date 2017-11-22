import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RESOURCES_LAYOUTService } from './IDD_RESOURCES_LAYOUT.service';

@Component({
    selector: 'tb-IDD_RESOURCES_LAYOUT',
    templateUrl: './IDD_RESOURCES_LAYOUT.component.html',
    providers: [IDD_RESOURCES_LAYOUTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RESOURCES_LAYOUTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RESOURCES_LAYOUTService,
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
		boService.appendToModelStructure({'global':['ResourceType','Resource','AllResources','SelectResource','DisabledToo','DetailImage','DBTNodeDetail'],'DBTNodeDetail':['l_FieldName','l_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RESOURCES_LAYOUTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RESOURCES_LAYOUTComponent, resolver);
    }
} 