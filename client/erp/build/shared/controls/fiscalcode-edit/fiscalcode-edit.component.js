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
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService, HttpService, ParameterService } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http } from '@angular/http';
import JsCheckTaxId from '../taxid-edit/jscheckTaxIDNumber';
var FiscalCodeEditComponent = (function (_super) {
    __extends(FiscalCodeEditComponent, _super);
    function FiscalCodeEditComponent(layoutService, eventData, tbComponentService, changeDetectorRef, parameterService, store, httpservice, httpCore, http) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.parameterService = parameterService;
        _this.store = store;
        _this.httpservice = httpservice;
        _this.httpCore = httpCore;
        _this.http = http;
        _this.readonly = false;
        _this.ctrlEnabled = false;
        _this.isMasterBR = false;
        _this.isMasterIT = false;
        _this.isEuropeanUnion = false;
        _this.isoCode = '';
        _this.checkfiscalcodeContextMenu = [];
        _this.menuItemITCheck = new ContextMenuItem(_this._TB('Check TaxId existence (IT site)'), '', true, false, null, _this.checkIT.bind(_this));
        _this.menuItemEUCheck = new ContextMenuItem(_this._TB('Check TaxId existence (EU site)'), '', true, false, null, _this.checkEU.bind(_this));
        _this.menuItemBRCheck = new ContextMenuItem(_this._TB('Check Fiscal Code existence'), '', true, false, null, _this.checkBR.bind(_this));
        return _this;
    }
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        var _this = this;
        if (this.store && this.selector) {
            this.store
                .select(this.selector)
                .select('formMode')
                .subscribe(function (v) { return _this.onFormModeChanged(v); });
        }
        this.httpservice.isActivated('ERP', 'MasterData_BR').take(1).subscribe(function (res) { _this.isMasterBR = res.result; });
        this.httpservice.isActivated('ERP', 'MasterData_IT').take(1).subscribe(function (res) { _this.isMasterIT = res.result; });
        this.httpservice.isActivated('ERP', 'EuropeanUnion').take(1).subscribe(function (res) { _this.isEuropeanUnion = res.result; });
    };
    /**
     * @param {?} changes
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        if (changes.slice && changes.slice.isoCode) {
            this.isoCode = changes.slice.isoCode.value;
        }
        this.validate();
    };
    /**
     * @param {?} formMode
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.onFormModeChanged = /**
     * @param {?} formMode
     * @return {?}
     */
    function (formMode) {
        this.ctrlEnabled = formMode === FormMode.FIND || formMode === FormMode.NEW || formMode === FormMode.EDIT;
        this.buildContextMenu();
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.buildContextMenu = /**
     * @return {?}
     */
    function () {
        this.checkfiscalcodeContextMenu.splice(0, this.checkfiscalcodeContextMenu.length);
        if (!this.ctrlEnabled)
            return;
        if (this.isTaxIdField(this.model.value, false))
            this.checkfiscalcodeContextMenu.push(this.menuItemITCheck);
        if (this.isEuropeanUnion && this.isTaxIdField(this.model.value, false))
            this.checkfiscalcodeContextMenu.push(this.menuItemEUCheck);
        if (this.isMasterBR && this.isoCode === 'BR')
            this.checkfiscalcodeContextMenu.push(this.menuItemBRCheck);
    };
    /**
     * @param {?} code
     * @param {?} all
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.isTaxIdField = /**
     * @param {?} code
     * @param {?} all
     * @return {?}
     */
    function (code, all) {
        return code !== '' && (all || this.isMasterIT);
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.onBlur = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.buildContextMenu();
                this.validate();
                if (!this.isValid)
                    return [2 /*return*/];
                this.blur.emit(this);
                this.eventData.change.emit(this.cmpId);
                return [2 /*return*/];
            });
        });
    };
    /**
     * @param {?} value
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.changeModelValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this.model.value = value;
        this.validate();
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.checkIT = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var stato, url, newWindow;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.getStato()];
                    case 1:
                        stato = _a.sent();
                        url = "http://www1.agenziaentrate.it/servizi/vies/vies.htm?act=piva&s=" + stato + "&p=" + this.model.value;
                        newWindow = window.open(url, 'blank');
                        return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.checkBR = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var url, newWindow;
            return __generator(this, function (_a) {
                url = 'https://www.receita.fazenda.gov.br/Aplicacoes/SSL/ATCTA/CPF/ConsultaSituacao/ConsultaPublica.asp';
                newWindow = window.open(url, 'blank');
                return [2 /*return*/];
            });
        });
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.checkEU = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var stato, r;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.getStato()];
                    case 1:
                        stato = _a.sent();
                        return [4 /*yield*/, this.httpCore.checkVatEU(stato, this.model.value).toPromise()];
                    case 2:
                        r = _a.sent();
                        if (r.json().isValid)
                            this.errorMessage = this._TB('VALID: The Tax code or Fiscal code is correct.');
                        else
                            this.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
                        this.changeDetectorRef.detectChanges();
                        return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.getStato = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var stato;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        stato = this.isoCode;
                        if (!(stato == '' || stato == undefined)) return [3 /*break*/, 2];
                        return [4 /*yield*/, this.parameterService.getParameter('MA_CustSuppParameters.ISOCountryCode')];
                    case 1:
                        stato = _a.sent();
                        _a.label = 2;
                    case 2:
                        if (stato == '' || stato == undefined)
                            stato = 'IT';
                        return [2 /*return*/, stato];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.validate = /**
     * @return {?}
     */
    function () {
        this.errorMessage = '';
        if (!this.model)
            return;
        this.isFiscalCodeValid(this.model.value, this.isoCode);
    };
    /**
     * @param {?} fiscalcode
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.isTaxIdNumber = /**
     * @param {?} fiscalcode
     * @return {?}
     */
    function (fiscalcode) {
        if (!fiscalcode) {
            return false;
        }
        return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
    };
    /**
     * @param {?} fiscalcode
     * @param {?} country
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.isFiscalCodeValid = /**
     * @param {?} fiscalcode
     * @param {?} country
     * @return {?}
     */
    function (fiscalcode, country) {
        if (!this.isSupported(country)) {
            this.errorMessage = '';
            return;
        }
        if (!fiscalcode) {
            this.errorMessage = '';
            return;
        }
        switch (country) {
            case 'IT':
                this.FiscalCodeCheckIT(fiscalcode);
                break;
            case 'BR':
                this.FiscalCodeCheckBR(fiscalcode);
                break;
            case 'ES':
                this.FiscalCodeCheckES(fiscalcode);
                break;
        }
    };
    /**
     * @param {?} country
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.isSupported = /**
     * @param {?} country
     * @return {?}
     */
    function (country) {
        if (!country) {
            return false;
        }
        var /** @type {?} */ supported = [
            'IT',
            'BR',
            'ES'
        ];
        return !!supported.find(function (c) { return c === country.toUpperCase(); });
    };
    /**
     * @param {?} fiscalcode
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.FiscalCodeCheckIT = /**
     * @param {?} fiscalcode
     * @return {?}
     */
    function (fiscalcode) {
        if (this.isTaxIdNumber(fiscalcode)) {
            if (!JsCheckTaxId.isTaxIdValid(fiscalcode, this.isoCode))
                this.errorMessage = this._TB('Incorrect fiscal code number');
            return;
        }
        if (fiscalcode.length != 16) {
            this.errorMessage = this._TB('The fiscal code number must have at least 16 characters!');
            return;
        }
        var /** @type {?} */ regex = /[A-Za-z]{6}[0-9]{2}[A-Za-z][0-9]{2}[A-Za-z][0-9]{3}[A-Za-z]/g;
        var /** @type {?} */ m;
        if ((m = regex.exec(fiscalcode)) === null) {
            this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: AAA-AAA-NN-A-NN-ANNN-A");
            return;
        }
        var /** @type {?} */ tbc = [
            0,
            1, 0, 5, 7, 9,
            13, 15, 17, 19, 21,
            2, 4, 18, 20, 11,
            3, 6, 8, 12, 14,
            16, 10, 22, 25, 24,
            23, 1, 0, 5, 7,
            9, 13, 15, 17, 19,
            21
        ];
        var /** @type {?} */ array = Array.from(fiscalcode).map(function (c) { return c.toUpperCase().charCodeAt(0); });
        var /** @type {?} */ nOdd = 0;
        for (var /** @type {?} */ i = 0; i <= 14; i += 2) {
            var /** @type {?} */ n = array[i] - (array[i] < 58 ? 21 : 64);
            nOdd += tbc[n];
        }
        var /** @type {?} */ nEqual = 0;
        for (var /** @type {?} */ i = 1; i <= 13; i += 2) {
            nEqual += array[i] - (array[i] < 58 ? 48 : 65);
        }
        var /** @type {?} */ check = (nOdd + nEqual) % 26 + 65;
        if (check != array[15])
            this.errorMessage = this._TB('Incorrect fiscal code number');
    };
    /**
     * @param {?} fiscalcode
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.FiscalCodeCheckBR = /**
     * @param {?} fiscalcode
     * @return {?}
     */
    function (fiscalcode) {
        if (fiscalcode.length != 14) {
            this.errorMessage = this._TB('The fiscal code number must have at least 14 characters!');
            return;
        }
        var /** @type {?} */ regex = /([0-9]{3}[\.]){2}[0-9]{3}[-][0-9]{2}/g;
        // const str = `123.456.789-12`;
        var /** @type {?} */ m;
        if ((m = regex.exec(fiscalcode)) === null) {
            this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: NNN.NNN.NNN-NN");
            return;
        }
        var /** @type {?} */ array = Array.from(fiscalcode.replace(/[.]/g, '').replace(/[-]/g, '')).map(function (c) { return c.charCodeAt(0) - 48; });
        var /** @type {?} */ lSum = 0;
        for (var /** @type {?} */ i = 0; i < 9; i++) {
            lSum += array[8 - i] * (i + 2);
        }
        var /** @type {?} */ nCtrlDigit1 = 11 - lSum % 11;
        if (nCtrlDigit1 > 9)
            nCtrlDigit1 = 0;
        lSum = 0;
        for (var /** @type {?} */ i = 0; i < 10; i++) {
            lSum += array[9 - i] * (i + 2);
        }
        var /** @type {?} */ nCtrlDigit2 = 11 - lSum % 11;
        if (nCtrlDigit2 > 9)
            nCtrlDigit2 = 0;
        if (nCtrlDigit1 != array[9] || nCtrlDigit2 != array[10])
            this.errorMessage = this._TB('Incorrect fiscal code number');
    };
    /**
     * @param {?} fiscalcode
     * @return {?}
     */
    FiscalCodeEditComponent.prototype.FiscalCodeCheckES = /**
     * @param {?} fiscalcode
     * @return {?}
     */
    function (fiscalcode) {
        if (fiscalcode.length != 9) {
            this.errorMessage = this._TB('The fiscal code number must have 9 characters!');
            return;
        }
        var /** @type {?} */ regex = /[A-Za-z][0-9]{7}[A-Za-z]/g;
        // const str = `A1234567B`;
        var /** @type {?} */ m;
        if ((m = regex.exec(fiscalcode)) === null) {
            this.errorMessage = this._TB("The fiscal code number entered does not comply with the structure: X9999999X");
            return;
        }
    };
    Object.defineProperty(FiscalCodeEditComponent.prototype, "isValid", {
        get: /**
         * @return {?}
         */
        function () { return !this.errorMessage; },
        enumerable: true,
        configurable: true
    });
    FiscalCodeEditComponent.decorators = [
        { type: Component, args: [{
                    selector: 'erp-fiscalcode-edit',
                    template: "<div class=\"tb-control erp-fiscalcode-edit\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <input kendoTextBox maxlength=\"{{model?.length}}\" [ngModel]=\"model?.value | toUpper: model?.uppercase\" required=\"required\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"changeModelValue($event)\" [style.width.px]=\"width\" /> <tb-context-menu [contextMenu]=\"checkfiscalcodeContextMenu\"></tb-context-menu> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [".has-error { color: red; font-size: small; padding-left: 10px; } "]
                },] },
    ];
    /** @nocollapse */
    FiscalCodeEditComponent.ctorParameters = function () { return [
        { type: LayoutService, },
        { type: EventDataService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: ParameterService, },
        { type: Store, },
        { type: HttpService, },
        { type: CoreHttpService, },
        { type: Http, },
    ]; };
    FiscalCodeEditComponent.propDecorators = {
        "readonly": [{ type: Input, args: ['readonly',] },],
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
    };
    return FiscalCodeEditComponent;
}(ControlComponent));
export { FiscalCodeEditComponent };
function FiscalCodeEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    FiscalCodeEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    FiscalCodeEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    FiscalCodeEditComponent.propDecorators;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.readonly;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.slice;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.selector;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.errorMessage;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.ctrlEnabled;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.isMasterBR;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.isMasterIT;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.isEuropeanUnion;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.isoCode;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.checkfiscalcodeContextMenu;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.menuItemITCheck;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.menuItemEUCheck;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.menuItemBRCheck;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.eventData;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.parameterService;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.store;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.httpservice;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.httpCore;
    /** @type {?} */
    FiscalCodeEditComponent.prototype.http;
}
