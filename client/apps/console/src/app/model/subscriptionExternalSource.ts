export class SubscriptionExternalSource {

  InstanceKey: string;
  SubscriptionKey: string;
  Source: string;
  Description: string = '';
  Provider: string;
  Server: string;
  DBName: string;
  UserId: string;
  Password: string;
  Disabled: boolean = false;
  UnderMaintenance: boolean = false;
  AdditionalInfo: string;

  constructor() {
  }
}