import { CookieService } from 'angular2-cookie/services/cookies.service';

import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscription } from 'rxjs';

import { CommandType, baseobj, fieldrect, textrect } from './reporting-studio.model';

import { DocumentComponent } from '../shared/document.component';

import { ComponentService } from './../core/component.service';
import { EventDataService } from './../core/eventdata.service';
import { ReportingStudioService } from './reporting-studio.service';


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

  public objects: baseobj[] = [];
  public templates: any[] = [];

  constructor(private rsService: ReportingStudioService, eventData: EventDataService, private cookieService: CookieService) {
    super(rsService, eventData);
  }

  // -----------------------------------------------
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

  // -----------------------------------------------
  ngOnDestroy() {
    this.subMessage.unsubscribe();
  }

  // -----------------------------------------------
  onMessage(message: any) {
    //elaborate
    try {
      let msg = JSON.parse(message);
      switch (msg.commandType) {
        case CommandType.ASK: break;

        case CommandType.ERROR: break;
        case CommandType.GUID: break;
        case CommandType.NAMESPACE: break;

        case CommandType.OK: break;
        case CommandType.PAGE: break;
        case CommandType.PDF: break;

        case CommandType.PREVPAGE:
          this.RenderLayout(this.templates[this.rsService.pageNum - 1]);
          break;

        case CommandType.NEXTPAGE:
          this.UpdateData(msg.message);
          break;

        case CommandType.RUN:
          this.UpdateData(msg.message);
          break;

        case CommandType.STOP: break;
        case CommandType.TEMPLATE:
          this.RenderLayout(msg.message);
          if (this.rsService.pageNum > this.templates.length) {
            this.templates.push(msg.message);
          }
          break;

        case CommandType.TEST: break;

      }



      //this.message = msg;//.message;
    } catch (err) {
      this.message = 'Error Occured';
    }
  }

  // -----------------------------------------------
  rsInitStateMachine() {

    let message = {
      commandType: CommandType.NAMESPACE,
      nameSpace: this.args.nameSpace,
      authtoken: this.cookieService.get('authtoken')
    };

    this.rsService.doSendSync(JSON.stringify(message));

  }

  // -----------------------------------------------
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

  // -----------------------------------------------
  StopReport() {
    this.running = false;
    this.currCommand = CommandType.STOP;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
    };
    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  NextPage() {
    this.currCommand = CommandType.NEXTPAGE;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  PrevPage() {
    this.currCommand = CommandType.PREVPAGE;
    let message = {
      commandType: this.currCommand,
      message: this.args.nameSpace,
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  // -----------------------------------------------
  RenderLayout(msg: any) {
    this.objects = [];
    let k = JSON.parse(msg);
    if (this.rsService.pageNum != k.page.page_number) { return; }
    this.rsService.currLayout = k.page.layout.name;


    for (let index = 0; index < k.page.layout.objects.length; index++) {
      let element = k.page.layout.objects[index];
      if (element.fieldrect !== undefined) {
        let fr = new fieldrect(element.fieldrect);
        this.objects.push(fr);
      }
      if (element.textrect !== undefined) {
        let tr = new textrect(element.textrect);
        this.objects.push(tr);
      }
    }
  }

  // -----------------------------------------------
  UpdateData(msg: any) {
    let k = JSON.parse(msg);
    if (this.rsService.pageNum != k.page.page_number) { return; }
    let id: number;
    let value: any;
    for (let index = 0; index < k.page.layout.objects.length; index++) {
      let element = k.page.layout.objects[index];
      if (element.fieldrect !== undefined) {
        id = element.fieldrect.baserect.baseobj.id;
        value = element.fieldrect.value ? element.fieldrect.value : '[empty]' + id;
      }
      if (element.textrect !== undefined) {
        id = element.textrect.baserect.baseobj.id;
        value = element.textrect.value ? element.textrect.value : '[empty]' + id;
      }
      // to complete

      let obj = this.FindObj(id);
      if (obj === undefined) {
        continue;
      }
      obj.value = value;
    }
  }

  private FindObj(id: number): any {
    for (let key in this.objects) {
      if (this.objects.hasOwnProperty(key)) {
        let element = this.objects[key];
        if (element.id === id) {
          return element;
        }
      }
    }
    return undefined;
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