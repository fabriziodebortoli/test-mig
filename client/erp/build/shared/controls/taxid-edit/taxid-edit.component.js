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
var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
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
import { FormMode, ContextMenuItem, Store, TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { DataService, HttpService, ParameterService, MessageDlgArgs } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { CoreHttpService } from '../../../core/services/core/core-http.service';
import { Http, URLSearchParams } from '@angular/http';
import * as moment from 'moment';
import JsCheckTaxId from './jscheckTaxIDNumber';
var TaxIdEditComponent = (function (_super) {
    __extends(TaxIdEditComponent, _super);
    function TaxIdEditComponent(layoutService, eventData, dataService, tbComponentService, changeDetectorRef, parameterService, store, httpservice, httpCore, http) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.dataService = dataService;
        _this.parameterService = parameterService;
        _this.store = store;
        _this.httpservice = httpservice;
        _this.httpCore = httpCore;
        _this.http = http;
        _this.readonly = false;
        _this.ctrlEnabled = false;
        _this.isMasterBR = false;
        _this.isMasterIT = false;
        _this.isMasterRO = false;
        _this.isEuropeanUnion = false;
        _this.naturalPerson = false;
        _this.isoCode = '';
        _this.checktaxidcodeContextMenu = [];
        _this.menuItemITCheck = new ContextMenuItem(_this._TB('Check TaxId existence (IT site)'), '', true, false, null, _this.checkIT.bind(_this));
        _this.menuItemEUCheck = new ContextMenuItem(_this._TB('Check TaxId existence (EU site)'), '', true, false, null, _this.checkEU.bind(_this));
        _this.menuItemBRCheck = new ContextMenuItem(_this._TB('Check Fiscal Code existence'), '', true, false, null, _this.checkBR.bind(_this));
        _this.menuItemROCheck = new ContextMenuItem(_this._TB('Tax Status Check'), '', true, false, null, _this.checkRO.bind(_this));
        return _this;
    }
    /**
     * @param {?} changes
     * @return {?}
     */
    TaxIdEditComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        if (changes.slice) {
            if (changes.slice.isoCode) {
                this.isoCode = changes.slice.isoCode.value;
                this.buildContextMenu();
            }
            if (changes.slice.naturalPerson) {
                this.naturalPerson = changes.slice.naturalPerson.value;
                this.buildContextMenu();
            }
        }
        this.validate();
    };
    /**
     * @return {?}
     */
    TaxIdEditComponent.prototype.ngOnInit = /**
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
        this.httpservice.isActivated('ERP', 'MasterData_RO').take(1).subscribe(function (res) { _this.isMasterRO = res.result; });
        this.httpservice.isActivated('ERP', 'EuropeanUnion').take(1).subscribe(function (res) { _this.isEuropeanUnion = res.result; });
    };
    /**
     * @param {?} message
     * @return {?}
     */
    TaxIdEditComponent.prototype.openMessageDialog = /**
     * @param {?} message
     * @return {?}
     */
    function (message) {
        var /** @type {?} */ args = new MessageDlgArgs();
        this.eventData.openMessageDialog.emit(__assign({}, args, { yes: true, no: true, text: message }));
        // this.eventData.closeMessageDialog.take(1).subscribe(s => console.log(s));
        return this.eventData.closeMessageDialog.take(1).toPromise();
    };
    /**
     * @param {?} formMode
     * @return {?}
     */
    TaxIdEditComponent.prototype.onFormModeChanged = /**
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
    TaxIdEditComponent.prototype.buildContextMenu = /**
     * @return {?}
     */
    function () {
        this.checktaxidcodeContextMenu.splice(0, this.checktaxidcodeContextMenu.length);
        if (!this.ctrlEnabled)
            return;
        if (this.isTaxIdField(this.model.value, false))
            this.checktaxidcodeContextMenu.push(this.menuItemITCheck);
        if (this.isMasterRO && this.isoCode === 'RO' && !this.naturalPerson && this.isTaxIdField(this.model.value, false))
            this.checktaxidcodeContextMenu.push(this.menuItemROCheck);
        if (this.isEuropeanUnion && this.isTaxIdField(this.model.value, false))
            this.checktaxidcodeContextMenu.push(this.menuItemEUCheck);
        if (this.isMasterBR && this.isoCode === 'BR')
            this.checktaxidcodeContextMenu.push(this.menuItemBRCheck);
    };
    /**
     * @param {?} code
     * @param {?} all
     * @return {?}
     */
    TaxIdEditComponent.prototype.isTaxIdField = /**
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
    TaxIdEditComponent.prototype.onBlur = /**
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
    TaxIdEditComponent.prototype.changeModelValue = /**
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
    TaxIdEditComponent.prototype.checkIT = /**
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
    TaxIdEditComponent.prototype.checkBR = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var url, newWindow;
            return __generator(this, function (_a) {
                url = 'https://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/cnpjreva_solicitacao.asp';
                newWindow = window.open(url, 'blank');
                return [2 /*return*/];
            });
        });
    };
    // '23260646'; taxid RO
    /**
     * @return {?}
     */
    TaxIdEditComponent.prototype.checkRO = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var now, today, vatCode, r, found, exc_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        now = moment();
                        today = now.format('YYYY-MM-DD');
                        vatCode = this.model.value.replace(/([\D])/g, '');
                        if (vatCode.length > 9 || vatCode.length === 0) {
                            this.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
                            return [2 /*return*/];
                        }
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this.httpCore.checkVatRO(this.model.value, today).toPromise()];
                    case 2:
                        r = _a.sent();
                        found = r.json().found;
                        if (found.length) {
                            this.errorMessage = this._TB('VALID: The Tax code or Fiscal code is correct.');
                            this.fillFields(found);
                        }
                        else {
                            this.errorMessage = this._TB('INVALID: Incorrect Tax code or fiscal code.');
                        }
                        return [3 /*break*/, 4];
                    case 3:
                        exc_1 = _a.sent();
                        this.errorMessage = exc_1;
                        return [3 /*break*/, 4];
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    TaxIdEditComponent.prototype.checkEU = /**
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
     * @param {?} result
     * @return {?}
     */
    TaxIdEditComponent.prototype.fillFields = /**
     * @param {?} result
     * @return {?}
     */
    function (result) {
        return __awaiter(this, void 0, void 0, function () {
            var slice, company, exTemp, reg, splitAddress_1, data, county, city, address, taxExempt, message;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.store.select(this.selector).take(1).toPromise()];
                    case 1:
                        slice = _a.sent();
                        if (!slice.companyName) return [3 /*break*/, 4];
                        company = result[0].denumire;
                        exTemp = result[0].tva;
                        reg = /(MUN.\s+|MUNICIPUL\s+|MUNICIPIUL\s+|JUD.\s+|JUDETUL\s+|)/g;
                        splitAddress_1 = (/** @type {?} */ (result[0].adresa)).replace(reg, '').split(',');
                        return [4 /*yield*/, this.dataService.getData('DataFile.ERP.Company.County', 'code', new URLSearchParams()).take(1).toPromise()];
                    case 2:
                        data = _a.sent();
                        county = void 0;
                        if (data !== undefined) {
                            county = data.rows.find(function (x) { return x.Description === splitAddress_1[0]; });
                        }
                        if (county === undefined)
                            county = { County: '' };
                        city = splitAddress_1[1];
                        address = '';
                        if (splitAddress_1[2] !== '' || splitAddress_1[3] !== '') {
                            address = splitAddress_1[2];
                            if (address !== '' && splitAddress_1[3] !== '') {
                                address = address + ', ' + splitAddress_1[3];
                            }
                        }
                        taxExempt = exTemp === 'true' ? this._TB('Tax Exempt') : this._TB('Tax Subject');
                        message = this._TB('Tax Number found:\n{0}\n{1},{2},{3}\n{4}\nDo you want to overwrite data?', company, address, city, county.County, taxExempt);
                        return [4 /*yield*/, this.openMessageDialog(message)];
                    case 3:
                        if ((_a.sent()).yes) {
                            if (slice.companyName) {
                                slice.companyName.value = company;
                            }
                            if (slice.address) {
                                slice.address.value = address;
                            }
                            if (slice.city) {
                                slice.city.value = city;
                            }
                            if (slice.county) {
                                slice.county.value = county.County;
                            }
                            this.changeDetectorRef.detectChanges();
                        }
                        _a.label = 4;
                    case 4: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    TaxIdEditComponent.prototype.getStato = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var stato;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        stato = this.isoCode;
                        if (!(stato === '' || stato === undefined)) return [3 /*break*/, 2];
                        return [4 /*yield*/, this.parameterService.getParameter('MA_CustSuppParameters.ISOCountryCode')];
                    case 1:
                        stato = _a.sent();
                        _a.label = 2;
                    case 2:
                        if (stato === '' || stato === undefined)
                            stato = 'IT';
                        return [2 /*return*/, stato];
                }
            });
        });
    };
    /**
     * @return {?}
     */
    TaxIdEditComponent.prototype.validate = /**
     * @return {?}
     */
    function () {
        this.errorMessage = '';
        if (!this.model)
            return;
        if (!JsCheckTaxId.isTaxIdValid(this.model.value, this.isoCode))
            this.errorMessage = this._TB('Incorrect Tax Number');
    };
    /**
     * @param {?} fiscalcode
     * @return {?}
     */
    TaxIdEditComponent.prototype.isTaxIdNumber = /**
     * @param {?} fiscalcode
     * @return {?}
     */
    function (fiscalcode) {
        if (!fiscalcode) {
            return false;
        }
        return fiscalcode.charAt(0) >= '0' && fiscalcode.charAt(0) <= '9';
    };
    Object.defineProperty(TaxIdEditComponent.prototype, "isValid", {
        get: /**
         * @return {?}
         */
        function () { return !this.errorMessage; },
        enumerable: true,
        configurable: true
    });
    TaxIdEditComponent.decorators = [
        { type: Component, args: [{
                    selector: 'erp-taxid-edit',
                    template: "<div class=\"tb-control erp-taxid-edit\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <input kendoTextBox maxlength=\"{{model?.length}}\" [ngModel]=\"model?.value | toUpper: model?.uppercase\" required=\"required\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"changeModelValue($event)\" [style.width.px]=\"width\" /> <tb-context-menu [contextMenu]=\"checktaxidcodeContextMenu\"></tb-context-menu> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [".has-error { color: red; font-size: small; padding-left: 10px; } "]
                },] },
    ];
    /** @nocollapse */
    TaxIdEditComponent.ctorParameters = function () { return [
        { type: LayoutService, },
        { type: EventDataService, },
        { type: DataService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: ParameterService, },
        { type: Store, },
        { type: HttpService, },
        { type: CoreHttpService, },
        { type: Http, },
    ]; };
    TaxIdEditComponent.propDecorators = {
        "readonly": [{ type: Input, args: ['readonly',] },],
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
    };
    return TaxIdEditComponent;
}(ControlComponent));
export { TaxIdEditComponent };
function TaxIdEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    TaxIdEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    TaxIdEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    TaxIdEditComponent.propDecorators;
    /** @type {?} */
    TaxIdEditComponent.prototype.readonly;
    /** @type {?} */
    TaxIdEditComponent.prototype.slice;
    /** @type {?} */
    TaxIdEditComponent.prototype.selector;
    /** @type {?} */
    TaxIdEditComponent.prototype.errorMessage;
    /** @type {?} */
    TaxIdEditComponent.prototype.ctrlEnabled;
    /** @type {?} */
    TaxIdEditComponent.prototype.isMasterBR;
    /** @type {?} */
    TaxIdEditComponent.prototype.isMasterIT;
    /** @type {?} */
    TaxIdEditComponent.prototype.isMasterRO;
    /** @type {?} */
    TaxIdEditComponent.prototype.isEuropeanUnion;
    /** @type {?} */
    TaxIdEditComponent.prototype.naturalPerson;
    /** @type {?} */
    TaxIdEditComponent.prototype.isoCode;
    /** @type {?} */
    TaxIdEditComponent.prototype.checktaxidcodeContextMenu;
    /** @type {?} */
    TaxIdEditComponent.prototype.menuItemITCheck;
    /** @type {?} */
    TaxIdEditComponent.prototype.menuItemEUCheck;
    /** @type {?} */
    TaxIdEditComponent.prototype.menuItemBRCheck;
    /** @type {?} */
    TaxIdEditComponent.prototype.menuItemROCheck;
    /** @type {?} */
    TaxIdEditComponent.prototype.eventData;
    /** @type {?} */
    TaxIdEditComponent.prototype.dataService;
    /** @type {?} */
    TaxIdEditComponent.prototype.parameterService;
    /** @type {?} */
    TaxIdEditComponent.prototype.store;
    /** @type {?} */
    TaxIdEditComponent.prototype.httpservice;
    /** @type {?} */
    TaxIdEditComponent.prototype.httpCore;
    /** @type {?} */
    TaxIdEditComponent.prototype.http;
}
