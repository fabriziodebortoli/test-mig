import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({ selector: '[tdsNormal]' })
export class TdsNormalDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-normal', true);
    }
}

@Directive({ selector: '[tdsFilter]' })
export class TdsFilterDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-filter', true);
    }
}

@Directive({ selector: '[tdsHeader]' })
export class TdsHeaderDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-header', true);
    }
}

@Directive({ selector: '[tdsFooter]' })
export class TdsFooterDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-footer', true);
    }
}

@Directive({ selector: '[tdsWizard]' })
export class TdsWizardDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-wizard', true);
    }
}

@Directive({ selector: '[tdsParameters]' })
export class TdsParametersDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-parameters', true);
    }
}

@Directive({ selector: '[tdsBatch]' })
export class TdsBatchDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tds-batch', true);
    }
}