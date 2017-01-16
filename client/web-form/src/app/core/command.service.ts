import { Injectable } from '@angular/core';

@Injectable()
export class CommandService {

    constructor() {
         console.log('CommandService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    
    isServerSideCommand(idCommand: String) {
        //per ora sono considerati tutti server-side,ma in futuro ci sara la mappa dei comandi che vanno eseguito server side
        return true;
    }

}