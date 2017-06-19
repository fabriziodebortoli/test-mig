import { Component, Input } from '@angular/core';
import { DataService } from './../../services/data.service';
export class GridComponent {
    /**
     * @param {?} dataService
     */
    constructor(dataService) {
        this.dataService = dataService;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        // this.dataService.getColumns(this.gridNamespace, this.gridSelectionType).subscribe(columns => this.gridColumns = columns);
        this.dataSubscription = this.dataService.getData(this.gridNamespace, this.gridSelectionType, this.gridParams).subscribe((data) => {
            this.gridColumns = data.titles;
            this.gridData = data.rows;
        });
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.dataSubscription.unsubscribe();
    }
}
GridComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-grid',
                template: "<kendo-grid [data]=\"gridData\" [height]=\"370\"> <kendo-grid-column *ngFor=\"let col of gridColumns\" field=\"{{col}}\" title=\"{{col}}\"></kendo-grid-column> </kendo-grid>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
GridComponent.ctorParameters = () => [
    { type: DataService, },
];
GridComponent.propDecorators = {
    'gridNamespace': [{ type: Input },],
    'gridSelectionType': [{ type: Input },],
    'gridParams': [{ type: Input },],
};
function GridComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    GridComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    GridComponent.ctorParameters;
    /** @type {?} */
    GridComponent.propDecorators;
    /** @type {?} */
    GridComponent.prototype.gridNamespace;
    /** @type {?} */
    GridComponent.prototype.gridSelectionType;
    /** @type {?} */
    GridComponent.prototype.gridParams;
    /** @type {?} */
    GridComponent.prototype.dataSubscription;
    /** @type {?} */
    GridComponent.prototype.gridColumns;
    /** @type {?} */
    GridComponent.prototype.gridData;
    /** @type {?} */
    GridComponent.prototype.dataService;
}
