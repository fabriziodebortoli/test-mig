import { ComponentService } from './../../core/component.service';
import { DynamicCmpComponent } from './../../shared/dynamic-cmp.component';
import { HttpService } from './../../core/http.service';
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
    private httpService: HttpService,
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
      httpService.runDocument(this.namespace);

    });
  }

  ngOnInit() {
  }

}
