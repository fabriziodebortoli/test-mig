import { Directive, Input, ElementRef } from '@angular/core';

@Directive({
  selector: '[tb-name]'
})
export class NameDirective  {
  constructor(private el: ElementRef) {
  }
  @Input('tb-name')
  set myName(val: string) {
    this.el.nativeElement.name = val;
  }
}



@Directive({
  selector: '[tb-value]'
})
export class ValueDirective  {
  constructor(private el: ElementRef) {
  }
  @Input('tb-value')
  set myValue(val: string) {
    this.el.nativeElement.name = val;
  }
}

