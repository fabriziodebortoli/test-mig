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
import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import Esr from './esr';
import * as u from '../../../core/u/helpers';
var EsrComponent = (function (_super) {
    __extends(EsrComponent, _super);
    function EsrComponent(eventData, layoutService, tbComponentService, changeDetectorRef) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.minLen = 0;
        _this.errorMessage = '';
        _this.minLen = 0;
        return _this;
    }
    /**
     * @param {?} e
     * @return {?}
     */
    EsrComponent.prototype.onPasting = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (!u.ClipboardEventHelper.isNotAlphanumeric(e)) {
            this.errorMessage = this._TB('Only numbers admitted.');
            e.preventDefault();
        }
    };
    /**
     * @param {?} e
     * @return {?}
     */
    EsrComponent.prototype.onTyping = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        this.errorMessage = '';
        if (!u.KeyboardEventHelper.isNotAlphanumeric(e))
            e.preventDefault();
    };
    /**
     * @param {?} value
     * @return {?}
     */
    EsrComponent.prototype.changeModelValue = /**
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
    EsrComponent.prototype.onBlur = /**
     * @return {?}
     */
    function () {
        this.validate();
    };
    /**
     * @return {?}
     */
    EsrComponent.prototype.validate = /**
     * @return {?}
     */
    function () {
        if (!this.model)
            return;
        this.errorMessage = '';
        var /** @type {?} */ r = Esr.checkEsrDigit(this.model.value);
        if (!r.result)
            this.errorMessage = r.error;
    };
    Object.defineProperty(EsrComponent.prototype, "isValid", {
        get: /**
         * @return {?}
         */
        function () { return !this.errorMessage; },
        enumerable: true,
        configurable: true
    });
    EsrComponent.decorators = [
        { type: Component, args: [{
                    selector: 'erp-esr',
                    template: "<div class=\"tb-control erp-esr\" [ngClass]=\"componentClass()\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <input kendoTextBox [ngModel]=\"model?.value\" required=\"required\" (blur)=\"onBlur()\" (keypress)=\"onTyping($event)\" (paste)=\"onPasting($event)\" (ngModelChange)=\"changeModelValue($event)\" [disabled]=\"!(model?.enabled)\" /> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [""]
                },] },
    ];
    /** @nocollapse */
    EsrComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
    ]; };
    EsrComponent.propDecorators = {
        "minLen": [{ type: Input },],
    };
    return EsrComponent;
}(ControlComponent));
export { EsrComponent };
function EsrComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    EsrComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    EsrComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    EsrComponent.propDecorators;
    /** @type {?} */
    EsrComponent.prototype.minLen;
    /** @type {?} */
    EsrComponent.prototype.errorMessage;
    /** @type {?} */
    EsrComponent.prototype.eventData;
}
