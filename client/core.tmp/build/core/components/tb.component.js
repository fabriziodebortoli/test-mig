import { Input } from '@angular/core';
/**
 * @abstract
 */
export class TbComponent {
    constructor() {
        this.cmpId = '';
    }
}
TbComponent.propDecorators = {
    'cmpId': [{ type: Input },],
};
function TbComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    TbComponent.propDecorators;
    /** @type {?} */
    TbComponent.prototype.cmpId;
}
