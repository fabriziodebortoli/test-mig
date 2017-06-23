import { ComponentService } from '@taskbuilder/core';
import { HttpService } from '@taskbuilder/core';
import { ActivatedRoute, Params } from '@angular/router';
import { DynamicCmpComponent } from './../../shared/dynamic-cmp.component';
import { Component, OnInit, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-standalone',
  templateUrl: './standalone.report.component.html',
  styleUrls: ['./standalone.report.component.scss']
})


export class StandaloneReportComponent implements OnInit {
  namespace: string;
  params:any;
  @ViewChild(DynamicCmpComponent) dynamicCmp: DynamicCmpComponent;
  subscriptions = [];

  constructor(
    private activatedRoute: ActivatedRoute,
    private httpService: HttpService,
    private componentService: ComponentService) {
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