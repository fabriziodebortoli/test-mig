import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EventDataService {

    public command: EventEmitter<string> = new EventEmitter();
    public change: EventEmitter<string> = new EventEmitter();
    public openDropdown: EventEmitter<any> = new EventEmitter();

    public oldModel: any = {};//model before user changes (I need it for delta construction)
    public model: any = {};//current model
    public activation: any = {};//contains activation data
    constructor() {
        console.log('EventDataService created');
    }

}