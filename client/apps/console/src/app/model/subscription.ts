export class Subscription {
    
    subscriptionKey: string;
    description: string;
    activationToken: string;
    preferredLanguage: string;
    applicationLanguage: string;
    minDBSizeToWarn: number;
    underMaintenance: boolean;

    constructor() {
    }
}