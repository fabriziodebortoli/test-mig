
import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';

import { CommandType, baseobj } from './reporting-studio.model';

import { DocumentComponent } from '../shared/document.component';

import { ComponentService } from './../core/component.service';
import { EventDataService } from './../core/eventdata.service';
import { ReportingStudioService } from './reporting-studio.service';

import { CookieService } from 'angular2-cookie/services/cookies.service';



@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss'],
  providers: [ReportingStudioService, EventDataService],
})
export class ReportingStudioComponent extends DocumentComponent implements OnInit, OnDestroy {

  /*if this component is used standalone, the namespace has to be passed from the outside template,
  otherwise it is passed by the ComponentService creation logic*/
  private subMessage: Subscription;
  private message: any = '';
  private running: boolean = false;
  private currCommand: CommandType = CommandType.OK;

  private pageNum: number = 1;
  private currLayout: string;

  public objects: baseobj[] = [];

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

    let message = {
      commandType: CommandType.TEMPLATE,
      message: ''
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  ngOnDestroy() {
    this.subMessage.unsubscribe();
  }

  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);
      switch (msg.commandType) {
        case CommandType.ASK: break;
        case CommandType.DATA: break;
        case CommandType.ERROR: break;
        case CommandType.GUID: break;
        case CommandType.NAMESPACE: break;
        case CommandType.NEXTPAGE: break;
        case CommandType.OK: break;
        case CommandType.PAGE: break;
        case CommandType.PDF: break;
        case CommandType.PREVPAGE: break;
        case CommandType.RUN: break;
        case CommandType.STOP: break;
        case CommandType.TEMPLATE:
          this.message = msg.message; //render layout
          break;

        case CommandType.TEST: break;

      }

      //this.message = msg;//.message;
    } catch (err) {
      this.message = 'Error Occured';
    }
  }

  rsInitStateMachine() {

    let message = {
      commandType: CommandType.NAMESPACE,
      nameSpace: this.args.nameSpace,
      authtoken: this.cookieService.get('authtoken')
    };

    let res = this.rsService.doSendSync(JSON.stringify(message));

  }

  RunReport() {
    this.running = true;
    this.currCommand = CommandType.RUN;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
      Response: ''
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  StopReport() {
    this.running = false;
    this.currCommand = CommandType.STOP;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  NextPage() {
    this.currCommand = CommandType.NEXTPAGE;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  PrevPage() {
    this.currCommand = CommandType.PREVPAGE;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
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