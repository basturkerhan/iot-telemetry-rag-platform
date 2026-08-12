"use client";

import React, {
  createContext,
  useContext,
  useEffect,
  useRef,
  useState,
  useCallback,
  ReactNode,
  useMemo,
} from "react";
import * as signalR from "@microsoft/signalr";
import ChatMessage from "../models/ChatMessage";

export interface AskAIQueryResponseDto {
  question: string;
  answer: string;
  retrievedContextCount: number;
  contextUsed: string[];
}

export interface DeviceLatestTelemetryDto {
  deviceId: string;
  temperature: number;
  humidity: number;
  vibration: number;
  timestamp: string;
}

export interface HubResult<T> {
  value?: T;
  Value?: T;
  errors?: string[];
  Errors?: string[];
  message?: HubResult<T>;
  Message?: HubResult<T>;
  isSuccess?: boolean;
  IsSuccess?: boolean;
}

interface SignalRContextType {
  isConnected: boolean;
  connectionError: string | null;
  messages: ChatMessage[];
  telemetryData: DeviceLatestTelemetryDto[];
  setMessages: React.Dispatch<React.SetStateAction<ChatMessage[]>>;
  sendMessage: (input: string) => Promise<void>;
  connection: signalR.HubConnection | null;
}

const SignalRContext = createContext<SignalRContextType | null>(null);

export const SignalRProvider = ({ children }: { children: ReactNode }) => {
  const hubUrl: string = process.env.NEXT_PUBLIC_SIGNALR_HUB_URL || "";
  const [isConnected, setIsConnected] = useState(false);
  const [connectionError, setConnectionError] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [telemetryData, setTelemetryData] = useState<DeviceLatestTelemetryDto[]>([]);
  const [activeConnection, setActiveConnection] = useState<signalR.HubConnection | null>(null);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const streamSubscriptionRef = useRef<signalR.ISubscription<HubResult<DeviceLatestTelemetryDto[]>> | null>(null);

  const handleReceiveMessage = useCallback((response: HubResult<AskAIQueryResponseDto>) => {
    const data = response.message || response.Message || response;
    const val = data.value || data.Value;
    const errs = data.errors || data.Errors;

    setMessages((prev) => {
      const pendingIndex = [...prev].reverse().findIndex((m) => m.role === "model" && m.isPending);

      if (pendingIndex !== -1) {
        const actualIndex = prev.length - 1 - pendingIndex;
        const updated = [...prev];

        if (errs && errs.length > 0) {
          updated[actualIndex] = {
            ...updated[actualIndex],
            text: "An error occurred while calling the AI service.",
            errors: errs,
            isPending: false,
          };
        } else if (val) {
          updated[actualIndex] = {
            ...updated[actualIndex],
            text: val.answer || "No response received.",
            contextCount: val.retrievedContextCount,
            contexts: val.contextUsed,
            isPending: false,
          };
        } else {
          updated[actualIndex] = {
            ...updated[actualIndex],
            text: "Received an empty response from the hub.",
            isPending: false,
          };
        }
        return updated;
      }

      const newMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "model",
        text: val?.answer || "No response details available.",
        contextCount: val?.retrievedContextCount,
        contexts: val?.contextUsed,
        errors: errs || undefined,
        timestamp: new Date(),
      };
      return [...prev, newMsg];
    });
  }, []);

  const startTelemetryStream = useCallback((conn: signalR.HubConnection) => {
    if (streamSubscriptionRef.current) {
      streamSubscriptionRef.current.dispose();
      streamSubscriptionRef.current = null;
    }

    try {
      const subscription = conn
        .stream<HubResult<DeviceLatestTelemetryDto[]>>("StreamLatestTelemetryAsync")
        .subscribe({
          next: (response) => {
            const data = response.message || response.Message || response;
            const records = data.value || data.Value;
            if (records && Array.isArray(records)) {
              setTelemetryData(records);
            }
          },
          error: (err) => {
            console.error("Telemetry stream error:", err);
          },
          complete: () => {
            console.log("Telemetry stream completed.");
          },
        });

      streamSubscriptionRef.current = subscription;
    } catch (err) {
      console.error("Failed to start telemetry stream:", err);
    }
  }, []);

  useEffect(() => {
    if (!hubUrl) {
      setConnectionError("SignalR Hub URL is missing.");
      return;
    }

    let isDisposed = false;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.serverTimeoutInMilliseconds = 300000;

    connection.on("ReceiveMessage", (response: HubResult<AskAIQueryResponseDto>) => {
      handleReceiveMessage(response);
    });

    connection.onclose((err) => {
      if (isDisposed) return;
      if (streamSubscriptionRef.current) {
        streamSubscriptionRef.current.dispose();
        streamSubscriptionRef.current = null;
      }
      setIsConnected(false);
      if (err) setConnectionError(err.message);
    });

    connection.onreconnecting((err) => {
      if (isDisposed) return;
      if (streamSubscriptionRef.current) {
        streamSubscriptionRef.current.dispose();
        streamSubscriptionRef.current = null;
      }
      setIsConnected(false);
      if (err) setConnectionError(err.message);
    });

    connection.onreconnected(() => {
      if (isDisposed) return;
      setIsConnected(true);
      setConnectionError(null);
      startTelemetryStream(connection);
    });


    connectionRef.current = connection;
    setActiveConnection(connection);

    connection
      .start()
      .then(() => {
        if (isDisposed) {
          connection.stop();
          return;
        }
        setIsConnected(true);
        setConnectionError(null);
        startTelemetryStream(connection);
      })
      .catch((err) => {
        if (isDisposed) return;
        setConnectionError(err instanceof Error ? err.message : String(err));
        setIsConnected(false);
      });

    return () => {
      isDisposed = true;

      if (streamSubscriptionRef.current) {
        streamSubscriptionRef.current.dispose();
        streamSubscriptionRef.current = null;
      }

      connection.off("ReceiveMessage");

      if (connectionRef.current === connection) {
        connectionRef.current = null;
        setActiveConnection(null);
      }

      if (connection.state === signalR.HubConnectionState.Connected) {
        connection.stop().catch(() => {});
      }
    };
  }, [hubUrl, handleReceiveMessage, startTelemetryStream]);

  const sendMessage = useCallback(async (input: string) => {
    const conn = connectionRef.current;
    
    if (!conn || conn.state !== signalR.HubConnectionState.Connected) {
      throw new Error(`SignalR is not connected (State: ${conn?.state ?? "null"})`);
    }

    await conn.invoke("AskAIAsync", input);
  }, []);

  const values = useMemo(() => {
    return {
      isConnected,
      connectionError,
      messages,
      telemetryData,
      setMessages,
      sendMessage,
      connection: activeConnection,
    };
  }, [isConnected, connectionError, messages, telemetryData, sendMessage, activeConnection]);

  return <SignalRContext.Provider value={values}>{children}</SignalRContext.Provider>;
};

export const useSignalRContext = (): SignalRContextType => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error("useSignalRContext must be used within a SignalRProvider");
  }
  return context;
};