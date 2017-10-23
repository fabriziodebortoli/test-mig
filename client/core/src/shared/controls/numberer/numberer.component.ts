import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from '../control.component';
import { EventDataService } from './../../../core/services/eventdata.service';

import { Component, Input, ViewChild, ViewContainerRef } from '@angular/core';
import { NumbererStateEnum } from './numberer-state.enum';
@Component({
    selector: "tb-numberer",
    templateUrl: './numberer.component.html',
    styleUrls: ['./numberer.component.scss']
})

export class NumbererComponent extends ControlComponent {
    @Input('readonly') readonly: boolean = false;
    @Input() public hotLink: any = undefined;
    @Input() automaticNumbering: boolean;

    @ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;

    tbEditIcon = "tb-edit";
    tbExecuteIcon = "tb-execute";

    icon: string;

    formatMask: string = '';
    enableCtrlInEdit = false;
    enableStateInEdit = false;
    useFormatMask = false;

    private currentValue = "";
    private currentState: NumbererStateEnum;

    ngOnInit() {
        this.currentState = this.model.stateData.invertState ? NumbererStateEnum.FreeInput : NumbererStateEnum.MaskedInput;
        this.icon = this.model.stateData.invertState ? this.tbEditIcon : this.tbExecuteIcon;

        this.eventData.behaviours.subscribe(x => {
            let b = x[this.cmpId];
            if (b) {
                this.formatMask = this.translateMask(b.formatMask);
                this.enableCtrlInEdit = b.enableCtrlInEdit;
                this.enableStateInEdit = b.enableStateInEdit;
                this.useFormatMask = b.useFormatMask;
            }
        })
    }

    constructor(
        public eventData: EventDataService,
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService
    ) {
        super(layoutService, tbComponentService);
    }

    translateMask(serverMask: string): string {
        return serverMask;
    }

    toggleState() {
        if (this.currentState == NumbererStateEnum.MaskedInput) {
            this.icon = this.tbEditIcon;
            this.currentState = NumbererStateEnum.FreeInput
        }
        else {
            this.icon = this.tbExecuteIcon;
            this.currentState = NumbererStateEnum.MaskedInput
        }

        "model?.stateData?.binding?.datasource"
    }

    ngOnChanges(changes) {

    }




}
