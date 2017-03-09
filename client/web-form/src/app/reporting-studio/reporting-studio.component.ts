import { Response } from '@angular/http';


import { Component, OnInit, OnDestroy, Input, ComponentFactoryResolver } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';

import { CommandType } from './reporting-studio.model';

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

  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);

      if (msg.commandType !== this.currCommand) {
        this.message = 'The command was ' + this.currCommand + ' but the response recieved is for ' + msg.commandType;
      }else {
        this.message = msg.message;
      }
    } catch (err) {
      this.message = 'Error Occured';
    }
  }

  rsInitStateMachine() {

    let message = {
      commandType: CommandType.NAMESPACE.toString(),
      nameSpace: this.args.nameSpace,
      authtoken: this.cookieService.get('authtoken')
    };

    this.rsService.doSend(JSON.stringify(message));

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

  PauseReport() {
    this.running = false;
    this.currCommand = CommandType.PAUSE;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace
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