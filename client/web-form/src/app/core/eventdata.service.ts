import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventDataService {

    public command: EventEmitter<string> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
   
    public model: any = {};

    constructor() { 
        console.log('EventDataService created');
    }

}