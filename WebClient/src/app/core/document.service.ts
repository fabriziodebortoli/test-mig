import { WebSocketService } from './websocket.service';
 import { Injectable } from '@angular/core';
 @Injectable()
 export class DocumentService {
    data: any;

     constructor(private webSocketService: WebSocketService) {
         this.webSocketService.dataReady.subscribe(data => {
            this.data = data;
         });
     }
 }
