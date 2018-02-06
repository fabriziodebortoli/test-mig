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
import { Injectable } from '@angular/core';
import { HttpService, ParameterService } from '@taskbuilder/core';
var ItemsHttpService = (function () {
    function ItemsHttpService(httpService, parametersService) {
        this.httpService = httpService;
        this.parametersService = parametersService;
        this.controllerRoute = '/erp-core/';
        this.DEFAULT_LEN_ITEM = 21;
    }
    /*parametri*/
    /**
     * @return {?}
     */
    ItemsHttpService.prototype.getItemInfo_CodeLength = /**
     * @return {?}
     */
    function () {
        return __awaiter(this, void 0, void 0, function () {
            var len;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.parametersService.getParameter('Ma_ItemParameters.CodeLength')];
                    case 1:
                        len = _a.sent();
                        if (len && (+len) > 3) {
                            return [2 /*return*/, +len];
                        }
                        else {
                            // TODO GIANLUCA
                            // lettura della lunghezza colonna da db....
                            // verificare se compatibile con oracle
                            // int nlen = (pColInfo) ? pColInfo->GetColumnLength() : DEFAULT_LEN_ITEM;
                            // TODO GIANLUCA
                            // lettura della lunghezza colonna da db....
                            // verificare se compatibile con oracle 
                            // int nlen = (pColInfo) ? pColInfo->GetColumnLength() : DEFAULT_LEN_ITEM;
                            return [2 /*return*/, this.DEFAULT_LEN_ITEM];
                        }
                        return [2 /*return*/];
                }
            });
        });
    };
    /**
     * @param {?} param
     * @return {?}
     */
    ItemsHttpService.prototype.queryParam = /**
     * @param {?} param
     * @return {?}
     */
    function (param) {
        return __awaiter(this, void 0, void 0, function () {
            var ret;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.parametersService.getParameter(param)];
                    case 1:
                        ret = _a.sent();
                        return [2 /*return*/, ret];
                }
            });
        });
    };
    /*interrogazioni varie*/
    /**
     * @param {?} queryType
     * @return {?}
     */
    ItemsHttpService.prototype.getItemsSearchList = /**
     * @param {?} queryType
     * @return {?}
     */
    function (queryType) {
        return this.httpService.execPost(this.controllerRoute, 'GetItemsSearchList', queryType);
    };
    ItemsHttpService.decorators = [
        { type: Injectable },
    ];
    /** @nocollapse */
    ItemsHttpService.ctorParameters = function () { return [
        { type: HttpService, },
        { type: ParameterService, },
    ]; };
    return ItemsHttpService;
}());
export { ItemsHttpService };
function ItemsHttpService_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ItemsHttpService.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ItemsHttpService.ctorParameters;
    /** @type {?} */
    ItemsHttpService.prototype.controllerRoute;
    /** @type {?} */
    ItemsHttpService.prototype.DEFAULT_LEN_ITEM;
    /** @type {?} */
    ItemsHttpService.prototype.httpService;
    /** @type {?} */
    ItemsHttpService.prototype.parametersService;
}
