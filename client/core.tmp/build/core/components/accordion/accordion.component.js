import { Component, Input } from '@angular/core';
export class AccordionComponent {
    constructor() {
        this.groups = [];
    }
    /**
     * @param {?} group
     * @return {?}
     */
    addGroup(group) {
        this.groups.push(group);
    }
    /**
     * @param {?} openGroup
     * @return {?}
     */
    closeOthers(openGroup) {
        this.groups.forEach((group) => {
            if (group !== openGroup) {
                group.isOpen = false;
            }
        });
    }
    /**
     * @param {?} group
     * @return {?}
     */
    removeGroup(group) {
        const /** @type {?} */ index = this.groups.indexOf(group);
        if (index !== -1) {
            this.groups.splice(index, 1);
        }
    }
}
AccordionComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-accordion',
                template: `<ng-content></ng-content>`,
                host: { 'class': 'panel-group' }
            },] },
];
/**
 * @nocollapse
 */
AccordionComponent.ctorParameters = () => [];
function AccordionComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    AccordionComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    AccordionComponent.ctorParameters;
    /** @type {?} */
    AccordionComponent.prototype.groups;
}
export class AccordionGroupComponent {
    /**
     * @param {?} accordion
     */
    constructor(accordion) {
        this.accordion = accordion;
        this._isOpen = false;
        this.accordion.addGroup(this);
    }
    /**
     * @param {?} value
     * @return {?}
     */
    set isOpen(value) {
        this._isOpen = value;
        if (value) {
            this.accordion.closeOthers(this);
        }
    }
    /**
     * @return {?}
     */
    get isOpen() {
        return this._isOpen;
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.accordion.removeGroup(this);
    }
    /**
     * @param {?} event
     * @return {?}
     */
    toggleOpen(event) {
        event.preventDefault();
        this.isOpen = !this.isOpen;
    }
}
AccordionGroupComponent.decorators = [
    { type: Component, args: [{
                selector: 'accordion-group',
                template: `
                <div class="panel panel-default" [ngClass]="{'panel-open': isOpen}">
                  <div class="panel-heading" (click)="toggleOpen($event)">
                    <h4 class="panel-title">
                      <a href tabindex="0"><span>{{heading}}</span></a>
                    </h4>
                  </div>
                  <div class="panel-collapse" [hidden]="!isOpen">
                    <div class="panel-body">
                        <ng-content></ng-content>
                    </div>
                  </div>
                </div>
          `,
            },] },
];
/**
 * @nocollapse
 */
AccordionGroupComponent.ctorParameters = () => [
    { type: AccordionComponent, },
];
AccordionGroupComponent.propDecorators = {
    'heading': [{ type: Input },],
    'isOpen': [{ type: Input },],
};
function AccordionGroupComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    AccordionGroupComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    AccordionGroupComponent.ctorParameters;
    /** @type {?} */
    AccordionGroupComponent.propDecorators;
    /** @type {?} */
    AccordionGroupComponent.prototype._isOpen;
    /** @type {?} */
    AccordionGroupComponent.prototype.heading;
    /** @type {?} */
    AccordionGroupComponent.prototype.accordion;
}
