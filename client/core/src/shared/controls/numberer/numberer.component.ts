import { Component, Input, ViewChild, ViewContainerRef, OnInit, OnChanges } from '@angular/core';

import { isNumeric } from '../../../rxjs.imports';
import { ContextMenuItem } from './../../models/context-menu-item.model';
import { FormMode } from './../../../shared/models/form-mode.enum';
import { NumbererStateEnum } from './numberer-state.enum';
import { ControlComponent } from '../control.component';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { Store } from './../../../core/services/store.service';


import { MaskedTextBoxComponent } from '@progress/kendo-angular-inputs';

@Component({
    selector: "tb-numberer",
    templateUrl: './numberer.component.html',
    styleUrls: ['./numberer.component.scss']
})

// COSA MANCA:
// menu: insert in search - remove from search
// state button
// sistemare la posizione del context menu
// enabled del context menu (legare a enabled della text)
// estendere la tb-text

export class NumbererComponent extends ControlComponent {
    @Input('readonly') readonly = false;
    @Input() public hotLink: any = undefined;
    @Input() automaticNumbering: boolean;

    @Input() slice: any;
    @Input() selector: any;

    @ViewChild('contextMenu', { read: ViewContainerRef }) contextMenu: ViewContainerRef;
    //@ViewChild('textbox') textbox: MaskedTextBoxComponent;

    tbEditIcon = 'tb-edit';
    tbExecuteIcon = 'tb-execute';

    icon: string;

    private tbMask = '';
    private useFormatMask = false;
    private enableCtrlInEdit = false;
    private paddingEnabled: boolean = true;

    numbererContextMenu: ContextMenuItem[] = [];
    menuItemDisablePadding = new ContextMenuItem('disable automatic digit padding in front of the number', '', true, false, null, this.togglePadding.bind(this));
    menuItemEnablePadding = new ContextMenuItem('enable automatic digit padding in front of the number', '', true, false, null, this.togglePadding.bind(this));
    menuItemDoPadding = new ContextMenuItem('perform digit padding in front of the number', '', true, false, null, this.doPadding.bind(this));

    mask = '';
    valueWasPadded = false;
    //ctrlEnabled = false;
    ctrlEnabled = true; // da rimuovere dopo aver implementato lo store
    enableStateInEdit = false;

    private currentState: NumbererStateEnum;

    // PADDING: in modalità find se maschera vuota allora padding default = false, altrimenti true

    ngOnInit() {
        // this.currentState = this.model.stateData.invertState ? NumbererStateEnum.FreeInput : NumbererStateEnum.MaskedInput;
        // this.icon = this.model.stateData.invertState ? this.tbEditIcon : this.tbExecuteIcon;

        this.eventData.behaviours.subscribe(x => {
            const b = x[this.cmpId];
            if (b) {
                this.tbMask = b.formatMask;
                // this.useFormatMask = b.useFormatMask;
                this.useFormatMask = (b.formatMask !== '');
                this.enableCtrlInEdit = b.enableCtrlInEdit;
                this.enableStateInEdit = b.enableStateInEdit;

                this.paddingEnabled = (this.tbMask !== '');
                this.ctrlEnabled = (x.value === FormMode.NEW || (x.value === FormMode.EDIT && this.enableCtrlInEdit));

                this.ctrlEnabled = true; // da rimuovere dopo aver implementato lo store

                this.setComponentMask();
            }
        });

        this.store
            .select(this.selector)
            .select('value')
            .subscribe(
            v => console.log('changeOfValue ' + v)
            );
    }

    constructor(
        public eventData: EventDataService,
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        private store: Store
    ) {
        super(layoutService, tbComponentService);


    }

    ngAfterViewInit() {
        this.buildContextMenu();
    }

    buildContextMenu() {
        this.numbererContextMenu.splice(0, this.numbererContextMenu.length);
        if (this.paddingEnabled) {
            this.numbererContextMenu.push(this.menuItemDisablePadding);
            this.numbererContextMenu.push(this.menuItemDoPadding);
        }
        else {
            this.numbererContextMenu.push(this.menuItemEnablePadding);
        }
    }

    togglePadding() {
        this.paddingEnabled = !this.paddingEnabled;
        this.buildContextMenu();
    }

    setComponentMask() {
        switch (this.eventData.model.FormMode.value) {
            case FormMode.BROWSE:
            case FormMode.FIND: {
                this.mask = '';
                break;
            }
            default: {
                this.mask = this.valueToMask(this.model.value, this.tbMask);
                break;
            }
        }
    }

