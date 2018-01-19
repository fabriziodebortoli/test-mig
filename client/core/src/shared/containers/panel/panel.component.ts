import { Component, OnInit, Input, Output, ViewEncapsulation, EventEmitter } from '@angular/core';

@Component({
  selector: 'tb-panel',
  templateUrl: './panel.component.html',
  styleUrls: ['./panel.component.scss']
})
export class PanelComponent implements OnInit {
  
  ngOnInit() {  }
  
  @Input() title: string;
  @Input() isCollapsed: boolean = false;
  @Input() isCollapsible: boolean = true;

  @Output() toggle = new EventEmitter<boolean>();
  
  toggleCollapse(emit:boolean = true): void {
    
    if (!this.isCollapsible)
      return;
    
      this.isCollapsed = !this.isCollapsed;
    
    if(emit)
      this.toggle.emit(this.isCollapsed);
      
  }
}
