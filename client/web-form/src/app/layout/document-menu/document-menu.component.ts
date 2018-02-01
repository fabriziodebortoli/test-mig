import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap, ContextMenuItem } from '@taskbuilder/core';

import { DocumentMenuService } from './document-menu.service';

@Component({
    selector: 'tb-document-menu',
    templateUrl: './document-menu.component.html',
    providers: [DocumentMenuService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentMenuComponent extends BOComponent implements OnInit, OnDestroy {

    public cm_IDC_QUOTATIONS_TST_QUOTATIONNO = [
        new ContextMenuItem('Prova 1', 'CMD_1'),
        new ContextMenuItem('Prova 1', 'CMD_2'),
        new ContextMenuItem('Prova 2', 'CMD_3'),
        new ContextMenuItem('Prova 3', 'CMD_4'),
        new ContextMenuItem('Prova 4', 'CMD_5')
    ]

    constructor(document: DocumentMenuService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef
    ) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "Test nuova toolbar" } };
    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class DocumentMenuFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(DocumentMenuComponent, resolver);
    }
}
