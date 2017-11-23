import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DMSREPOSITORY_EXPLORERService } from './IDD_DMSREPOSITORY_EXPLORER.service';

@Component({
    selector: 'tb-IDD_DMSREPOSITORY_EXPLORER',
    templateUrl: './IDD_DMSREPOSITORY_EXPLORER.component.html',
    providers: [IDD_DMSREPOSITORY_EXPLORERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DMSREPOSITORY_EXPLORERComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DMSREPOSITORY_FILEEXTENSION_itemSource: any;
public IDC_DMSREPOSITORY_ADVSEL_COLLECTIONS_itemSource: any;

    constructor(document: IDD_DMSREPOSITORY_EXPLORERService,
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
        this.IDC_DMSREPOSITORY_FILEEXTENSION_itemSource = {
  "name": "FilesExtensionsItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.FilesExtensionsItemSource"
}; 
this.IDC_DMSREPOSITORY_ADVSEL_COLLECTIONS_itemSource = {
  "name": "CollectionsItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.CollectionsItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllExtractedDoc','bFirstExtractedDoc','nTopNrDocuments','FromDate','ToDate','sFileExtension','FreeText','bFileNameAndDescription','bTags','bBarcode','bBookmarks','bDocumentContent','bSelectWorkers','bShowDisabledWorkers','bAllRepository','bOnlyCollection','DocNamespace','DBTSearchFieldsConditions','DBTArchivedDocuments'],'DBTSearchFieldsConditions':['VFieldDescription','VFormattedValue'],'DBTArchivedDocuments':['VIsSelected','VArchivedDocId','VIsAttachmentBmp','VIsWoormReportBmp','VCheckOutWorkerBmp','VName','VDescription','VWorker','VCreationDate','VModifiedDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DMSREPOSITORY_EXPLORERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DMSREPOSITORY_EXPLORERComponent, resolver);
    }
} 