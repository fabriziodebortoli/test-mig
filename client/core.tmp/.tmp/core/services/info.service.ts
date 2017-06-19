import { Injectable } from '@angular/core';

import { HttpService } from './http.service';

@Injectable()
export class InfoService {

    desktop: boolean;

    constructor(httpService: HttpService) { }

}