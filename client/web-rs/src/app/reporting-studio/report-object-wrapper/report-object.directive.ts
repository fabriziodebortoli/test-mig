import { Directive, OnInit, Input, ElementRef, Renderer } from '@angular/core';
import { ReportObject } from '../report.model';

@Directive({
  selector: '[rsObject]'
})
export class ReportObjectDirective implements OnInit {

  @Input() ro: ReportObject;

  constructor(private el: ElementRef, private renderer: Renderer) { }

  ngOnInit() {
    // console.log('DIRECTIVE', this.ro);
    this.style();
  }

  style() {
    this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.backgroundColor);

    this.renderer.setElementStyle(this.el.nativeElement, 'border', this.ro.borderColor + ' solid ' + this.ro.borderSize + 'px');

    this.renderer.setElementStyle(this.el.nativeElement, 'left', this.ro.posXpx + 'px');
    this.renderer.setElementStyle(this.el.nativeElement, 'top', this.ro.posYpx + 'px');

    this.renderer.setElementStyle(this.el.nativeElement, 'left', this.ro.posXmm + 'mm');
    this.renderer.setElementStyle(this.el.nativeElement, 'top', this.ro.posYmm + 'mm');

    this.renderer.setElementAttribute(this.el.nativeElement, 'title', this.ro.tooltip);

    if (this.ro.hidden) {
      this.renderer.setElementStyle(this.el.nativeElement, 'display', 'none');
    }

    this.renderer.setElementStyle(this.el.nativeElement, 'boxShadow', this.ro.shadowSize + 'px ' + this.ro.shadowSize + 'px ' + this.ro.shadowSize + 'px ' + this.ro.shadowColor);
  }

}
