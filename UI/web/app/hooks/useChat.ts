"use client";

import { useState } from "react";
import { useSignalRContext } from "../contexts/analyticsHub";

export const useChat = () => {
  const [inputValue, setInputValue] = useState("");
  const { isConnected, sendMessage, messages, setMessages } = useSignalRContext();

  const handleSend = async (textToSend?: string) => {
    const queryText = textToSend || inputValue;
    if (!queryText.trim()) return;

    if (!textToSend) {
      setInputValue("");
    }

    const userMessageId = crypto.randomUUID();
    const modelMessageId = crypto.randomUUID();

    // 1. Kullanıcı mesajı ve beklemede olan model mesajı eklenir
    setMessages((prev) => [
      ...prev,
      {
        id: userMessageId,
        role: "user",
        text: queryText,
        timestamp: new Date(),
      },
      {
        id: modelMessageId,
        role: "model",
        text: "",
        isPending: true,
        timestamp: new Date(),
      },
    ]);

    // 2. Hub çağrısı yapılır
    try {
      if (!isConnected) {
        throw new Error("SignalR connection is not established. Check your backend status.");
      }
      await sendMessage(queryText);
    } catch (err) {
      setMessages((prev) =>
        prev.map((msg) =>
          msg.id === modelMessageId
            ? {
                ...msg,
                text: err instanceof Error ? err.message : "Failed to send message.",
                isPending: false,
                errors: [err instanceof Error ? err.message : String(err)],
              }
            : msg
        )
      );
    }
  };

  return {
    messages,
    setMessages,
    inputValue,
    setInputValue,
    handleSend,
    isConnected,
  };
};