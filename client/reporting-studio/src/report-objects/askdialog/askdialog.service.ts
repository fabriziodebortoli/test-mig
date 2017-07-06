import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class AskdialogService {
   askChanged = new EventEmitter();

}