import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

@Injectable()
export class InfoService {
    desktop: boolean;
    constructor(httpService: HttpService) {
    }



}