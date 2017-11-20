import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class DataChannelService {

  dataChannel: BehaviorSubject<any>;

  constructor() { 

  }

  sendMessage() {
    this.dataChannel.next("");
  }

}
