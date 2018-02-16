import { Component, OnInit, Input, QueryList, AfterViewInit, ContentChildren, OnDestroy } from '@angular/core';
import { PanelComponent } from '../../panel/panel.component';
import { untilDestroy } from './../../../commons/untilDestroy';

@Component({
  selector: 'tb-layout-container',
  templateUrl: './layout-container.component.html',
  styleUrls: ['./layout-container.component.scss']
})
export class LayoutContainerComponent implements AfterViewInit, OnDestroy {

  @ContentChildren(PanelComponent) panels: QueryList<PanelComponent>

  ngAfterViewInit() {

    this.panels.forEach(panel => {
      panel.toggle.asObservable().pipe(untilDestroy(this)).subscribe((r) => {
        this.panels.forEach(p => {
          if (p !== panel)
            p.toggleCollapse(false);
        });
      });
    });

  }

  ngOnDestroy() { }

}