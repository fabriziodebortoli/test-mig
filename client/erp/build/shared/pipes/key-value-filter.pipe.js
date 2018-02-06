/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import { Pipe } from '@angular/core';
var KeyValueFilterPipe = (function () {
    function KeyValueFilterPipe() {
    }
    /**
     * @param {?} value
     * @param {?} queryString
     * @return {?}
     */
    KeyValueFilterPipe.prototype.transform = /**
     * @param {?} value
     * @param {?} queryString
     * @return {?}
     */
    function (value, queryString) {
        if (value === null) {
            return null;
        }
        if (queryString !== undefined) {
            return value.filter(function (item) {
                return (item.key.toLowerCase().indexOf(queryString.toLowerCase()) !== -1) ||
                    (item.value.toLowerCase().indexOf(queryString.toLowerCase()) !== -1);
            });
        }
        else {
            return value;
        }
    };
    KeyValueFilterPipe.decorators = [
        { type: Pipe, args: [{
                    name: 'keyValueFilter'
                },] },
    ];
    /** @nocollapse */
    KeyValueFilterPipe.ctorParameters = function () { return []; };
    return KeyValueFilterPipe;
}());
export { KeyValueFilterPipe };
function KeyValueFilterPipe_tsickle_Closure_declarations() {
    /** @type {!Array<{type: !Function, args: (undefined|!Array<?>)}>} */
    KeyValueFilterPipe.decorators;
    /**
     * @nocollapse
     * @type {function(): !Array<(null|{type: ?, decorators: (undefined|!Array<{type: !Function, args: (undefined|!Array<?>)}>)})>}
     */
    KeyValueFilterPipe.ctorParameters;
}
