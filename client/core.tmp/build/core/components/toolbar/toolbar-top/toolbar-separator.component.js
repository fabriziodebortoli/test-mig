import { Component } from '@angular/core';
export class ToolbarSeparatorComponent {
    constructor() { }
    /**
     * @return {?}
     */
    ngOnInit() {
    }
}
ToolbarSeparatorComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-toolbar-separator',
                template: `<div></div>`,
                styles: [`
    div{
      width:1px;
      height:30px;
      background:#ddd;
    }
  `]
            },] },
];
/**
 * @nocollapse
 */
ToolbarSeparatorComponent.ctorParameters = () => [];
function ToolbarSeparatorComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ToolbarSeparatorComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ToolbarSeparatorComponent.ctorParameters;
}
