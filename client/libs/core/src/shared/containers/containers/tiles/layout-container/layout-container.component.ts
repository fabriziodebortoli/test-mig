import { Component, OnInit, Input, QueryList, AfterContentInit, ContentChildren, OnDestroy } from '@angular/core';
import { PanelComponent } from './../../panel/panel.component';
import { untilDestroy } from './../../../commons/untilDestroy';
import { Subscription } from 'rxjs/Subscription';

@Component({
  selector: 'tb-layout-container',
  templateUrl: './layout-container.component.html',
  styleUrls: ['./layout-container.component.scss']
})
export class LayoutContainerComponent implements AfterContentInit, OnDestroy {

  subscriptions: Subscription[] = [];

  @ContentChildren(PanelComponent) panels: QueryList<PanelComponent>

  ngAfterContentInit() {
    this.toggleSubscribe();

    this.panels.changes.subscribe(() => {
      this.dispose(); // potrebbero arrivare delle attivazioni successivamente quindi verrebbero replicate le subscription
      this.toggleSubscribe();
    });
  }

  toggleSubscribe() {
    this.panels.forEach(panel => {

      this.subscriptions.push(panel.toggle.asObservable().subscribe(collapse => {
        this.panels.forEach(p => {
          if (collapse)
            p.collapse();
          else
            p.expand();
        });
      }));

    });
  }

  dispose() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }

  ngOnDestroy() {
    this.dispose();
  }

}