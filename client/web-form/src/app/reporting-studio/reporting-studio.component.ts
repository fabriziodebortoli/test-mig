import { EventDataService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { ReportingStudioService } from './reporting-studio.service';
import { ReportingStudioConnection } from './reporting-studio-connection.component';

import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss'],
  providers: [ReportingStudioService, EventDataService],
})
export class ReportingStudioComponent extends DocumentComponent implements OnInit, OnDestroy {

  /*if this component is used standalone, the namespace has to be passed from the outside template,
  otherwise it is passed by the ComponentService creation logic*/
  @Input()
  public nameSpace: string;

  sub: Subscription;
  private rsConn: ReportingStudioConnection;
  private message: string = '';

  constructor(private rsService: ReportingStudioService, eventData: EventDataService) {
    super(rsService, eventData);
  }

  ngOnInit() {
    super.ngOnInit();
    this.nameSpace = this.args.nameSpace;
    this.rsConn = new ReportingStudioConnection();
    this.eventData.model = { 'Title': { 'value': this.nameSpace } };
    this.sub = this.rsConn.message.subscribe(received => {
      this.onMessage(received);

    });

    this.rsConn.rsInitStateMachine(this.nameSpace);
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  onMessage(message: string) {
    //elaborate
    this.message = message;

  }

}
