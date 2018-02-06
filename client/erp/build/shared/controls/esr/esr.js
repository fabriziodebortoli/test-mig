/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
import * as _ from 'lodash';
var Esr = (function () {
    function Esr() {
    }
    /**
     * @param {?} s
     * @return {?}
     */
    Esr.checkEsrDigit = /**
     * @param {?} s
     * @return {?}
     */
    function (s) {
        var /** @type {?} */ strNewValue = s;
        var /** @type {?} */ valid = { result: true, error: '' };
        if (strNewValue === '')
            return valid;
        var /** @type {?} */ length = strNewValue.length;
        var /** @type {?} */ finalCheckDigit;
        // Se il codice è 27, 16 o < 15 faccio il controllo Modulo10.
        if (length === 27 || length === 16 || length < 15) {
            strNewValue = _.padStart(strNewValue, 27, '0');
            // non prendo l'ultimo, perche' e' il carattere di controllo
            finalCheckDigit = this.calculatePVRCheck(strNewValue.substring(0, 26));
            var /** @type {?} */ expectedValue = strNewValue.substring(strNewValue.length - 1, strNewValue.length);
            if (finalCheckDigit !== expectedValue) {
                return { result: false,
                    error: 'Incorrect ESR Check Digit, value expected ' + finalCheckDigit + ' value found ' + expectedValue + '.' };
            }
        }
        else if (length === 15) { }
        else {
            return { result: false,
                error: 'Wrong length for ESR Reference Number' };
        }
        return valid;
    };
    /**
     * @param {?} number
     * @return {?}
     */
    Esr.calculatePVRCheck = /**
     * @param {?} number
     * @return {?}
     */
    function (number) {
        // CheckDigit Modulo10
        // Creo un array di oggetti CString che a loro volta sono array di Char, quindo ho una matrice a 2 dimensioni...
        var /** @type {?} */ aMatrix = [
            '0946827135 0',
            '9468271350 9',
            '4682713509 8',
            '6827135094 7',
            '8271350946 6',
            '2713509468 5',
            '7135094682 4',
            '1350946827 3',
            '3509468271 2',
            '5094682713 1'
        ];
        var /** @type {?} */ checkCode = '';
        var /** @type {?} */ riporto = 0, /** @type {?} */ digit = 0;
        for (var /** @type {?} */ i = 0; i < number.length; i++) {
            digit = Number(number[i]);
            riporto = Number(aMatrix[riporto][digit]);
        }
        checkCode = aMatrix[riporto][11];
        return checkCode;
    };
    return Esr;
}());
export default Esr;
