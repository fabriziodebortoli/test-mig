import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_ANALISYSService } from './IDD_TOOLS_ANALISYS.service';

@Component({
    selector: 'tb-IDD_TOOLS_ANALISYS',
    templateUrl: './IDD_TOOLS_ANALISYS.component.html',
    providers: [IDD_TOOLS_ANALISYSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TOOLS_ANALISYSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TOOLS_ANALISYSService,
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
        
        		this.bo.appendToModelStructure({'global':['bMOALL','bMOSel','sMOFrom','sMOTo','bChooseAlternateAll','bChooseAlternatePreferred','bChooseAlternateSpecific','sChooseAlternateCode','bAlsoHints','bAlsoWarnings','bAlsoErrors','bAllToolStatus','bSelToolStatus','eToolStatus','ToolsAnalysis'],'ToolsAnalysis':['Type','BmpStatus','Status','_IsFamily','Tool','ToolType','Exclusive','NeededQty','NeededTime','MONo','RtgStep','Alternate','AltRtgStep','Message'],'HKLSelectorTools':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_ANALISYSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_ANALISYSComponent, resolver);
    }
} 