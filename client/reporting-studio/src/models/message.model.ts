import { CommandType } from "./command-type.model";

export interface Message {
    commandType: CommandType;
    message?: string;
}