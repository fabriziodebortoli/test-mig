export type HotLinkInfo = { namespace?: string,
    name: string,
    selector?: string,
    items?: [{name:string, namespace:string}],
    autoFind?:boolean,
    enableAddOnFly?: boolean,
    mustExistData?: boolean,
    ctx?: any };