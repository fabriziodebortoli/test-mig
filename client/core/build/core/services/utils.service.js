import { Injectable } from '@angular/core';
import { Logger } from './logger.service';
export class UtilsService {
    /**
     * @param {?} logger
     */
    constructor(logger) {
        this.logger = logger;
        //---------------------------------------------------------------------------------------------
        this.getCurrentDate = function () {
            const /** @type {?} */ d = new Date();
            const /** @type {?} */ p = parseInt(d.getFullYear() +
                ('00' + (d.getMonth() + 1)).slice(-2) +
                ('00' + d.getDate()).slice(-2) +
                ('00' + d.getHours()).slice(-2) +
                ('00' + d.getMinutes()).slice(-2) +
                ('00' + d.getSeconds()).slice(-2), 10);
            return p;
        };
        this.logger.debug('UtilsService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @param {?} data
     * @return {?}
     */
    serializeData(data) {
        let /** @type {?} */ buffer = [];
        // Serialize each key in the object.
        for (let /** @type {?} */ name in data) {
            if (!data.hasOwnProperty(name)) {
                continue;
            }
            let /** @type {?} */ value = data[name];
            buffer.push(encodeURIComponent(name) + '=' + encodeURIComponent((value == null) ? '' : value));
        }
        // Serialize the buffer and clean it up for transportation.
        let /** @type {?} */ source = buffer.join('&').replace(/%20/g, '+');
        return (source);
    }
    ;
    /**
     * @return {?}
     */
    generateGUID() {
        /**
         * @return {?}
         */
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    }
    ;
    /**
     * @param {?} items
     * @return {?}
     */
    toArray(items) {
        let /** @type {?} */ filtered = [];
        if (items === undefined) {
            return filtered;
        }
        if (Object.prototype.toString.call(items) === '[object Array]') {
            return items;
        }
        else {
            filtered.push(items);
            return filtered;
        }
    }
    ;
    /**
     * @param {?} str
     * @return {?}
     */
    parseBool(str) {
        if (typeof str === 'boolean')
            return str;
        if (typeof str === 'string' && str.toLowerCase() == 'true')
            return true;
        return (parseInt(str) > 0);
    }
    /**
     * @return {?}
     */
    getApplicationFromQueryString() {
        var /** @type {?} */ application = '';
        var /** @type {?} */ pageUrl = window.location.search;
        var /** @type {?} */ index = pageUrl.indexOf("?app=");
        if (index < 0)
            return application;
        application = pageUrl.substring(index + "?app=".length);
        index = application.indexOf("&");
        if (index < 0)
            return application;
        return application.substring(0, index);
    }
    /**
     * @param {?} hex
     * @return {?}
     */
    hexToRgba(hex) {
        let /** @type {?} */ result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16),
            a: 1
        } : null;
    }
}
UtilsService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
UtilsService.ctorParameters = () => [
    { type: Logger, },
];
function UtilsService_tsickle_Closure_declarations() {
    /** @type {?} */
    UtilsService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    UtilsService.ctorParameters;
    /** @type {?} */
    UtilsService.prototype.getCurrentDate;
    /** @type {?} */
    UtilsService.prototype.logger;
}
