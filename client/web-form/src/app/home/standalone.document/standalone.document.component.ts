import { WebSocketService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';
import { DynamicCmpComponent } from './../../shared/dynamic-cmp.component';
import { ActivatedRoute, Params } from '@angular/router';
import { Component, OnInit, ViewChild } from '@angular/core';

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
    private activatedRoute: ActivatedRoute,
    private webSocketService: WebSocketService,
    private componentService: ComponentService) {
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
  }

}
