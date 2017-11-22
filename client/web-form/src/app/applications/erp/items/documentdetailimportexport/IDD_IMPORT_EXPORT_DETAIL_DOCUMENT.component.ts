import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMPORT_EXPORT_DETAIL_DOCUMENTService } from './IDD_IMPORT_EXPORT_DETAIL_DOCUMENT.service';

@Component({
    selector: 'tb-IDD_IMPORT_EXPORT_DETAIL_DOCUMENT',
    templateUrl: './IDD_IMPORT_EXPORT_DETAIL_DOCUMENT.component.html',
    providers: [IDD_IMPORT_EXPORT_DETAIL_DOCUMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMPORT_EXPORT_DETAIL_DOCUMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMPORT_EXPORT_DETAIL_DOCUMENTService,
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
		boService.appendToModelStructure({'global':['ImportExportFileName','ImportExportnCurrentElement','ImportExportNotesFileName','ImportExportnCurrentElement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMPORT_EXPORT_DETAIL_DOCUMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMPORT_EXPORT_DETAIL_DOCUMENTComponent, resolver);
    }
} 