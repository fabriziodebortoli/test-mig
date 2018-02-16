var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { Component, Input, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ContextMenuItem, ControlComponent, TbComponentService, LayoutService, EventDataService, Store, FormMode } from '@taskbuilder/core';
import { isNumeric } from './../../../rxjs.imports';
var NumbererComponent = (function (_super) {
    __extends(NumbererComponent, _super);
    function NumbererComponent(eventData, layoutService, tbComponentService, changeDetectorRef, store) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.store = store;
        _this.readonly = false;
        _this.popUpMenu = true;
        _this.maxLength = -1;
        _this.tbEditIcon = 'tb-edit';
        _this.tbExecuteIcon = 'tb-execute';
        _this.tbMask = '';
        _this.useFormatMask = false;
        _this.enableCtrlInEdit = false;
        _this.paddingEnabled = true;
        _this.subscribedToSelector = false;
        _this.numbererContextMenu = [];
        // PADDING: in modalità find se maschera vuota allora padding default = false, altrimenti true
        _this.mask = '';
        _this.valueWasPadded = false;
        _this.ctrlEnabled = false;
        _this.enableStateInEdit = false;
        _this.enableLocalization();
        return _this;
    }
    /**
     * @return {?}
     */
    NumbererComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        var _this = this;
        // this.currentState = this.model.stateData.invertState ? NumbererStateEnum.FreeInput : NumbererStateEnum.MaskedInput;
        // this.icon = this.model.stateData.invertState ? this.tbEditIcon : this.tbExecuteIcon;
        //console.log(this.textbox.input.nativeElement);
        this.eventData.behaviours.subscribe(function (x) {
            var /** @type {?} */ b = x[_this.cmpId];
            if (b) {
                _this.tbMask = b.formatMask;
                // this.useFormatMask = b.useFormatMask;
                // this.useFormatMask = b.useFormatMask;
                _this.useFormatMask = (b.formatMask !== '');
                _this.enableCtrlInEdit = b.enableCtrlInEdit;
                _this.enableStateInEdit = b.enableStateInEdit;
                _this.paddingEnabled = (_this.tbMask !== '');
                _this.onFormModeChanged(x.value);
                _this.setComponentMask();
            }
        });
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.onTranslationsReady = /**
     * @return {?}
     */
    function () {
        _super.prototype.onTranslationsReady.call(this);
        this.menuItemDisablePadding = new ContextMenuItem(this._TB('disable automatic digit padding in front of the number'), '', true, false, null, this.togglePadding.bind(this));
        this.menuItemEnablePadding = new ContextMenuItem(this._TB('enable automatic digit padding in front of the number'), '', true, false, null, this.togglePadding.bind(this));
        this.menuItemDoPadding = new ContextMenuItem(this._TB('perform digit padding in front of the number'), '', true, false, null, this.doPadding.bind(this));
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.subscribeToSelector = /**
     * @return {?}
     */
    function () {
        var _this = this;
        if (!this.subscribedToSelector && this.store && this.selector) {
            this.store
                .select(this.selector)
                .select('value')
                .subscribe(function (v) { return _this.setComponentMask(); });
            this.store
                .select(this.selector)
                .select('formMode')
                .subscribe(function (v) { return _this.onFormModeChanged(v); });
            this.subscribedToSelector = true;
        }
    };
    /**
     * @param {?} formMode
     * @return {?}
     */
    NumbererComponent.prototype.onFormModeChanged = /**
     * @param {?} formMode
     * @return {?}
     */
    function (formMode) {
        this.setComponentMask();
        this.ctrlEnabled = (formMode === FormMode.FIND || formMode === FormMode.NEW || (formMode === FormMode.EDIT && this.enableCtrlInEdit));
        this.buildContextMenu();
        this.valueWasPadded = false;
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.buildContextMenu = /**
     * @return {?}
     */
    function () {
        this.numbererContextMenu.splice(0, this.numbererContextMenu.length);
        if (this.ctrlEnabled) {
            if (this.paddingEnabled) {
                this.numbererContextMenu.push(this.menuItemDisablePadding);
                this.numbererContextMenu.push(this.menuItemDoPadding);
            }
            else {
                this.numbererContextMenu.push(this.menuItemEnablePadding);
            }
        }
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.togglePadding = /**
     * @return {?}
     */
    function () {
        this.paddingEnabled = !this.paddingEnabled;
        this.buildContextMenu();
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.setComponentMask = /**
     * @return {?}
     */
    function () {
        if (this.eventData.model.FormMode != undefined) {
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
    };
    /**
     * @param {?} value
     * @param {?} tbMask
     * @return {?}
     */
    NumbererComponent.prototype.valueToMask = /**
     * @param {?} value
     * @param {?} tbMask
     * @return {?}
     */
    function (value, tbMask) {
        var /** @type {?} */ ret = '';
        var /** @type {?} */ tbMaskChar;
        value = value.trim();
        if (value.length === 0)
            return '';
        for (var /** @type {?} */ i = 0, /** @type {?} */ len = tbMask.length; i < len; i++) {
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
            .replace(/[,]/g, '.');
    };
    /**
     * @param {?} tbMask
     * @return {?}
     */
    NumbererComponent.prototype.splitMask = /**
     * @param {?} tbMask
     * @return {?}
     */
    function (tbMask) {
        var /** @type {?} */ curChar;
        var /** @type {?} */ ret = { prefix: '', separator: '', body: '', suffix: '' };
        for (var /** @type {?} */ i = 0, /** @type {?} */ len = tbMask.length; i < len; i++) {
            curChar = tbMask.substring(i, i + 1);
            // prefisso
            if (curChar === 'Y')
                ret.prefix += curChar;
            else if (curChar === '#')
                //  (['#', ',', '.'].indexOf(curChar))  separatori decimali non considerati
                ret.body += curChar;
            else if (['-', '?', '*'].indexOf(curChar) > -1)
                ret.suffix += curChar;
            else
                ret.separator += curChar;
        }
        return ret;
    };
    /**
     * @param {?} tbMaskParts
     * @param {?} value
     * @return {?}
     */
    NumbererComponent.prototype.maskToValue = /**
     * @param {?} tbMaskParts
     * @param {?} value
     * @return {?}
     */
    function (tbMaskParts, value) {
        var /** @type {?} */ ret = '';
        var /** @type {?} */ curChar;
        var /** @type {?} */ bodyPos;
        var /** @type {?} */ bodyValue;
        var /** @type {?} */ suffixPos = -1;
        // queta routine non considera i separatori decimali, che in mago non sono utilizzati nei numeratori
        var /** @type {?} */ sepPos = tbMaskParts.separator.length > 0 ? value.indexOf(tbMaskParts.separator) : -1;
        // prefisso e separatore
        if (sepPos > -1) {
            // tutto quello che trovo a sinistra del separatore è il prefisso
            ret += value.substring(0, sepPos);
            bodyPos = sepPos + tbMaskParts.separator.length;
        }
        else {
            if (tbMaskParts.prefix !== '')
                ret += (new Date()).getFullYear().toString().substr(4 - tbMaskParts.prefix.length, tbMaskParts.prefix.length);
            bodyPos = 0;
        }
        ret += tbMaskParts.separator;
        for (var /** @type {?} */ i = bodyPos, /** @type {?} */ len = value.length; i < len; i++) {
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
        }
        else {
            bodyValue = value.substr(bodyPos, suffixPos - bodyPos);
            ret += (this.repeatChar('0', tbMaskParts.body.length - bodyValue.length) +
                bodyValue);
            if (tbMaskParts.suffix.length > 0)
                ret += value.substr(suffixPos, tbMaskParts.suffix.length);
        }
        return ret;
    };
    /**
     * @param {?} char
     * @param {?} times
     * @return {?}
     */
    NumbererComponent.prototype.repeatChar = /**
     * @param {?} char
     * @param {?} times
     * @return {?}
     */
    function (char, times) {
        return String(char).repeat(times);
    };
    /**
     * @param {?} $event
     * @return {?}
     */
    NumbererComponent.prototype.onKeyPress = /**
     * @param {?} $event
     * @return {?}
     */
    function ($event) {
        if ($event.charCode === 95) {
            $event.preventDefault();
        }
    };
    /**
     * @param {?} $event
     * @return {?}
     */
    NumbererComponent.prototype.onKeyDown = /**
     * @param {?} $event
     * @return {?}
     */
    function ($event) {
        // VERIFICARE SE SI PUO' FARE CON LA MASCHERA
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
            $event.preventDefault();
        }
    };
    /**
     * @param {?} charStr
     * @return {?}
     */
    NumbererComponent.prototype.transformTypedChar = /**
     * @param {?} charStr
     * @return {?}
     */
    function (charStr) {
        return /[a-g]/.test(charStr) ? charStr.toUpperCase() : charStr;
    };
    /**
     * @param {?} $event
     * @return {?}
     */
    NumbererComponent.prototype.onBlur = /**
     * @param {?} $event
     * @return {?}
     */
    function ($event) {
        if (blur && !blur())
            return;
        if (this.eventData.model.FormMode.value === FormMode.FIND && this.paddingEnabled)
            this.doPadding();
    };
    /**
     * @return {?}
     */
    NumbererComponent.prototype.doPadding = /**
     * @return {?}
     */
    function () {
        var /** @type {?} */ value = this.model.value;
        if (value.trim() !== '' &&
            isNumeric(value.substr(0, 1)) &&
            !this.valueWasPadded) {
            this.model.value = this.maskToValue(this.splitMask(this.tbMask), value);
            this.valueWasPadded = true;
        }
    };
    /**
     * @param {?} changes
     * @return {?}
     */
    NumbererComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        this.subscribeToSelector();
        if (changes.maxLength &&
            this.maxLength > -1 &&
            this.maxLength != this.textbox.input.nativeElement.maxLength) {
            this.textbox.input.nativeElement.maxLength = this.maxLength;
        }
        ;
    };
    /**
     * @param {?} value
     * @return {?}
     */
    NumbererComponent.prototype.changeModelValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        // if a mask with a fixed prefix is set, the textbox return as its value only the changeable part of it
        // ex. i'm creating a new document with number '17/00001' (fixed part) and the user completes it with a suffix, so the number becomes '17/00001AD'
        // the textbox return as its value only the suffix 'AD'. this compels me to read the entire value from the native element,
        // stripping the underscore from it
        // this.model.value = value;
        this.model.value = this.textbox.input.nativeElement.value.replace('_', ' ').trim();
        this.valueWasPadded = false;
    };
    NumbererComponent.decorators = [
        { type: Component, args: [{
                    selector: "tb-numberer",
                    template: "<div class=\"tb-control tb-numberer\" [ngClass]=\"componentClass()\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <div class=\"group\"> <kendo-maskedtextbox #textbox [mask]=\"mask\" [ngModel]=\"model?.value\" required=\"required\" [disabled]=\"!model?.enabled || !ctrlEnabled\" (ngModelChange)=\"changeModelValue($event)\" [style.width.px]=\"width\" (keypress)=\"onKeyPress($event)\" (keydown)=\"onKeyDown($event)\" (blur)=\"onBlur($event)\" [maxlength]=\"maxLength\"> </kendo-maskedtextbox> <tb-context-menu [contextMenu]=\"numbererContextMenu\" *ngIf=\"popUpMenu\"></tb-context-menu> </div> </div>",
                    styles: [".group { display: inline-flex; } "]
                },] },
    ];
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
    /** @nocollapse */
    NumbererComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: Store, },
    ]; };
    NumbererComponent.propDecorators = {
        "readonly": [{ type: Input, args: ['readonly',] },],
        "hotLink": [{ type: Input },],
        "popUpMenu": [{ type: Input },],
        "maxLength": [{ type: Input },],
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
        "textbox": [{ type: ViewChild, args: ['textbox',] },],
    };
    return NumbererComponent;
}(ControlComponent));
export { NumbererComponent };
function NumbererComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    NumbererComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    NumbererComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    NumbererComponent.propDecorators;
    /** @type {?} */
    NumbererComponent.prototype.readonly;
    /** @type {?} */
    NumbererComponent.prototype.hotLink;
    /** @type {?} */
    NumbererComponent.prototype.popUpMenu;
    /** @type {?} */
    NumbererComponent.prototype.maxLength;
    /** @type {?} */
    NumbererComponent.prototype.slice;
    /** @type {?} */
    NumbererComponent.prototype.selector;
    /** @type {?} */
    NumbererComponent.prototype.textbox;
    /** @type {?} */
    NumbererComponent.prototype.tbEditIcon;
    /** @type {?} */
    NumbererComponent.prototype.tbExecuteIcon;
    /** @type {?} */
    NumbererComponent.prototype.icon;
    /** @type {?} */
    NumbererComponent.prototype.tbMask;
    /** @type {?} */
    NumbererComponent.prototype.useFormatMask;
    /** @type {?} */
    NumbererComponent.prototype.enableCtrlInEdit;
    /** @type {?} */
    NumbererComponent.prototype.paddingEnabled;
    /** @type {?} */
    NumbererComponent.prototype.subscribedToSelector;
    /** @type {?} */
    NumbererComponent.prototype.numbererContextMenu;
    /** @type {?} */
    NumbererComponent.prototype.menuItemDisablePadding;
    /** @type {?} */
    NumbererComponent.prototype.menuItemEnablePadding;
    /** @type {?} */
    NumbererComponent.prototype.menuItemDoPadding;
    /** @type {?} */
    NumbererComponent.prototype.mask;
    /** @type {?} */
    NumbererComponent.prototype.valueWasPadded;
    /** @type {?} */
    NumbererComponent.prototype.ctrlEnabled;
    /** @type {?} */
    NumbererComponent.prototype.enableStateInEdit;
    /** @type {?} */
    NumbererComponent.prototype.currentState;
    /** @type {?} */
    NumbererComponent.prototype.eventData;
    /** @type {?} */
    NumbererComponent.prototype.store;
}
