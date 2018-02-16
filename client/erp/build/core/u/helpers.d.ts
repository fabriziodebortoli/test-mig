export declare class KeyboardEventHelper {
    private static isSpecial(e);
    static isNumber(e: {
        key: string;
        ctrlKey: boolean;
    }): boolean;
    static isLetter(e: {
        key: string;
        ctrlKey: boolean;
    }): boolean;
    static isNotAlphanumeric(e: {
        key: string;
        ctrlKey: boolean;
    }): boolean;
}
export declare class ClipboardEventHelper {
    static isNumber(e: {
        type: string;
        clipboardData: {
            getData: (string) => string;
        };
    }): boolean;
    static isLetter(e: {
        clipboardData: {
            getData: (string) => string;
        };
    }): boolean;
    static isNotAlphanumeric(e: {
        clipboardData: {
            getData: (string) => string;
        };
    }): boolean;
}
