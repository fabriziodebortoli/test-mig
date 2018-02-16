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
import { ChangeDetectionStrategy, Component, Input, ViewContainerRef, ChangeDetectorRef } from '@angular/core';
import { Store, ControlComponent, TbComponentService, LayoutService, ParameterService } from '@taskbuilder/core';
import { ItemsHttpService } from '../../../core/services/items/items-http.service';
var ItemEditComponent = (function (_super) {
    __extends(ItemEditComponent, _super);
    function ItemEditComponent(vcr, layoutService, tbComponentService, changeDetectorRef, store, http, parameterService) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.vcr = vcr;
        _this.store = store;
        _this.http = http;
        _this.parameterService = parameterService;
        _this.maxLength = -1;
        _this.itemsAutoNumbering = true;
        return _this;
    }
    /**
     * @return {?}
     */
    ItemEditComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        this.readParams();
    };
    /**
     * @return {?}
     */
    ItemEditComponent.prototype.readParams = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var _a, result;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _a = this;
                        return [4 /*yield*/, this.http.getItemInfo_CodeLength()];
                    case 1:
                        _a.maxLength = _b.sent();
                        return [4 /*yield*/, this.parameterService.getParameter('MA_ItemParameters.ItemAutoNum')];
                    case 2:
                        result = _b.sent();
                        this.itemsAutoNumbering = (result == '1');
                        return [2 /*return*/];
                }
            });
        });
    };
    ItemEditComponent.decorators = [
        { type: Component, args: [{
                    selector: "erp-item-edit",
                    template: "<tb-numberer [slice]=\"slice\" [selector]=\"selector\" [model]=\"model\" [popUpMenu]=\"itemsAutoNumbering\" [maxLength]=\"maxLength\"></tb-numberer> <ng-template [ngIf]=\"hotLink\"> <ng-template [tbHotLink]=\"hotLink\"></ng-template> </ng-template>",
                    styles: [""],
                    changeDetection: ChangeDetectionStrategy.OnPush
                },] },
    ];
    /** @nocollapse */
    ItemEditComponent.ctorParameters = function () { return [
        { type: ViewContainerRef, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: Store, },
        { type: ItemsHttpService, },
        { type: ParameterService, },
    ]; };
    ItemEditComponent.propDecorators = {
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
        "hotLink": [{ type: Input },],
    };
    return ItemEditComponent;
}(ControlComponent));
export { ItemEditComponent };
function ItemEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ItemEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ItemEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    ItemEditComponent.propDecorators;
    /** @type {?} */
    ItemEditComponent.prototype.slice;
    /** @type {?} */
    ItemEditComponent.prototype.selector;
    /** @type {?} */
    ItemEditComponent.prototype.hotLink;
    /** @type {?} */
    ItemEditComponent.prototype.maxLength;
    /** @type {?} */
    ItemEditComponent.prototype.itemsAutoNumbering;
    /** @type {?} */
    ItemEditComponent.prototype.vcr;
    /** @type {?} */
    ItemEditComponent.prototype.store;
    /** @type {?} */
    ItemEditComponent.prototype.http;
    /** @type {?} */
    ItemEditComponent.prototype.parameterService;
}
