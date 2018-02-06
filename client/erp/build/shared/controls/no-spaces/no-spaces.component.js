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
import { Component, Input, ChangeDetectorRef, ViewChild } from '@angular/core';
import { ControlComponent } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { TbComponentService } from '@taskbuilder/core';
import { LayoutService } from '@taskbuilder/core';
import { Store } from '@taskbuilder/core';
var NoSpacesEditComponent = (function (_super) {
    __extends(NoSpacesEditComponent, _super);
    function NoSpacesEditComponent(eventData, layoutService, tbComponentService, changeDetectorRef, store) {
        var _this = _super.call(this, layoutService, tbComponentService, changeDetectorRef) || this;
        _this.eventData = eventData;
        _this.store = store;
        _this.INITIAL_SEGMENT_LENGTH = 8;
        _this.readonly = false;
        _this.errorMessage = '';
        _this.subscribedToSelector = false;
        _this.maxLength = -1;
        return _this;
    }
    /**
     * @return {?}
     */
    NoSpacesEditComponent.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        this.textbox.input.nativeElement.maxLength = this.INITIAL_SEGMENT_LENGTH;
    };
    /**
     * @param {?} changes
     * @return {?}
     */
    NoSpacesEditComponent.prototype.ngOnChanges = /**
     * @param {?} changes
     * @return {?}
     */
    function (changes) {
        this.subscribeToSelector();
    };
    /**
     * @return {?}
     */
    NoSpacesEditComponent.prototype.subscribeToSelector = /**
     * @return {?}
     */
    function () {
        var _this = this;
        // maxLength is an optional parameter, i may not have to use it.
        // It is also the only parameter, so i have no selector without it
        if (!this.subscribedToSelector && this.store && this.selector) {
            this.store
                .select(this.selector)
                .select('maxLength')
                .subscribe(function (v) {
                if (v) {
                    _this.maxLength = v;
                    _this.textbox.input.nativeElement.maxLength = v;
                }
            });
            this.subscribedToSelector = true;
        }
    };
    /**
     * @param {?} $event
     * @return {?}
     */
    NoSpacesEditComponent.prototype.onKeyDown = /**
     * @param {?} $event
     * @return {?}
     */
    function ($event) {
        if ($event.keyCode === 32) {
            $event.preventDefault();
        }
    };
    /**
     * @return {?}
     */
    NoSpacesEditComponent.prototype.removeSpaces = /**
     * @return {?}
     */
    function () {
        if (this.model && this.model.value)
            this.model.value = this.model.value.replace(/\s+/g, '');
        if (this.maxLength > 0 && this.model.value.length !== this.maxLength)
            this.errorMessage = this._TB('Value length must be of ') + this.maxLength + this._TB(' chars');
        else
            this.errorMessage = '';
        this.eventData.change.emit(this.cmpId);
        this.blur.emit(this);
    };
    NoSpacesEditComponent.decorators = [
        { type: Component, args: [{
                    selector: 'erp-no-spaces',
                    template: "<div class=\"tb-control no-spaces\" [ngClass]=\"componentClass()\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <div class=\"group\"> <kendo-maskedtextbox #textbox [ngModel]=\"model?.value\" required=\"required\" (blur)=\"removeSpaces()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"model.value=$event\" [style.width.px]=\"width\" (keydown)=\"onKeyDown($event)\"> <ng-template [ngIf]=\"hotLink\"> <ng-template [tbHotLink]=\"hotLink\"></ng-template> </ng-template> </kendo-maskedtextbox> <p>{{errorMessage}}</p> </div> </div>",
                    styles: [""]
                },] },
    ];
    /** @nocollapse */
    NoSpacesEditComponent.ctorParameters = function () { return [
        { type: EventDataService, },
        { type: LayoutService, },
        { type: TbComponentService, },
        { type: ChangeDetectorRef, },
        { type: Store, },
    ]; };
    NoSpacesEditComponent.propDecorators = {
        "readonly": [{ type: Input, args: ['readonly',] },],
        "slice": [{ type: Input },],
        "selector": [{ type: Input },],
        "hotLink": [{ type: Input },],
        "textbox": [{ type: ViewChild, args: ['textbox',] },],
    };
    return NoSpacesEditComponent;
}(ControlComponent));
export { NoSpacesEditComponent };
function NoSpacesEditComponent_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    NoSpacesEditComponent.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    NoSpacesEditComponent.ctorParameters;
    /** @type {!Object<string,!Array<{type: !Function, args: (undefined|!Array<?>)}>>} */
    NoSpacesEditComponent.propDecorators;
    /** @type {?} */
    NoSpacesEditComponent.prototype.INITIAL_SEGMENT_LENGTH;
    /** @type {?} */
    NoSpacesEditComponent.prototype.readonly;
    /** @type {?} */
    NoSpacesEditComponent.prototype.slice;
    /** @type {?} */
    NoSpacesEditComponent.prototype.selector;
    /** @type {?} */
    NoSpacesEditComponent.prototype.hotLink;
    /** @type {?} */
    NoSpacesEditComponent.prototype.textbox;
    /** @type {?} */
    NoSpacesEditComponent.prototype.errorMessage;
    /** @type {?} */
    NoSpacesEditComponent.prototype.subscribedToSelector;
    /** @type {?} */
    NoSpacesEditComponent.prototype.maxLength;
    /** @type {?} */
    NoSpacesEditComponent.prototype.eventData;
    /** @type {?} */
    NoSpacesEditComponent.prototype.store;
}
