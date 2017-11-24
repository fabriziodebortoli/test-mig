import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DRAWINGService } from './IDD_DRAWING.service';

@Component({
    selector: 'tb-IDD_DRAWING',
    templateUrl: './IDD_DRAWING.component.html',
    providers: [IDD_DRAWINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DRAWINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DRAWINGService,
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
        
        		this.bo.appendToModelStructure({'Drawings':['Drawing','Description','DerivedFrom','Notes','Item','PreferredDrawing','Revision','FilePath','DateOfSignature','ApprovalSignature','BarCode'],'HKLDrawings':['Description'],'HKLItems':['Description'],'global':['DrawingsRevisions','DrawingsDescription','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DrawingsRevisions':['Notes'],'DrawingsDescription':['Language','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DRAWINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DRAWINGComponent, resolver);
    }
} 