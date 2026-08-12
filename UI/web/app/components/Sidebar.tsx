import { PanelLeftClose, Cpu, Thermometer, Droplets, Activity, Clock } from "lucide-react";
import { useSignalRContext } from "../contexts/analyticsHub";

interface SidebarProps {
  sidebarOpen: boolean;
  setSidebarOpen: (open: boolean) => void;
}

export const Sidebar = ({ sidebarOpen, setSidebarOpen }: SidebarProps) => {
  const { telemetryData } = useSignalRContext();

  const formatDate = (dateStr: string) => {
    try {
      const date = new Date(dateStr);
      if (isNaN(date.getTime())) return dateStr;
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    } catch {
      return dateStr;
    }
  };

  return (
    <aside 
      className={`${
        sidebarOpen ? "w-80" : "w-0"
      } transition-all duration-300 ease-in-out bg-[#1e1f20] flex flex-col z-20 overflow-hidden relative shrink-0 border-r border-[#2d2e30]`}
    >
      <div className="p-4 flex items-center justify-between border-b border-[#2d2e30]">
        <div className="flex items-center gap-2">
          <Cpu className="w-5 h-5 text-[#8ab4f8]" />
          <span className="font-semibold text-sm text-[#e3e3e3]">Device Panel</span>
          <span className="text-xs bg-[#2d3033] px-2 py-0.5 rounded-full text-[#8ab4f8]">
            {telemetryData.length}
          </span>
        </div>
        <button 
          onClick={() => setSidebarOpen(false)}
          className="p-2 hover:bg-[#282a2c] rounded-full transition-colors hidden md:block"
          title="Collapse menu"
        >
          <PanelLeftClose className="w-5 h-5 text-[#c4c7c5]" />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {telemetryData.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 text-[#c4c7c5] text-xs text-center px-4 space-y-2">
            <Activity className="w-8 h-8 text-[#5f6368] animate-pulse" />
            <p>Telemetry data not available...</p>
          </div>
        ) : (
          telemetryData.map((device) => (
            <div 
              key={device.deviceId} 
              className="bg-[#282a2c] rounded-xl p-4 border border-[#3c4043] hover:border-[#8ab4f8] transition-all duration-200 shadow-md group hover:shadow-[#8ab4f810]"
            >
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full bg-emerald-500 animate-ping absolute" />
                  <div className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                  <span className="font-medium text-sm text-[#f1f3f4] group-hover:text-[#8ab4f8] transition-colors truncate max-w-[150px]">
                    {device.deviceId}
                  </span>
                </div>
                <div className="flex items-center gap-1 text-[10px] text-[#9aa0a6]">
                  <Clock className="w-3 h-3" />
                  <span>{formatDate(device.timestamp)}</span>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-2 border-t border-[#3c4043]">
                <div className="flex flex-col items-center bg-[#1e1f20] p-2 rounded-lg">
                  <Thermometer className="w-4 h-4 text-orange-400 mb-1" />
                  <span className="text-[10px] text-[#9aa0a6]">Temperature</span>
                  <span className="text-xs font-semibold text-white mt-0.5">
                    {device.temperature.toFixed(1)}°C
                  </span>
                </div>

                <div className="flex flex-col items-center bg-[#1e1f20] p-2 rounded-lg">
                  <Droplets className="w-4 h-4 text-blue-400 mb-1" />
                  <span className="text-[10px] text-[#9aa0a6]">Humidity</span>
                  <span className="text-xs font-semibold text-white mt-0.5">
                    {device.humidity.toFixed(1)}%
                  </span>
                </div>

                <div className="flex flex-col items-center bg-[#1e1f20] p-2 rounded-lg">
                  <Activity className="w-4 h-4 text-purple-400 mb-1" />
                  <span className="text-[10px] text-[#9aa0a6]">Vibration</span>
                  <span className="text-xs font-semibold text-white mt-0.5">
                    {device.vibration.toFixed(1)}
                  </span>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </aside>
  );
};

