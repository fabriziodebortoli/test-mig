import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
export class BodyEditComponent extends ControlComponent {
    constructor() {
        super();
    }
}
BodyEditComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-body-edit',
                template: "<kendo-grid [kendoGridBinding]=\"model\"> <kendo-grid-column *ngFor=\"let column of columns\" field=\"{{column?.binding?.datasource}}\" title=\"{{column?.text}}\" width=\"200\"> <template kendoGridCellTemplate let-dataItem> <div>{{dataItem[column.binding.datasource]?.value}}</div> </template> </kendo-grid-column> </kendo-grid>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
BodyEditComponent.ctorParameters = () => [];
BodyEditComponent.propDecorators = {
    'columns': [{ type: Input },],
};
function BodyEditComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    BodyEditComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    BodyEditComponent.ctorParameters;
    /** @type {?} */
    BodyEditComponent.propDecorators;
    /** @type {?} */
    BodyEditComponent.prototype.columns;
}
