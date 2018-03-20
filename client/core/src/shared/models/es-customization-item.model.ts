export class EsCustomizItem {
    
    fileFullPath: string
    customizationName: string
    applicationOwner: string
    moduleOwner: string
    
        public constructor(  
            customizationName: string,
            applicationOwner: string,
            moduleOwner: string,
            fileFullPath: string
        ) {
                this.fileFullPath = fileFullPath,
                this.customizationName = customizationName,
                this.applicationOwner = applicationOwner,
                this.moduleOwner = moduleOwner
        }
    }
    

    export class PairAppMod {
        application: string
        module: string
    
        constructor(app, mod) {
            this.application = app;
            this.module = mod;
        }
    }    