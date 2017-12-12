export enum UrlType {
    API,
    APP,
    TBLOADER,
    DATABASE
}

export class ServerUrl {

    urlType: UrlType;
    url: string;
    appName: string;

    constructor(urlType: UrlType, url: string, appName: string) {
        this.urlType = urlType;
        this.url = url;
        this.appName = appName;
    }
}