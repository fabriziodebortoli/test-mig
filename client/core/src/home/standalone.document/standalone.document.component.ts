import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

import { ComponentService } from './../../core/services/component.service';
import { WebSocketService } from './../../core/services/websocket.service';
import { TaskBuilderService } from './../../core/services/taskbuilder.service';
import { DynamicCmpComponent } from './../../shared/components/dynamic-cmp.component';

@Component({
  selector: 'tb-standalone-document',
  templateUrl: './standalone.document.component.html',
  styleUrls: ['./standalone.document.component.scss']
})
export class StandaloneDocumentComponent implements OnInit {
  namespace: string;
  @ViewChild(DynamicCmpComponent) dynamicCmp: DynamicCmpComponent;
  subscriptions = [];

  constructor(
    public activatedRoute: ActivatedRoute,
    public webSocketService: WebSocketService,
    public taskbuilderService: TaskBuilderService,
    public componentService: ComponentService
  ) {

    this.activatedRoute.params.subscribe((params: Params) => {
      this.namespace = params['ns'];
      //la rundocument mi manderà indietro un ws con le istruzioni per creare un componente
      const subs = componentService.componentInfoAdded.subscribe(info => {
        //quando il framework crea il componentinfo, posso passarlo al mio componente ospite e creare il
        //componente dinamico
        this.dynamicCmp.componentInfo = info;
        this.dynamicCmp.createComponent();
        subs.unsubscribe();
      });

      //eseguo la rundocument
      webSocketService.runDocument(this.namespace);

    });
  }

  ngOnInit() {
    let sub = this.taskbuilderService.openTbConnectionAndShowDiagnostic().subscribe(res => { sub.unsubscribe();});
  }

}
