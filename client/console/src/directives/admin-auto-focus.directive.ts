import { Directive, OnInit, ElementRef } from '@angular/core';

@Directive({
  selector: '[adminAutoFocus]'
})
export class AdminAutoFocusDirective {

  constructor(private elementRef: ElementRef) {}

  ngOnInit(): void {
    this.elementRef.nativeElement.focus();
  }

  ngAfterContentChecked(): void {
    this.elementRef.nativeElement.focus();
  }

}
