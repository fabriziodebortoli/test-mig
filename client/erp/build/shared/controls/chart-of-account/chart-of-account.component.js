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
import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
var ChartOfAccountComponent = (function (_super) {
    __extends(ChartOfAccountComponent, _super);
    function ChartOfAccountComponent(eventData, layoutService, tbComponentService, changeDetectorRef) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.errorMessage = '';
        return _this;
    }
    /**
     * @return {?}
     */
    ChartOfAccountComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        console.log('OnInit');
    };
    /**
     * @param {?} value
     * @return {?}
     */
    ChartOfAccountComponent.prototype.changeModelValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this.model.value = value;
    };
    /**
     * @return {?}
     */
    ChartOfAccountComponent.prototype.onBlur = /**
     * @return {?}
     */
    function () {
    };
    ChartOfAccountComponent.decorators = [
        { type: Component, args: [{
                    selector: 'erp-chart-of-account',
                    template: "<div class=\"tb-control erp-hotlink-buttons\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <input width=\"80\" style=\"margin-right: 4px\" [ngModel]=\"model?.value\" required=\"required\" (blur)=\"onBlur()\"> <ng-template [ngIf]=\"hotLink\"> <ng-template [tbHotLink]=\"hotLink\" ></ng-template> </ng-template> <div class=\"has-error\">{{errorMessage}}</div> </div>",
                    styles: [""],
                    changeDetection: ChangeDetectionStrategy.OnPush
                },] },
    ];
    /** @nocollapse */
    ChartOfAccountComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
    ]; };
    ChartOfAccountComponent.propDecorators = {
        "hotLink": [{ type: Input },],
        "selector": [{ type: Input },],
        "slice$": [{ type: Input },],
    };
    return ChartOfAccountComponent;
}(ControlComponent));
export { ChartOfAccountComponent };
function ChartOfAccountComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    ChartOfAccountComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    ChartOfAccountComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    ChartOfAccountComponent.propDecorators;
    /** @type {?} */
    ChartOfAccountComponent.prototype.hotLink;
    /** @type {?} */
    ChartOfAccountComponent.prototype.errorMessage;
    /** @type {?} */
    ChartOfAccountComponent.prototype.selector;
    /** @type {?} */
    ChartOfAccountComponent.prototype.slice$;
    /** @type {?} */
    ChartOfAccountComponent.prototype.eventData;
}
