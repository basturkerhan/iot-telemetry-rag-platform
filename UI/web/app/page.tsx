"use client";

import { useState } from "react";
import { Header } from "./components/Header";
import { Sidebar } from "./components/Sidebar";
import { InputBar } from "./components/InputBar";
import { ContentArea } from "./components/ContentArea";

export default function Home() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  
  return (
    <div className="flex h-screen w-full bg-[#131314] text-[#e3e3e3] font-sans overflow-hidden">
      {/* Sidebar */}
      <Sidebar
        sidebarOpen={sidebarOpen}
        setSidebarOpen={setSidebarOpen}
      />

      {/* Main Container */}
      <main className="flex-1 flex flex-col min-w-0 relative">
        {/* Header */}
        <Header
          sidebarOpen={sidebarOpen}
          setSidebarOpen={setSidebarOpen}
        />
        {/* Content Area */}
        <ContentArea />
        {/* Input Bar Section */}
        <InputBar />
      </main>
    </div>
  );
}
