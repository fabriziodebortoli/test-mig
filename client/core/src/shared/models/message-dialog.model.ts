export class MessageDlgArgs {
    public cmpId = '';
    public text = '';
    public ok = false;
    public cancel = false;
    public yes = false;
    public no = false;
    public abort = false;
    public ignore = false;
    public retry = false;
    public continue = false;
}

export class MessageDlgResult {

}
export class DiagnosticData {
    public cmpId = '';
    public messages = new Array<Message>();

}
export class Message {
    type: DiagnosticType;
    text: string;
}
export class DiagnosticDlgResult {

}
export enum DiagnosticType { FatalError = 16, Error = 2, Warning = 1, Info = 8, Banner = 32 }