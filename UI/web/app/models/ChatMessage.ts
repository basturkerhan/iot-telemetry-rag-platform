export default interface ChatMessage {
  id: string;
  role: "user" | "model";
  text: string;
  contextCount?: number;
  contexts?: string[];
  errors?: string[];
  isPending?: boolean;
  timestamp: Date;
}