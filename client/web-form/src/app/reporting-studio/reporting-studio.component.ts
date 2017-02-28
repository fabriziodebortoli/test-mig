
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { CommandType } from './reporting-studio.model';
import { EventDataService, ComponentService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { ReportingStudioService } from './reporting-studio.service';
import { Component, OnInit, OnDestroy, Input, ComponentFactoryResolver } from '@angular/core';
import { Subscription } from 'rxjs';
import { ActivatedRoute, Params } from '@angular/router';


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
  private message: string = '';

  constructor(private rsService: ReportingStudioService, eventData: EventDataService, private cookieService: CookieService) {
    super(rsService, eventData);
  }

  ngOnInit() {
    super.ngOnInit();
    this.eventData.model = { 'Title': { 'value': this.args.nameSpace } };

    this.subMessage = this.rsService.message.subscribe(received => {
      this.onMessage(received);

    });

    this.rsInitStateMachine();
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

    this.rsService.doSend(JSON.stringify(message));

  }

}

@Component({
  template: ''
})
export class ReportingStudioFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver, private activatedRoute: ActivatedRoute) {
    this.activatedRoute.params.subscribe((params: Params) => {
      let ns = params['ns'];
      componentService.createComponent(ReportingStudioComponent, resolver, { 'nameSpace': ns });
    });
  }
}