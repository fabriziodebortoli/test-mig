import { Component, Input, TemplateRef, ContentChild } from '@angular/core';

@Component({
  selector: 'tb-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class TbHeaderComponent {
    
    @ContentChild(TemplateRef) templateRef: any;
    
    @Input() active: boolean;
    
    constructor() { }
}
