export class EsCustomizItem {
    
    fileName: string
    customizationName: string
    applicationOwner: string
    moduleOwner: string
    
        public constructor(  fileName: string,
            customizationName: string,
            applicationOwner: string,
            moduleOwner: string) {
                this.fileName = fileName,
                this.customizationName = customizationName,
                this.applicationOwner = applicationOwner,
                this.moduleOwner = moduleOwner
        }
    }
    