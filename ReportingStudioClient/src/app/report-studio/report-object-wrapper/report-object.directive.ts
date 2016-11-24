import { Directive, OnInit, Input, ElementRef, Renderer } from '@angular/core';
import { ReportObject } from './report-object.model';

@Directive({
  selector: '[rsReportObject]'
})
export class ReportObjectDirective implements OnInit {

  @Input() ro: ReportObject;

  constructor(private el: ElementRef, private renderer: Renderer) { }

  ngOnInit() {
    console.log('DIRECTIVE', this.ro);
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
    // -webkit-box-shadow: 10px 10px 10px 0px rgba(0,0,0,0.75);
    // -moz-box-shadow: 10px 10px 10px 0px rgba(0,0,0,0.75);
    // box-shadow: 10px 10px 10px 0px rgba(0,0,0,0.75);


  }

}
