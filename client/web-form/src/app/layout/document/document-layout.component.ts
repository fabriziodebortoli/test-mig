import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { DocumentLayoutService } from './document-layout.service';

@Component({
    selector: 'tb-document-layout',
    templateUrl: './document-layout.component.html',
    providers: [DocumentLayoutService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentLayoutComponent extends BOComponent implements OnInit, OnDestroy {

    constructor(document: DocumentLayoutService,
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
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class DocumentLayoutFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(DocumentLayoutComponent, resolver);
    }
}
