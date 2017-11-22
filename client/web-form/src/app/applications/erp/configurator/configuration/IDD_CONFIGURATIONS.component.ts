import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONFIGURATIONSService } from './IDD_CONFIGURATIONS.service';

@Component({
    selector: 'tb-IDD_CONFIGURATIONS',
    templateUrl: './IDD_CONFIGURATIONS.component.html',
    providers: [IDD_CONFIGURATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CONFIGURATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CONFIGQUESTIONSANSWER_ANSWERNO_itemSource: any;

    constructor(document: IDD_CONFIGURATIONSService,
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
        this.IDC_CONFIGQUESTIONSANSWER_ANSWERNO_itemSource = {
  "name": "AnswersToQuestionComboBox",
  "namespace": "ERP.Configurator.Documents.AnswersToQuestionComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Configuration':['Configuration','Price','Item','Customer'],'HKLItem':['Description'],'HKLCustomer':['CompanyName'],'global':['ConfigQuestionsAnswer','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ConfigQuestionsAnswer':['QuestionNo','Question','AnswerNo','l_Answer','DeleteComponent','l_DeleteDes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONFIGURATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONFIGURATIONSComponent, resolver);
    }
} 