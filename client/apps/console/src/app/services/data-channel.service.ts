import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class DataChannelService {

  dataChannel: BehaviorSubject<any>;

  constructor() { 
    this.dataChannel = new BehaviorSubject<any>(null);
  }

  sendMessage() {
    this.dataChannel.next("");
  }

}
