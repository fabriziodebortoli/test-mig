import { Component } from '@angular/core';
export class MessageDialogComponent {
    constructor() {
        this.opened = false;
    }
    /**
     * @return {?}
     */
    ngOnInit() { }
    /**
     * @param {?} args
     * @param {?=} eventData
     * @return {?}
     */
    open(args, eventData) {
        this.eventData = eventData;
        this.args = args;
        this.opened = true;
    }
    /**
     * @param {?} result
     * @return {?}
     */
    close(result) {
        const /** @type {?} */ res = new MessageDlgResult();
        res[result] = true;
        this.opened = false;
        if (this.eventData) {
            this.eventData.closeMessageDialog.emit(res);
        }
    }
}
MessageDialogComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-message-dialog',
                template: "<kendo-dialog title=\"Please confirm\" *ngIf=\"opened\" (close)=\"close('cancel')\"> <p style=\"margin: 30px; text-align: center;\">{{args.text}}</p> <kendo-dialog-actions> <button *ngIf=\"args.no\" kendoButton (click)=\"close('no')\">No</button> <button *ngIf=\"args.yes\" kendoButton (click)=\"close('yes')\" primary=\"true\">Yes</button> <button *ngIf=\"args.cancel\" kendoButton (click)=\"close('cancel')\">Cancel</button> <button *ngIf=\"args.ok\" kendoButton (click)=\"close('ok')\"primary=\"true\">Ok</button> <button *ngIf=\"args.retry\" kendoButton (click)=\"close('retry')\">Retry</button> <button *ngIf=\"args.continue\" kendoButton (click)=\"close('continue')\">Continue</button> <button *ngIf=\"args.abort\" kendoButton (click)=\"close('abort')\">Abort</button> <button *ngIf=\"args.ignore\" kendoButton (click)=\"close('ignore')\">Ignore</button> </kendo-dialog-actions> </kendo-dialog>",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
MessageDialogComponent.ctorParameters = () => [];
function MessageDialogComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    MessageDialogComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    MessageDialogComponent.ctorParameters;
    /** @type {?} */
    MessageDialogComponent.prototype.opened;
    /** @type {?} */
    MessageDialogComponent.prototype.args;
    /** @type {?} */
    MessageDialogComponent.prototype.eventData;
}
export class MessageDlgArgs {
    constructor() {
        this.cmpId = '';
        this.text = '';
        this.ok = false;
        this.cancel = false;
        this.yes = false;
        this.no = false;
        this.abort = false;
        this.ignore = false;
        this.retry = false;
        this.continue = false;
    }
}
function MessageDlgArgs_tsickle_Closure_declarations() {
    /** @type {?} */
    MessageDlgArgs.prototype.cmpId;
    /** @type {?} */
    MessageDlgArgs.prototype.text;
    /** @type {?} */
    MessageDlgArgs.prototype.ok;
    /** @type {?} */
    MessageDlgArgs.prototype.cancel;
    /** @type {?} */
    MessageDlgArgs.prototype.yes;
    /** @type {?} */
    MessageDlgArgs.prototype.no;
    /** @type {?} */
    MessageDlgArgs.prototype.abort;
    /** @type {?} */
    MessageDlgArgs.prototype.ignore;
    /** @type {?} */
    MessageDlgArgs.prototype.retry;
    /** @type {?} */
    MessageDlgArgs.prototype.continue;
}
export class MessageDlgResult {
}
