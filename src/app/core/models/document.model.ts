export class Document {
  type: Number;
  ns: string;
  session: string;

  constructor(type: Number, ns: string, session: string) {
    this.type = type;
    this.ns = ns;
    this.session = session;
  }
}
