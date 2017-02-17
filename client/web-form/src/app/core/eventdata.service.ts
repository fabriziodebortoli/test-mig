import { Injectable, EventEmitter } from '@angular/core';
import { ViewModeType } from 'tb-shared';

@Injectable()
export class EventDataService {

    public command: EventEmitter<string> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();

    public model: any = {};

    constructor() { }

}