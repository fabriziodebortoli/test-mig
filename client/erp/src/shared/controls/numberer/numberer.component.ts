import { Component, Input, ViewChild, OnInit, OnChanges, ChangeDetectorRef, Output, EventEmitter, AfterViewInit } from '@angular/core';
import {
    ContextMenuItem, ControlComponent, TbComponentService, LayoutService, EventDataService
    , Store, FormMode, ControlContainerComponent, ParameterService
} from '@taskbuilder/core';
import { NumbererStateEnum } from './numberer-state.enum';
import { isNumeric } from './../../../rxjs.imports';

export type maskParts = { prefix: string, separator: string, body: string, suffix: string };

@Component({
    selector: "tb-numberer",
    templateUrl: './numberer.component.html',
    styleUrls: ['./numberer.component.scss']
})

// COSA MANCA:
// menu: insert in search - remove from search
// state button
// sistemare la posizione del context menu

export class NumbererComponent extends ControlComponent {
    @Input('readonly') readonly = false;
    @Input() public hotLink: { namespace: string, name: string };
    @Input() maxLength = -1;
    @Input() slice: any;
    @Input() selector: any;

    @ViewChild('textbox') textbox: any;
    @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

    @Output('blur') blur: EventEmitter<any> = new EventEmitter();

    // private tbEditIcon = 'tb-edit';
    // private tbExecuteIcon = 'tb-execute';
    // private currentState: NumbererStateEnum;
    // private icon: string;

    private tbMask = '';
    private useFormatMask = false;
    private enableCtrlInEdit = false;
    private paddingEnabled = true;
    private ctxMenuIndex = 0;
    private menuItemDisablePadding: ContextMenuItem;
    private menuItemEnablePadding: ContextMenuItem;
    private menuItemDoPadding: ContextMenuItem;

    // PADDING: in modalità find se maschera vuota allora padding default = false, altrimenti true

