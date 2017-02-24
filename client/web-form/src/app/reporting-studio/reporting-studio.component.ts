
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { CommandType } from './reporting-studio.model';
import { EventDataService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { ReportingStudioService } from './reporting-studio.service';
import { ReportingStudioConnectionComponent } from './reporting-studio-connection.component';

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


  private subMessage: Subscription;
  private rsConn: ReportingStudioConnectionComponent;
  private message: string = '';

  constructor(private rsService: ReportingStudioService, eventData: EventDataService, private cookieService: CookieService) {
    super(rsService, eventData);
  }

  ngOnInit() {
    super.ngOnInit();
    this.rsConn = new ReportingStudioConnectionComponent();
    this.eventData.model = { 'Title': { 'value': this.args.nameSpace } };

    this.subMessage = this.rsConn.message.subscribe(received => {
      this.onMessage(received);

    });


    this.rsInitStateMachine();

    this.eventData.opened.emit('');

  }


  ngOnDestroy() {
    this.subMessage.unsubscribe();
  }

  onMessage(message: string) {
    //elaborate
    this.message = message;
  }

  rsInitStateMachine() {

    let message = {
        commandType: CommandType.NAMESPACE.toString(),
        nameSpace: this.args.nameSpace,
        authtoken: this.cookieService.get('authtoken')
      };

    this.rsConn.doSend(JSON.stringify(message));

  }

}
