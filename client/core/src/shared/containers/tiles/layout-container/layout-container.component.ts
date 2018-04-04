import { Subscription } from 'rxjs/Subscription';
import { Component, OnInit, Input, QueryList, AfterContentInit, ContentChildren, OnDestroy } from '@angular/core';
import { PanelComponent } from '../../panel/panel.component';
import { untilDestroy } from './../../../commons/untilDestroy';

@Component({
  selector: 'tb-layout-container',
  templateUrl: './layout-container.component.html',
  styleUrls: ['./layout-container.component.scss']
})
export class LayoutContainerComponent implements AfterContentInit, OnDestroy {

  subscriptions: Subscription[] = [];

  @ContentChildren(PanelComponent) panels: QueryList<PanelComponent>

  ngAfterContentInit() {

    this.panels.changes.subscribe(() => {
      this.dispose(); // potrebbero arrivare delle attivazioni successivamente quindi verrebbero replicate le subscription
      this.panels.forEach(panel => {
        this.subscriptions.push(panel.toggle.asObservable().subscribe((r) => {
          this.panels.forEach(p => {
            if (p !== panel)
              p.toggleCollapse(false);
          });
        }));
      });
    });

  }

  dispose() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }

  ngOnDestroy() {
    this.dispose();
  }

}