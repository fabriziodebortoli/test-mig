import { Directive, ElementRef, Renderer, AfterViewInit , Input } from '@angular/core';

@Directive({
  selector: '[tbColored]'
})

export class ColoredControlDirective implements AfterViewInit {
  @Input() backgroundColor: string;
  @Input() color: string;

  constructor(private el: ElementRef, private renderer: Renderer) {
  }

  ngAfterViewInit() {
    if (this.backgroundColor)
      this.el.nativeElement.style.backgroundColor = this.backgroundColor;

    if (this.color)
      this.el.nativeElement.style.color = this.color;
  }
}