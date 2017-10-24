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
                this.formatMask = this.valueToMask(this.model.value, b.formatMask);
                //this.useFormatMask = b.useFormatMask;
                this.useFormatMask = (b.formatMask !== '');
                this.enableCtrlInEdit = b.enableCtrlInEdit;
                this.enableStateInEdit = b.enableStateInEdit;
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

    valueToMask(value: string, tbMask: string) {
        let ret = '';
        let i: number;
        let tbMaskChar: string;
    
        for (i = 0; i < tbMask.length; i++) {
          tbMaskChar = tbMask.substring(i, i + 1);
          // I 5 CARATTERI CHE SEGUONO INDICANO ELEMENTI DI MASCHERA NON EDITABILE, QUINDI PASSA IL CARATTERE DEL VALORE CONNESSO ALLA MASCHERA.
          // RIMANGONO DUE CARATTERI DI MASCHERA: IL SEPARATORE DECIMALE (,), CHE VIENE SOSTITUITO DAL PUNTO, E IL PUNTO INTERROGATIVO, CHE INDICA
          // UN SUFFISSO EDITABILE. IL PUNTO INTERROGATIVO VIENE SOSTITUITO SUCCESSIVAMENTE ALLA SOSTITUZIONE DEI CARATTERI CHIAVE DELLA
          // MASK KENDO CON I CORRISPENDENTI CARATTERI FISSI (ES. '0' DIVENTA '\0')
          if (['Y', '#', '*', '-', 'N'].indexOf(tbMaskChar) > -1)
            ret += value.substring(i, i + 1);
          else
            ret += tbMaskChar;
        }
    
        return ret
          .replace(/([09#LAa&C])/g, '\\\$1')
          .replace(/[?]/g, 'A')
          .replace(/[,]/g, '.')
          ;
    }

    onKeyDown($event) {
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
          $event.preventDefault();
        }
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
