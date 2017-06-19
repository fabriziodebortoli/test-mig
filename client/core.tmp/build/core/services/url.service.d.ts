export declare class UrlService {
    private host;
    private port;
    private secure;
    constructor();
    getBackendUrl(): string;
    getApiUrl(): string;
    getWsUrl(): string;
    setPort(port: number): void;
    setHost(host: string): void;
    setSecure(secure: boolean): void;
}
