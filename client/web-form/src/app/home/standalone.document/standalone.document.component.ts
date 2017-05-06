import { WebSocketService } from './../../core/websocket.service';
import { HttpService } from './../../core/http.service';
import { ActivatedRoute, Params } from '@angular/router';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-standalone-document',
  templateUrl: './standalone.document.component.html'
})
export class StandaloneDocumentComponent implements OnInit {
  namespace:string;
  constructor(
    private activatedRoute: ActivatedRoute,
    private httpService: HttpService,
    private webSocketService: WebSocketService) {
    this.activatedRoute.params.subscribe((params: Params) => {
      const namespace = params['ns'];
      httpService.runDocument(namespace);
      this.namespace = namespace;
    });
  }

  ngOnInit() {
  }

}
