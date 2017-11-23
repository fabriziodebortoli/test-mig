import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CPHELPService } from './IDD_CPHELP.service';

@Component({
    selector: 'tb-IDD_CPHELP',
    templateUrl: './IDD_CPHELP.component.html',
    providers: [IDD_CPHELPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CPHELPComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SMART_CODE_HELP_OPTION_itemSource: any;

    constructor(document: IDD_CPHELPService,
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
        this.IDC_SMART_CODE_HELP_OPTION_itemSource = {
  "name": "SmartCodeCombinationCombo",
  "namespace": "ERP.SmartCode.Components.CombinationCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['SmartCodeHelpRoot','SmartCodeHelpRootDescri','SmartCodeSegment','SmartCodeHelpSmartCodeGraphic','SmartCodeHelpSmartCodeDescription'],'SmartCodeSegment':['VSmartCodeSegment_p1','VSmartCodeSegment_p2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CPHELPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CPHELPComponent, resolver);
    }
} 