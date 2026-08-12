import React, { useEffect, useRef } from 'react'
import { ContentAreaMessagesFeed } from './ContentAreaMessagesFeed'
import { ContentAreaHelloSection } from './ContentAreaHelloSection'
import { useChat } from '../hooks/useChat';

export const ContentArea = () => {
    const { messages, setInputValue } = useChat();
    const chatEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [messages]);

    return (
        <div className="flex-1 overflow-y-auto px-4 md:px-8 py-6 space-y-6 flex flex-col">
            {messages.length === 0 ? (
                /* Welcome / Zero state screen */
                <ContentAreaHelloSection
                    setInputValue={setInputValue}
                />
            ) : (
                <ContentAreaMessagesFeed
                    messages={messages}
                    chatEndRef={chatEndRef}
                />
            )}
        </div>
    )
}
