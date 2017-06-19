export class ContextMenuItem {
    /**
     * @param {?} text
     * @param {?} id
     * @param {?} enabled
     * @param {?} checked
     * @param {?=} subItems
     */
    constructor(text, id, enabled, checked, subItems = null) {
        this.text = text;
        this.id = id;
        this.enabled = enabled;
        this.checked = checked;
        this.subItems = subItems;
        this.showMySub = false;
    }
}
function ContextMenuItem_tsickle_Closure_declarations() {
    /** @type {?} */
    ContextMenuItem.prototype.text;
    /** @type {?} */
    ContextMenuItem.prototype.id;
    /** @type {?} */
    ContextMenuItem.prototype.enabled;
    /** @type {?} */
    ContextMenuItem.prototype.checked;
    /** @type {?} */
    ContextMenuItem.prototype.dataBinding;
    /** @type {?} */
    ContextMenuItem.prototype.subItems;
    /** @type {?} */
    ContextMenuItem.prototype.showMySub;
}
