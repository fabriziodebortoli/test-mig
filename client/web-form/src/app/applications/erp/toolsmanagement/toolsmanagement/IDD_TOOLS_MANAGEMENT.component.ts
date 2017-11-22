import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_MANAGEMENTService } from './IDD_TOOLS_MANAGEMENT.service';

@Component({
    selector: 'tb-IDD_TOOLS_MANAGEMENT',
    templateUrl: './IDD_TOOLS_MANAGEMENT.component.html',
    providers: [IDD_TOOLS_MANAGEMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOOLS_MANAGEMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TOOLS_MANAGEMENTService,
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
		boService.appendToModelStructure({'global':['bGenerateTools','bSubstituteTools','bDisableTools','bAllItems','bSelItems','FromItem','ToItem','bCopyInFamilies','bSetNewDisabled','bAllTools','bSelTools','FromTool','ToTool','bAllFamilies','bSelFamilies','FromFamily','ToFamily','bOnlyDisabled','bOnlyExhausted','bOnlyOutOfOrder','bOperations','bBOMs','bMOs','bFamilies','bDisableSubstitutedTool','bOperationDelete','bOperationDisable','bOperations','bBOMs','bMOs','bFamilies','sUnloadIR','ToolsManagement','ToolsToGenerate'],'ToolsManagement':['LocalBmpStatus','LocalSelected','LocalItem','LocalItemOnHand','LocalNoToolsFromItem','LocalRootCode','LocalNoToolsToGenerate','LocalOriginalTool','LocalOriginalTool','LocalOriginalToolDescription','LocalOriginalTool','LocalOriginalToolDescription','LocalOriginalToolStatus','LocalOriginalToolDisabled','LocalOriginalToolDisabled','LocalNewTool','LocalItem','LocalUnloadToolIE'],'HKLItemBE':['Description','Description'],'HKLNewToolBE':['Description'],'ToolsToGenerate':['LocalBmpStatus','LocalSelected','LocalItem','LocalOriginalTool','LocalNewTool','LocalMessages'],'HKLToolByItemBE':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_MANAGEMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_MANAGEMENTComponent, resolver);
    }
} 