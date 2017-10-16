import { Injectable } from '@angular/core';

@Injectable()
export class DiagnosticService {

    messages = [];
isVisible: boolean = false;
    constructor() {

    }

    showDiagnostic(messages){
        this.messages = messages;
        this.isVisible = true;
    }
    
    resetDiagnostic(){
        this.messages = [];
        this.isVisible = false;
    }
}
