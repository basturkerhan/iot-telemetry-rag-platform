import { AlertTriangle, Bot, ChevronDown, ChevronUp, Database, User } from 'lucide-react'
import React, { useState } from 'react'
import ChatMessage from '../models/ChatMessage'

interface ContentAreaMessagesFeedProps {
    messages: ChatMessage[];
    chatEndRef: any;
}

function parseInlineMarkdown(text: string): React.ReactNode[] {
    const parts = text.split(/(\*\*.*?\*\*)/g);
    return parts.map((part, idx) => {
        if (part.startsWith("**") && part.endsWith("**")) {
            return <strong key={idx} className="font-bold text-white">{part.slice(2, -2)}</strong>;
        }
        return part;
    });
}

function renderMarkdown(text: string) {
    const lines = text.split("\n");
    let inList = false;
    const listItems: React.ReactNode[] = [];
    const elements: React.ReactNode[] = [];

    const flushList = (key: string) => {
        if (inList && listItems.length > 0) {
            elements.push(
                <ul key={`list-${key}`} className="list-disc pl-5 my-2 space-y-1">
                    {...listItems}
                </ul>
            );
            listItems.length = 0;
            inList = false;
        }
    };

    lines.forEach((line, index) => {
        const trimmed = line.trim();

        if (trimmed.startsWith("### ")) {
            flushList(String(index));
            elements.push(
                <h3 key={index} className="text-base md:text-lg font-bold text-white mt-4 mb-2">
                    {parseInlineMarkdown(trimmed.slice(4))}
                </h3>
            );
        } else if (trimmed.startsWith("## ")) {
            flushList(String(index));
            elements.push(
                <h2 key={index} className="text-lg md:text-xl font-bold text-white mt-5 mb-2">
                    {parseInlineMarkdown(trimmed.slice(3))}
                </h2>
            );
        } else if (trimmed.match(/^\d+\.\s/)) {
            flushList(String(index));
            const dotIndex = line.indexOf(".");
            const listContent = line.substring(dotIndex + 1).trim();
            elements.push(
                <div key={index} className="font-semibold text-white mt-3 mb-1">
                    {line.substring(0, dotIndex + 1)} {parseInlineMarkdown(listContent)}
                </div>
            );
        } else if (trimmed.startsWith("* ") || trimmed.startsWith("- ")) {
            inList = true;
            listItems.push(
                <li key={index} className="text-sm md:text-base text-[#e3e3e3] leading-relaxed">
                    {parseInlineMarkdown(trimmed.slice(2))}
                </li>
            );
        } else if (trimmed === "") {
            flushList(String(index));
        } else {
            flushList(String(index));
            elements.push(
                <p key={index} className="my-2 leading-relaxed text-sm md:text-base text-[#e3e3e3]">
                    {parseInlineMarkdown(line)}
                </p>
            );
        }
    });

    flushList("end");
    return elements;
}




export const ContentAreaMessagesFeed = ({ messages, chatEndRef }: ContentAreaMessagesFeedProps) => {
    const [expandedContexts, setExpandedContexts] = useState<Record<string, boolean>>({});

    const toggleContext = (msgId: string) => {
        setExpandedContexts((prev) => ({
            ...prev,
            [msgId]: !prev[msgId]
        }));
    };

    const MessageBox = (key: string, msg: ChatMessage) => (
        <div key={key} className="flex gap-4 md:gap-6 animate-fade-in">
            {/* Sender Icon */}
            <div className={`w-8 h-8 rounded-full flex items-center justify-center shrink-0 shadow-sm ${msg.role === "user"
                ? "bg-[#2d2e30] text-[#e3e3e3]"
                : "bg-[#2b3950] text-[#8ab4f8] border border-[#3e557b]/30"
                }`}>
                {msg.role === "user" ? (
                    <User className="w-4 h-4" />
                ) : (
                    <Bot className="w-4 h-4" />
                )}
            </div>

            {/* Message Body */}
            <div className="flex-1 space-y-3 overflow-hidden">
                <div className="flex items-center gap-2">
                    <span className="text-xs font-semibold text-[#80868b]">
                        {msg.role === "user" ? "You" : "IOT Assistant"}
                    </span>
                    <span className="text-[10px] text-[#5f6368]">
                        {msg.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                </div>

                {msg.isPending ? (
                    /* Thinking/Loading State skeleton */
                    <div className="space-y-2 animate-pulse py-2">
                        <div className="h-4 bg-[#2d2e30] rounded w-full"></div>
                        <div className="h-4 bg-[#2d2e30] rounded w-[90%]"></div>
                        <div className="h-4 bg-[#2d2e30] rounded w-[60%]"></div>
                    </div>
                ) : (
                    /* Text Content */
                    <div className="text-sm md:text-base leading-relaxed text-[#e3e3e3]">
                        {renderMarkdown(msg.text)}
                    </div>
                )}

                {/* Backend errors */}
                {msg.errors && msg.errors.length > 0 && (
                    <div className="mt-2 p-3 bg-red-950/20 border border-red-900/30 rounded-lg text-xs text-red-300 flex gap-2 items-start">
                        <AlertTriangle className="w-4 h-4 shrink-0 mt-0.5" />
                        <div>
                            <p className="font-semibold mb-1">Response Errors:</p>
                            <ul className="list-disc pl-4 space-y-1">
                                {msg.errors.map((err, idx) => (
                                    <li key={idx}>{err}</li>
                                ))}
                            </ul>
                        </div>
                    </div>
                )}

                {/* Context Used Panel */}
                {!msg.isPending && (msg.contextCount !== undefined || (msg.contexts && msg.contexts.length > 0)) && (
                    <div className="mt-4 border border-[#2d2e30] bg-[#1e1f20] rounded-xl overflow-hidden text-xs">
                        <button
                            onClick={() => toggleContext(msg.id)}
                            className="w-full flex items-center justify-between p-3 text-[#c4c7c5] hover:bg-[#282a2c] hover:text-white transition-colors"
                        >
                            <span className="flex items-center gap-2 font-medium">
                                <Database className="w-4 h-4 text-[#8ab4f8]" />
                                Retrieved Context count: <strong className="text-white">{msg.contextCount ?? 0}</strong>
                            </span>
                            {expandedContexts[msg.id] ? (
                                <ChevronUp className="w-4 h-4" />
                            ) : (
                                <ChevronDown className="w-4 h-4" />
                            )}
                        </button>

                        {expandedContexts[msg.id] && msg.contexts && msg.contexts.length > 0 && (
                            <div className="p-3 border-t border-[#2d2e30] bg-[#131314] text-[#9aa0a6] space-y-2 max-h-40 overflow-y-auto">
                                <p className="font-medium text-[#c4c7c5] mb-1">Sources utilized:</p>
                                <ul className="list-decimal pl-4 space-y-1">
                                    {msg.contexts.map((ctx, idx) => (
                                        <li key={idx} className="font-mono text-[11px] break-all">{ctx}</li>
                                    ))}
                                </ul>
                            </div>
                        )}
                    </div>
                )}
            </div>
        </div>
    )

    return (
        <div className="flex-1 max-w-3xl mx-auto w-full space-y-8 pb-12">
            {messages.map((msg: ChatMessage) => (
                MessageBox(msg.id, msg)
            ))}
            <div ref={chatEndRef} />
        </div>
    )
}
