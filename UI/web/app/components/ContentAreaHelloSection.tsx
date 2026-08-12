import { ArrowRight } from 'lucide-react';
import { useChat } from '../hooks/useChat';

const suggestionCards = [
    { 
        title: "Identify Critical Anomalies", 
        prompt: "Analyze the last 1 hour of telemetry. Are there any devices with vibration levels exceeding 3.0 or temperature spikes above 33°C?" 
    },
    { 
        title: "Device Health Summary", 
        prompt: "Provide a quick status report for all active devices, focusing on the latest temperature and humidity trends." 
    },
    { 
        title: "Vibration Pattern Analysis", 
        prompt: "Compare vibration patterns across DEV-101, DEV-102, and DEV-103. Which device shows the most unstable behavior?" 
    },
    { 
        title: "Maintenance Prediction", 
        prompt: "Based on recent telemetry data, which device should be prioritized for hardware inspection due to high vibration levels?" 
    }
];

interface ContentAreaHelloSectionProps {
    setInputValue: (value: string) => void;
}

export const ContentAreaHelloSection = ({ setInputValue }: ContentAreaHelloSectionProps) => {
    const { handleSend } = useChat();

    return (
        <div className="flex-1 flex flex-col justify-center max-w-3xl mx-auto w-full py-8 space-y-12">
            <div className="space-y-3">
                <h1
                    className="text-4xl md:text-5xl font-medium tracking-tight bg-linear-to-r from-[#5983ef] via-[#c276f5] to-[#f58476] animate-gradient-x py-1"
                    style={{ WebkitBackgroundClip: "text", WebkitTextFillColor: "transparent" }}
                >
                    Hello,
                </h1>
                <h2 className="text-3xl md:text-4xl font-medium text-[#444746]">
                    How can I help you today?
                </h2>
            </div>

            {/* Cards Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {suggestionCards.map((card, idx) => (
                    <button
                        key={idx}
                        onClick={() => {
                            setInputValue(card.prompt);
                            handleSend(card.prompt);
                        }}
                        className="p-5 bg-[#1e1f20] hover:bg-[#282a2c] text-left rounded-xl transition-all border border-[#2d2e30] group hover:border-[#3c4043] flex flex-col justify-between h-40 hover:shadow-lg relative overflow-hidden"
                    >
                        <span className="text-sm font-medium text-[#e3e3e3] leading-relaxed group-hover:text-white">
                            {card.prompt}
                        </span>
                        <div className="flex items-center justify-between mt-4">
                            <span className="text-xs text-[#9aa0a6] font-normal">
                                {card.title}
                            </span>
                            <div className="w-8 h-8 rounded-full bg-[#131314] flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                <ArrowRight className="w-4 h-4 text-[#8ab4f8]" />
                            </div>
                        </div>
                    </button>
                ))}
            </div>
        </div>
    )
}
