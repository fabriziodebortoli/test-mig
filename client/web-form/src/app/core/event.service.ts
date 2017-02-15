import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventService {

    public command: EventEmitter<string> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
    constructor() {
    }


}