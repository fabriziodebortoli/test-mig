/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
var /** @type {?} */ admittedSpecialKeys = ['Backspace', 'Tab', 'End', 'Home', 'ArrowLeft', 'ArrowRight', 'Escape'];
/**
 * @param {?} s
 * @return {?}
 */
function _isNumber(s) {
    var /** @type {?} */ regex = new RegExp(/^-?\d+$/);
    if (!String(s).match(regex))
        return false;
    return true;
}
/**
 * @param {?} s
 * @return {?}
 */
function _isLetter(s) {
    var /** @type {?} */ regex = new RegExp(/[a-zA-Z]/);
    if (!String(s).match(regex))
        return false;
    return true;
}
/**
 * @param {?} s
 * @return {?}
 */
function _isNotAlphanumeric(s) {
    var /** @type {?} */ regex = new RegExp(/[^a-zA-Z\d]/);
    if (!String(s).match(regex))
        return false;
    return true;
}
var KeyboardEventHelper = (function () {
    function KeyboardEventHelper() {
    }
    /**
     * @param {?} e
     * @return {?}
     */
    KeyboardEventHelper.isSpecial = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (admittedSpecialKeys.indexOf(e.key) !== -1)
            return true;
        if (e.ctrlKey === true && (e.key === 'c' || e.key === 'x' || e.key === 'v' || e.key === 'z'))
            return true;
        return false;
    };
    /**
     * @param {?} e
     * @return {?}
     */
    KeyboardEventHelper.isNumber = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (this.isSpecial(e))
            return true;
        var /** @type {?} */ current = e.key;
        var /** @type {?} */ next = current.concat(e.key);
        if (next && !_isNumber(next))
            return false;
        return true;
    };
    /**
     * @param {?} e
     * @return {?}
     */
    KeyboardEventHelper.isLetter = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (this.isSpecial(e))
            return true;
        var /** @type {?} */ current = e.key;
        var /** @type {?} */ next = current.concat(e.key);
        if (next && !_isLetter(next))
            return false;
        return true;
    };
    /**
     * @param {?} e
     * @return {?}
     */
    KeyboardEventHelper.isNotAlphanumeric = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        if (this.isSpecial(e))
            return true;
        var /** @type {?} */ current = e.key;
        var /** @type {?} */ next = current.concat(e.key);
        if (next && !_isNotAlphanumeric(next))
            return false;
        return true;
    };
    return KeyboardEventHelper;
}());
export { KeyboardEventHelper };
var ClipboardEventHelper = (function () {
    function ClipboardEventHelper() {
    }
    /**
     * @param {?} e
     * @return {?}
     */
    ClipboardEventHelper.isNumber = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        var /** @type {?} */ pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isNumber(pasteData)) {
            return false;
        }
    };
    /**
     * @param {?} e
     * @return {?}
     */
    ClipboardEventHelper.isLetter = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        var /** @type {?} */ pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isLetter(pasteData)) {
            return false;
        }
    };
    /**
     * @param {?} e
     * @return {?}
     */
    ClipboardEventHelper.isNotAlphanumeric = /**
     * @param {?} e
     * @return {?}
     */
    function (e) {
        var /** @type {?} */ pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isNotAlphanumeric(pasteData)) {
            return false;
        }
    };
    return ClipboardEventHelper;
}());
export { ClipboardEventHelper };
