import { Injectable } from '@angular/core';

@Injectable()
export class LoadingService {

    isLoading: boolean = false;
    message: string = "";

    constructor() {

    }

    setLoading(isLoading: boolean,  message: string = "") {
        this.isLoading = isLoading;
        this.message = message;
    }

}
