enum TokenType {
    Undefined,
    API,
    Authentication
}

export class TokenInfo {
    token: string;
    expirationDate: string;
    tokenType: TokenType;

    constructor(token: string, expirationDate: string, tokenType: TokenType) {
        this.token = token;
        this.expirationDate = expirationDate;
        this.tokenType = tokenType;
    }
}