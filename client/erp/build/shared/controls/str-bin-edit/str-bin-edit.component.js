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
import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { WmsHttpService } from '../../../core/services/wms/wms-http.service';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
var StrBinEditComponent = (function (_super) {
    __extends(StrBinEditComponent, _super);
    function StrBinEditComponent(eventData, layoutService, tbComponentService, changeDetectorRef, http, store) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.http = http;
        _this.store = store;
        _this.mask = '';
        _this.errorMessage = '';
        return _this;
    }
    /**
     * @param {?} changes
     * @return {?}
     */
    StrBinEditComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        return __awaiter(this, void 0, void 0, function () {
            var r;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!changes) return [3 /*break*/, 2];
                        return [4 /*yield*/, this.http.checkBinUsesStructure(changes.slice.zone, changes.slice.storage).toPromise()];
                    case 1:
                        r = _a.sent();
                        if (r.json().UseBinStructure) {
                            this.mask = this.convertMask(changes.slice.formatter, changes.slice.separator, changes.slice.maskChar);
                        }
                        _a.label = 2;
                    case 2: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @param {?} mask
     * @param {?} separator
     * @param {?} maskChar
     * @return {?}
     */
    StrBinEditComponent.prototype.convertMask = /**
     * @param {?} mask
     * @param {?} separator
     * @param {?} maskChar
     * @return {?}
     */
    function (mask, separator, maskChar) {
        if (['0', '9', '#', 'L', 'A', 'a', '&', 'C', '?'].indexOf(separator) > -1)
            mask = mask.replace(new RegExp(separator, 'g'), '\\' + separator);
        return mask
            .replace(new RegExp(maskChar, 'g'), 'A');
    };
    /**
     * @param {?} value
     * @return {?}
     */
    StrBinEditComponent.prototype.changeModelValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this.model.value = value;
    };
    /**
     * @param {?} $event
     * @return {?}
     */
    StrBinEditComponent.prototype.onKeyDown = /**
     * @param {?} $event
     * @return {?}
     */
    function ($event) {
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
            $event.preventDefault();
        }
    };
    StrBinEditComponent.decorators = [
        { type: Component, args: [{
                    selector: "erp-strbinedit",
                    template: "<div class=\"tb-control erp-strbinedit\" [ngClass]=\"componentClass()\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <kendo-maskedtextbox [ngModel]=\"model?.value\" required=\"required\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"changeModelValue($event)\" (keydown)=\"onKeyDown($event)\" [style.width.px]=\"width\"></kendo-maskedtextbox> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [""]
                },] },
    ];
    /** @nocollapse */
    StrBinEditComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: WmsHttpService, },
        { type: Store, },
    ]; };
    StrBinEditComponent.propDecorators = {
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
    };
    return StrBinEditComponent;
}(ControlComponent));
export { StrBinEditComponent };
function StrBinEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    StrBinEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    StrBinEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    StrBinEditComponent.propDecorators;
    /** @type {?} */
    StrBinEditComponent.prototype.slice;
    /** @type {?} */
    StrBinEditComponent.prototype.selector;
    /** @type {?} */
    StrBinEditComponent.prototype.mask;
    /** @type {?} */
    StrBinEditComponent.prototype.errorMessage;
    /** @type {?} */
    StrBinEditComponent.prototype.eventData;
    /** @type {?} */
    StrBinEditComponent.prototype.http;
    /** @type {?} */
    StrBinEditComponent.prototype.store;
}
