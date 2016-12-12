export class DocumentInfo {
  ns: string;
  cmd: string;
  constructor(ns: string) {
    this.ns = ns;
    this.cmd = 'runDocument';
  }
}
