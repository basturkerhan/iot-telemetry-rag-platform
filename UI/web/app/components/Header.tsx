import { PanelLeft, Wifi, WifiOff } from 'lucide-react';
import { useSignalRContext } from '../contexts/analyticsHub';

interface HeaderProps {
    sidebarOpen: boolean;
    setSidebarOpen: (open: boolean) => void;
}

export const Header = ({ sidebarOpen, setSidebarOpen }: HeaderProps) => {
  const {isConnected, connectionError} = useSignalRContext();

  return (
    <header className="h-16 flex items-center justify-between px-4 border-b border-[#2d2e30] bg-[#131314]">
          <div className="flex items-center gap-2">
            {!sidebarOpen && (
              <button 
                onClick={() => setSidebarOpen(true)}
                className="p-2 hover:bg-[#282a2c] rounded-full transition-colors mr-2"
                title="Expand menu"
              >
                <PanelLeft className="w-5 h-5 text-[#c4c7c5]" />
              </button>
            )}
            <div className="flex items-center gap-2 font-medium text-lg">
              <span 
                className="bg-linear-to-r from-[#4285f4] via-[#9b51e0] to-[#ea4335] font-bold tracking-tight text-xl"
                style={{ WebkitBackgroundClip: "text", WebkitTextFillColor: "transparent" }}
              >
                IOT Assistant
              </span>
              <span className="text-xs px-2 py-0.5 rounded-full bg-[#2d2e30] text-[#c4c7c5] font-normal">
                Analytics Hub
              </span>
            </div>
          </div>

          {/* Connection Status indicator */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2 text-xs">
              {isConnected ? (
                <span className="flex items-center gap-1.5 text-green-400 bg-green-500/10 px-2.5 py-1 rounded-full border border-green-500/20">
                  <Wifi className="w-3.5 h-3.5" /> Connected
                </span>
              ) : (
                <span 
                  className="flex items-center gap-1.5 text-red-400 bg-red-500/10 px-2.5 py-1 rounded-full border border-red-500/20"
                  title={connectionError || "Disconnected"}
                >
                  <WifiOff className="w-3.5 h-3.5" /> Offline
                </span>
              )}
            </div>
          </div>
        </header>
  )
}
