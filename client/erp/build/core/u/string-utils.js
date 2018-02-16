/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
var StringUtils = (function () {
    function StringUtils() {
    }
    // object.padding(number, string)
    // Transform the string object to string of the actual width filling by the padding character (by default ' ')
    // Negative value of width means left padding, and positive value means right one
    /**
     * @param {?} val
     * @param {?} n
     * @return {?}
     */
    StringUtils.pad = /**
     * @param {?} val
     * @param {?} n
     * @return {?}
     */
    function (val, n) {
        if (Math.abs(n) <= val.length)
            return val;
        var /** @type {?} */ m = Math.max((Math.abs(n) - val.length) || 0, 0);
        var /** @type {?} */ pad = Array(m + 1).join(String(' ').charAt(0));
        return (n < 0) ? pad + val : val + pad;
    };
    ;
    // Replaces \r\n (win) and \r (osx) with \n (unix)
    /**
     * @param {?} s
     * @return {?}
     */
    StringUtils.toUnixEOL = /**
     * @param {?} s
     * @return {?}
     */
    function (s) {
        s = s.replace(/\r\n/g, '\n').replace(/\r/g, '\n');
        return s;
    };
    return StringUtils;
}());
export { StringUtils };
