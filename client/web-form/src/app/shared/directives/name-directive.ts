import { Directive, Input, ElementRef } from '@angular/core';

@Directive({
  selector: '[tb-name]'
})
export class NameDirective  {
  constructor(private el: ElementRef) {
  }
  @Input('tb-name')
  set myname(val: string) {
    this.el.nativeElement.name = val;
  }
}