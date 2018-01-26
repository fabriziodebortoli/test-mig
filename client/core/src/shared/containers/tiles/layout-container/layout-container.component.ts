import { Component, OnInit, Input, QueryList, AfterViewInit, ContentChildren } from '@angular/core';
import { PanelComponent } from '../../panel/panel.component';

@Component({
  selector: 'tb-layout-container',
  templateUrl: './layout-container.component.html',
  styleUrls: ['./layout-container.component.scss']
})
export class LayoutContainerComponent implements AfterViewInit{

  @ContentChildren(PanelComponent) panels: QueryList<PanelComponent>

  ngAfterViewInit() {
    
    this.panels.forEach(panel => {      
      panel.toggle.subscribe((r) => {         
        this.panels.forEach(p => {
          if (p !== panel)
            p.toggleCollapse(false);
        });
      });
    });

  }

}