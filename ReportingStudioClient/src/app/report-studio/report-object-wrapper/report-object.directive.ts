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
    // if (this.ro.bgColor) {
    this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.bgColor);
    this.renderer.setElementStyle(this.el.nativeElement, 'left', this.ro.posX + 'px');
    this.renderer.setElementStyle(this.el.nativeElement, 'top', this.ro.posY + 'px');
    // this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.bgColor);
    // this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.bgColor);
    // this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.bgColor);
    // this.renderer.setElementStyle(this.el.nativeElement, 'backgroundColor', this.ro.bgColor);
    // }
  }

}
