import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

import { DynamicCmpComponent, HttpService, ComponentService } from '@taskbuilder/core';

@Component({
  selector: 'tb-standalone',
  templateUrl: './standalone.report.component.html'
})
export class StandaloneReportComponent implements OnInit {
  namespace: string;
  params: any;
  @ViewChild(DynamicCmpComponent) dynamicCmp: DynamicCmpComponent;
  subscriptions = [];

  constructor(
    public activatedRoute: ActivatedRoute,
    public httpService: HttpService,
    public componentService: ComponentService
  ) {

    this.activatedRoute.params.subscribe((params: Params) => {
      this.namespace = params['ns'];
      this.params = JSON.parse(params['params']);
      //la rundocument mi manderà indietro un ws con le istruzioni per creare un componente
      const subs = componentService.componentInfoAdded.subscribe(info => {
        //quando il framework crea il componentinfo, posso passarlo al mio componente ospite e creare il
        //componente dinamico
        this.dynamicCmp.componentInfo = info;
        this.dynamicCmp.createComponent();
        subs.unsubscribe();
      });

      this.componentService.createReportComponent(this.namespace, true, this.params);

    });
  }

  ngOnInit() {
  }

}