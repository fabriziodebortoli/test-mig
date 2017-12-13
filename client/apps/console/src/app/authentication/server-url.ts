export enum UrlType {
    API,
    APP,
    TBLOADER,
    DATABASE
}

//================================================================================
export class ServerUrl {

  instanceKey: string;
  urlType: UrlType;
  url: string;

  //--------------------------------------------------------------------------------
  constructor(instanceKey: string, urlType: UrlType, url: string) {
    this.instanceKey = instanceKey;
    this.urlType = urlType;
    this.url = url;
  }
}