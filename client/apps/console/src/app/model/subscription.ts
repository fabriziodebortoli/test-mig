export class Subscription {
    
    subscriptionKey: string;
    description: string;
    activationToken: string;
    language: string;
    regionalSettings: string;
    minDBSizeToWarn: number;
    underMaintenance: boolean;

    constructor() {
    }
}