    valueToMask(value: string, tbMask: string) {
        let ret = '';
        let i: number;
        let tbMaskChar: string;

        value = value.trim();

        if (value.length > 0)
            return '';

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

    maskToValue(tbMask: string, value: string) {
        let i: number;
        let ret = '';
        let sepPos: number;
        let curChar: string;

        let numValue = '';
        let alphaValue = '';
        let readingNumValue = true;

        if (value.length === 0)
            return ret;

        // separo parte numerica e alfanumerica del valore inserito
        // al primo carattere non numerico inizia la parte alfanumerica, che include anche i numerici successivi
        for (i = 0; i < value.length; i++) {
            curChar = value.substring(i, i + 1);
            if (readingNumValue && isNumeric(curChar))
                numValue += curChar;
            else {
                readingNumValue = false;
                alphaValue += curChar;
            }
        }

        const tbMaskChunks: string[] = this.splitTbMask(tbMask);
        tbMaskChunks.forEach(c => {
            console.log(c);
            if (c.startsWith('Y'))
                // sostituisco la porzione di anno che mi interessa
                ret += (new Date()).getFullYear().toString().substr(4 - c.length, c.length);

            else if (
                c.startsWith('#') ||
                c.startsWith('N')) {
                // faccio il padding del numero
                sepPos = c.indexOf(',');
                if (sepPos === -1)
                    sepPos = c.indexOf('.');

                if (sepPos === -1) {
                    // se il valore inserito eccede la lunghezza del 'corpo' numerico tolgo l'eccedenza e l'aggiungo 
                    // al suffisso
                    if (numValue.length > c.length) {
                        alphaValue = numValue.substring(numValue.length) + alphaValue;
                        numValue = numValue.substring(0, numValue.length - 1);
                    }

                    ret += this.repeatChar('0', c.length - numValue.length) + numValue;
                } else {
                    // SEPARATORE, CASISTICA PROBABILMENTE NON SUPPORTATA IN MAGO
                    ret += this.repeatChar('0', sepPos - numValue.length) + numValue + '.' + this.repeatChar('0', numValue.length - sepPos + 1);
                }
            } else if (c === '/' || c === '-')
                // separatore
                ret += c;
            else if (
                c.startsWith('?') ||
                c.startsWith('*') ||
                c.startsWith('-')) {
                if (alphaValue.length > c.length)
                    alphaValue = alphaValue.substring(0, c.length - 1);
                ret += alphaValue;
            }
        });

        return ret;
    }

    repeatChar(char: string, times: number): string {
        let ret = '';
        let i: number;
        for (i = 1; i <= times; i++)
            ret += char;
        return ret;
    }

    splitTbMask(tbMask: string): string[] {
        let ret: string[] = [];
        let i: number;
        let tbPrevMaskChar: string;

        let chunk = '';
        let tbMaskChar = '';

        for (i = 0; i < tbMask.length; i++) {
            tbMaskChar = tbMask.substring(i, i + 1);

            if (['Y', '#', '?', '*', '-', 'N'].indexOf(tbMaskChar) > -1) {
                if (tbMaskChar !== tbPrevMaskChar) {
                    if (chunk.length > 0) {
                        ret.push(chunk);
                        chunk = '';
                    }
                }

                chunk += tbMaskChar;
            } else if ([',', '.'].indexOf(tbMaskChar) > -1) {
                // SEPARATORE, CASISTICA PROBABILMENTE NON SUPPORTATA IN MAGO
                chunk += tbMaskChar;
            } else {
                if (chunk.length > 0) {
                    ret.push(chunk);
                    chunk = tbMaskChar;
                }
            }

            tbPrevMaskChar = tbMaskChar;
        }

        if (chunk.length > 0)
            ret.push(chunk);

        return ret;
    }

    onKeyDown($event) {
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
            $event.preventDefault();
        }
    }

    onBlur($event) {
        if (this.eventData.model.FormMode.value === FormMode.FIND && this.paddingEnabled)
            this.doPadding();
    }

    doPadding() {
        let value = this.model.value;

        if (
            value.trim() !== '' &&
            isNumeric(value.substr(0, 1)) &&
            !this.valueWasPadded
        ) {
            this.model.value = this.maskToValue(this.tbMask, value);
            this.valueWasPadded = true;
        }
    }

    ngOnChanges(changes) {
        console.log('numberer ngOnChanges: ' + JSON.stringify(changes));
    }

    // test() {
    //     this.eventData.change.emit('0');
    // }

    changeModelValue(value) {
        this.model.value = value;
        this.valueWasPadded = false;
    }

    // onChange($event) {
    //     this.model.value = this.model.value;
    // }

    // get htmlValue() {
    //     if (this.model)
    //         return this.model.value;
    //     return null;
    // }

    // set htmlValue(value) {
    //     if (this.model)
    //         this.model.value = value;
    // }

    // toggleState() {
    //     if (this.currentState == NumbererStateEnum.MaskedInput) {
    //         this.icon = this.tbEditIcon;
    //         this.currentState = NumbererStateEnum.FreeInput
    //     }
    //     else {
    //         this.icon = this.tbExecuteIcon;
    //         this.currentState = NumbererStateEnum.MaskedInput
    //     }
    // }

}
