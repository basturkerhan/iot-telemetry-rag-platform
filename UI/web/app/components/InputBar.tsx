import { Send } from 'lucide-react';
import { useChat } from '../hooks/useChat';
import { useSignalRContext } from '../contexts/analyticsHub';

export const InputBar = () => {
  const { handleSend, inputValue, setInputValue } = useChat();
  const { isConnected } = useSignalRContext();

  return (
    <div className="bg-[#131314] border-t border-[#2d2e30] p-4 md:p-6">
      <div className="max-w-3xl mx-auto w-full flex flex-col gap-2">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            handleSend();
          }}
          className="relative flex items-center bg-[#1e1f20] hover:bg-[#282a2c] focus-within:bg-[#282a2c] rounded-full border border-[#2d2e30] focus-within:border-[#8ab4f8] transition-all px-4 py-2"
        >
          <input
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            placeholder="Ask IOT Assistant..."
            className="flex-1 bg-transparent border-0 outline-none focus:ring-0 text-sm md:text-base text-white px-2 py-2 placeholder-[#80868b]"
          />

          <button
            type="submit"
            disabled={!inputValue.trim() || !isConnected}
            className={`p-2.5 rounded-full transition-all shrink-0 ${inputValue.trim() && isConnected
                ? "bg-[#8ab4f8] text-[#131314] hover:bg-white cursor-pointer shadow-md"
                : "bg-[#2d2e30] text-[#5f6368] cursor-not-allowed"
              }`}
          >
            <Send className="w-4 h-4" />
          </button>
        </form>

        <span className="text-[11px] text-[#80868b] text-center mt-1">
          IOT Assistant may display inaccurate info, including about people, so double-check its responses.
        </span>
      </div>
    </div>
  )
}
