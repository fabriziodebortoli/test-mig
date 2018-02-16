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
import * as u from '../../../core/u/helpers';
var NumberEditWithFillerComponent = (function (_super) {
    __extends(NumberEditWithFillerComponent, _super);
    function NumberEditWithFillerComponent(eventData, layoutService, tbComponentService, changeDetectorRef) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.enableFilling = true;
        _this.maxLength = 20;
        _this.fillerDigit = '0';
        _this.minLen = 6;
        return _this;
    }
    /**
     * @param {?} e
     * @return {?}
     */
    NumberEditWithFillerComponent.prototype.onPasting = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (!u.ClipboardEventHelper.isNumber(e)) {
            this.errorMessage = this._TB('Only numbers admitted.');
            e.preventDefault();
        }
    };
    /**
     * @param {?} e
     * @return {?}
     */
    NumberEditWithFillerComponent.prototype.onTyping = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (!u.KeyboardEventHelper.isNumber(e))
            e.preventDefault();
    };
    /**
     * @param {?} value
     * @return {?}
     */
    NumberEditWithFillerComponent.prototype.changeModelValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this.model.value = value;
    };
    /**
     * @return {?}
     */
    NumberEditWithFillerComponent.prototype.onBlur = /**
     * @return {?}
     */
    function () {
        if (this.model.value.length === 0 || this.model.value.length >= this.maxLength) {
            this.blur.emit(this);
            this.eventData.change.emit(this.cmpId);
            return;
        }
        if (this.enableFilling && this.model.value.length < this.minLen) {
            var /** @type {?} */ filler = '';
            for (var /** @type {?} */ i = 0; i < (this.minLen - this.model.value.length); i++) {
                filler = filler + this.fillerDigit;
            }
            this.model.value = filler + this.model.value;
            this.model.value = this.model.value.substring(0, this.maxLength);
            this.blur.emit(this);
            this.eventData.change.emit(this.cmpId);
        }
    };
    NumberEditWithFillerComponent.decorators = [
        { type: Component, args: [{
                    selector: 'tb-number-edit-with-filler',
                    template: "<div class=\"tb-control\" [ngClass]=\"componentClass()\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input kendoTextBox required=\"required\" (keypress)=\"onTyping($event)\" (paste)=\"onPasting($event)\" [ngModel]=\"model.value\" (blur)=\"onBlur()\" (ngModelChange)=\"changeModelValue($event)\" [disabled]=\"!model?.enabled\" [style.width.px]=\"width\" /> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [""]
                },] },
    ];
    /** @nocollapse */
    NumberEditWithFillerComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
    ]; };
    NumberEditWithFillerComponent.propDecorators = {
        "enableFilling": [{ type: Input },],
        "maxLength": [{ type: Input },],
        "fillerDigit": [{ type: Input },],
        "minLen": [{ type: Input },],
    };
    return NumberEditWithFillerComponent;
}(ControlComponent));
export { NumberEditWithFillerComponent };
function NumberEditWithFillerComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    NumberEditWithFillerComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    NumberEditWithFillerComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    NumberEditWithFillerComponent.propDecorators;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.enableFilling;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.maxLength;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.fillerDigit;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.minLen;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.errorMessage;
    /** @type {?} */
    NumberEditWithFillerComponent.prototype.eventData;
}
