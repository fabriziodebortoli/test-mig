import { BodyEditService } from './../../core/services/body-edit.service';
import { Directive, ElementRef, Renderer2, Input, OnInit, OnDestroy } from '@angular/core';

import { Subscription } from '../../rxjs.imports';

import { LayoutService } from './../../core/services/layout.service';

@Directive({ selector: '[tbInsideBodyEdit]' })
export class TbInsideBodyEditDirective {

    //    @Input() marginLeft: number;
    constructor(
        private el: ElementRef,
        private renderer: Renderer2,
        public bodyEditService: BodyEditService
    ) {
    }
}