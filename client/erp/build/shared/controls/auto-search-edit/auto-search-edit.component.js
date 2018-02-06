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
import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { ItemsHttpService } from '../../../core/services/items/items-http.service';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { StringUtils } from './../../../core/u/string-utils';
var AutoSearchEditComponent = (function (_super) {
    __extends(AutoSearchEditComponent, _super);
    function AutoSearchEditComponent(eventData, layoutService, tbComponentService, changeDetectorRef, http, store) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.http = http;
        _this.store = store;
        _this.items = [];
        _this.filterText = '';
        return _this;
    }
    /**
     * @param {?} item
     * @return {?}
     */
    AutoSearchEditComponent.prototype.formatItem = /**
     * @param {?} item
     * @return {?}
     */
    function (item) {
        return StringUtils.pad(item.key, 8) + item.value;
    };
    /**
     * @param {?} changes
     * @return {?}
     */
    AutoSearchEditComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        if (changes.slice) {
            this.filterText = changes.slice.filterText;
            this.items = changes.slice.items;
        }
    };
    /**
     * @return {?}
     */
    AutoSearchEditComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        this.items = this.model.value;
    };
    AutoSearchEditComponent.decorators = [
        { type: Component, args: [{
                    selector: "erp-auto-search-edit",
                    template: "<select size=\"6\"> <option *ngFor=\"let item of items | keyValueFilter: filterText\">{{formatItem(item)}}</option> </select> ",
                    styles: [""]
                },] },
    ];
    /** @nocollapse */
    AutoSearchEditComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: ItemsHttpService, },
        { type: Store, },
    ]; };
    AutoSearchEditComponent.propDecorators = {
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
    };
    return AutoSearchEditComponent;
}(ControlComponent));
export { AutoSearchEditComponent };
function AutoSearchEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    AutoSearchEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    AutoSearchEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    AutoSearchEditComponent.propDecorators;
    /** @type {?} */
    AutoSearchEditComponent.prototype.slice;
    /** @type {?} */
    AutoSearchEditComponent.prototype.selector;
    /** @type {?} */
    AutoSearchEditComponent.prototype.items;
    /** @type {?} */
    AutoSearchEditComponent.prototype.filterText;
    /** @type {?} */
    AutoSearchEditComponent.prototype.eventData;
    /** @type {?} */
    AutoSearchEditComponent.prototype.http;
    /** @type {?} */
    AutoSearchEditComponent.prototype.store;
}