    mask = '';
    valueWasPadded = false;
    ctrlEnabled = false;
    enableStateInEdit = false;

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private store: Store,
        private parameterService: ParameterService
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }
    ngOnInit() {
        // this.currentState = this.model.stateData.invertState ? NumbererStateEnum.FreeInput : NumbererStateEnum.MaskedInput;
        // this.icon = this.model.stateData.invertState ? this.tbEditIcon : this.tbExecuteIcon;
        super.ngOnInit();

        this.ctxMenuIndex = this.cc.contextMenu.length;

        this.eventData.behaviours.subscribe(x => {
            const b = x[this.cmpId];
            if (b) {
                this.tbMask = b.formatMask;
                // this.useFormatMask = b.useFormatMask;
                this.useFormatMask = (b.formatMask !== '');
                this.enableCtrlInEdit = b.enableCtrlInEdit;
                this.enableStateInEdit = b.enableStateInEdit;

                this.paddingEnabled = (this.tbMask !== '');

                this.onFormModeChanged();

                this.setComponentMask();
            }
        });

        // selector as map: setting numberer properties, reading by parameters whose key is exposed as literal in the 
        // selector
        let selectorMap = this.selector.asMap();
        if (selectorMap.popUpParam) {
            this.buildContextMenuAsync(selectorMap.popUpParam);
        } else {
            this.buildContextMenu();
        }
        if (selectorMap.maxLengthParam) {
            this.setMaxLengthAsync(selectorMap.maxLengthParam);
        }

        this.store
            .select(this.selector)
            .select('value')
            .subscribe(this.setComponentMask.bind(this));

        this.store
            .select(this.selector)
            .select('formMode')
            .subscribe(this.onFormModeChanged.bind(this));
    }

    async setMaxLengthAsync(paramName: string) {
        let result = await this.parameterService.getParameter(paramName);
        if (result) {
            this.maxLength = +result;
            this.setMaxLength();
        }
    }

    setMaxLength() {
        if (
            this.maxLength > -1 &&
            this.maxLength != this.textbox.input.nativeElement.maxLength
        ) {
            this.textbox.input.nativeElement.maxLength = this.maxLength;
        };
    }

    async buildContextMenuAsync(paramName: string) {
        let result = await this.parameterService.getParameter(paramName);
        if (result) {
            this.buildContextMenu(result == '1');
        }
    }

    buildContextMenu(hasMenu: boolean = true) {
        this.cc.contextMenu.splice(this.ctxMenuIndex, this.cc.contextMenu.length);
        if (hasMenu) {
            if (this.paddingEnabled) {
                this.cc.contextMenu.push(this.menuItemDisablePadding);
                this.cc.contextMenu.push(this.menuItemDoPadding);
            }
            else {
                this.cc.contextMenu.push(this.menuItemEnablePadding);
            }
        }
    }
    onTranslationsReady() {
        super.onTranslationsReady();
        this.menuItemDisablePadding = new ContextMenuItem(this._TB('disable automatic digit padding in front of the number'), '', true, false, null, this.togglePadding.bind(this));
        this.menuItemEnablePadding = new ContextMenuItem(this._TB('enable automatic digit padding in front of the number'), '', true, false, null, this.togglePadding.bind(this));
        this.menuItemDoPadding = new ContextMenuItem(this._TB('perform digit padding in front of the number'), '', true, false, null, this.doPadding.bind(this));
    }

    onFormModeChanged() {
        if (this.eventData.model.FormMode) {
            let formMode = this.eventData.model.FormMode.value;
            this.ctrlEnabled = (formMode === FormMode.FIND || formMode === FormMode.NEW || (formMode === FormMode.EDIT && this.enableCtrlInEdit));
        }
        this.setComponentMask();
        this.valueWasPadded = false;
    }

    togglePadding() {
        switch (this.eventData.model.FormMode.value) {
            case FormMode.NEW:
            case FormMode.EDIT:
                {
                    this.paddingEnabled = !this.paddingEnabled;
                    this.buildContextMenu();
                }
        }
    }

    setComponentMask() {
        if (this.eventData.model.FormMode != undefined) {
            switch (this.eventData.model.FormMode.value) {
                case FormMode.FIND:
                case FormMode.BROWSE: {
                    this.mask = '';
                    break;
                }
                default: {
                    this.mask = this.valueToMask(this.model.value, this.tbMask);
                    break;
                }
            }
        }
    }

    valueToMask(value: string, tbMask: string) {
        let ret = '';
        let tbMaskChar: string;

        value = value.trim();

        if (value.length === 0)
            return '';

        for (let i = 0, len = tbMask.length; i < len; i++) {
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

    splitMask(tbMask: string): maskParts {
        let curChar: string;
        const ret: maskParts = { prefix: '', separator: '', body: '', suffix: '' };

        for (let i = 0, len = tbMask.length; i < len; i++) {
            curChar = tbMask.substring(i, i + 1);

            // prefisso
            if (curChar === 'Y')
                ret.prefix += curChar;

            // corpo
            else if (curChar === '#')            //  (['#', ',', '.'].indexOf(curChar))  separatori decimali non considerati
                ret.body += curChar;

            // suffisso
            else if (['-', '?', '*'].indexOf(curChar) > -1)
                ret.suffix += curChar;

            // separatore
            else
                ret.separator += curChar;
        }
        return ret;
    }

    maskToValue(tbMaskParts: maskParts, value: string): string {
        let ret = '';
        let curChar: string;
        let bodyPos: number;
        let bodyValue: string;
        let suffixPos = -1;

        // queta routine non considera i separatori decimali, che in mago non sono utilizzati nei numeratori

        const sepPos = tbMaskParts.separator.length > 0 ? value.indexOf(tbMaskParts.separator) : -1;

        // prefisso e separatore
        if (sepPos > -1) {
            // tutto quello che trovo a sinistra del separatore è il prefisso
            ret += value.substring(0, sepPos);
            bodyPos = sepPos + tbMaskParts.separator.length;
        } else {
            if (tbMaskParts.prefix !== '')
                ret += (new Date()).getFullYear().toString().substr(4 - tbMaskParts.prefix.length, tbMaskParts.prefix.length);
            bodyPos = 0;
        }
        ret += tbMaskParts.separator;

        for (let i = bodyPos, len = value.length; i < len; i++) {
            curChar = value.substring(i, i + 1);
            if (!isNumeric(curChar) || (i - bodyPos + 1) > tbMaskParts.body.length) {
                suffixPos = i;
                break;
            }
        }

        // corpo e suffisso
        if (suffixPos === -1) {
            bodyValue = value.substr(bodyPos, value.length - bodyPos);
            ret += (this.repeatChar('0', tbMaskParts.body.length - bodyValue.length) +
                bodyValue);
        } else {
            bodyValue = value.substr(bodyPos, suffixPos - bodyPos);
            ret += (this.repeatChar('0', tbMaskParts.body.length - bodyValue.length) +
                bodyValue);
            if (tbMaskParts.suffix.length > 0)
                ret += value.substr(suffixPos, tbMaskParts.suffix.length);
        }

        return ret;
    }

    repeatChar(char: string, times: number): string {
        return String(char).repeat(times);
    }

    onKeyPress($event) {
        if ($event.charCode === 95) {
            $event.preventDefault();
            return;
        }
    }

    onKeyDown($event) {
        // VERIFICARE SE SI PUO' FARE CON LA MASCHERA
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
            $event.preventDefault();
        }
    }

    transformTypedChar(charStr) {
        return /[a-g]/.test(charStr) ? charStr.toUpperCase() : charStr;
    }

    onBlur($event) {
        this.blur.emit(this);
        this.eventData.change.emit(this.cmpId);

        if (this.eventData.model.FormMode.value === FormMode.FIND && this.paddingEnabled)
            this.doPadding();
    }

    doPadding() {
        switch (this.eventData.model.FormMode.value) {
            case FormMode.FIND:
            case FormMode.NEW:
            case FormMode.EDIT:
                {
                    let value = this.model.value;

                    if (
                        value.trim() !== '' &&
                        isNumeric(value.substr(0, 1)) &&
                        !this.valueWasPadded
                    ) {
                        this.model.value = this.maskToValue(this.splitMask(this.tbMask), value);
                        this.valueWasPadded = true;
                    }
                }
        }
    }

    ngOnChanges(changes) {
        if (changes.maxLength) {
            this.setMaxLength();
        }
    }

    changeModelValue(event: any) {
        // if a mask with a fixed prefix is set, the textbox return as its value only the changeable part of it
        // ex. i'm creating a new document with number '17/00001' (fixed part) and the user completes it with a suffix, so the number becomes '17/00001AD'
        // the textbox return as its value only the suffix 'AD'. this compels me to read the entire value from the native element,
        // stripping the underscore from it 

        // this.model.value = value;
        switch (this.eventData.model.FormMode.value) {
            case FormMode.NEW:
            case FormMode.EDIT:
                this.model.value = this.textbox.input.nativeElement.value.replace('_', ' ').trim();
                this.valueWasPadded = false;
        }
    }

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
