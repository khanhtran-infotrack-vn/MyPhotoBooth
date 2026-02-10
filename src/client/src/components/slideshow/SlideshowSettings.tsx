import type { SlideshowConfig } from '../../types/slideshow'

interface SlideshowSettingsProps {
  config: SlideshowConfig
  onConfigChange: (config: Partial<SlideshowConfig>) => void
  onClose: () => void
}

export function SlideshowSettings({ config, onConfigChange, onClose }: SlideshowSettingsProps) {
  return (
    <div className="absolute inset-0 flex items-center justify-center bg-black/70 z-10" onClick={onClose}>
      <div
        className="bg-gray-900 rounded-2xl p-6 max-w-sm w-full mx-4 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-white">Slideshow Settings</h2>
          <button
            onClick={onClose}
            className="p-1 text-white/70 hover:text-white transition-colors"
          >
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Timing */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-white/80 mb-2">Slide Timing</label>
          <div className="grid grid-cols-4 gap-2">
            {[3, 5, 10, 15].map((seconds) => (
              <button
                key={seconds}
                onClick={() => onConfigChange({ timing: seconds })}
                className={`py-2 px-3 rounded-lg font-medium text-sm transition-all ${
                  config.timing === seconds
                    ? 'bg-primary-600 text-white'
                    : 'bg-gray-800 text-white/70 hover:bg-gray-700'
                }`}
              >
                {seconds}s
              </button>
            ))}
          </div>
        </div>

        {/* Shuffle */}
        <div className="mb-4">
          <label className="flex items-center justify-between cursor-pointer">
            <span className="text-white">Shuffle Photos</span>
            <div
              onClick={() => onConfigChange({ shuffle: !config.shuffle })}
              className={`relative w-12 h-6 rounded-full transition-colors ${
                config.shuffle ? 'bg-primary-600' : 'bg-gray-700'
              }`}
            >
              <div
                className={`absolute top-1 w-4 h-4 rounded-full bg-white transition-transform ${
                  config.shuffle ? 'left-7' : 'left-1'
                }`}
              />
            </div>
          </label>
        </div>

        {/* Loop */}
        <div className="mb-4">
          <label className="flex items-center justify-between cursor-pointer">
            <span className="text-white">Loop Slideshow</span>
            <div
              onClick={() => onConfigChange({ loop: !config.loop })}
              className={`relative w-12 h-6 rounded-full transition-colors ${
                config.loop ? 'bg-primary-600' : 'bg-gray-700'
              }`}
            >
              <div
                className={`absolute top-1 w-4 h-4 rounded-full bg-white transition-transform ${
                  config.loop ? 'left-7' : 'left-1'
                }`}
              />
            </div>
          </label>
        </div>

        {/* Ken Burns Effect */}
        <div className="mb-4">
          <label className="flex items-center justify-between cursor-pointer">
            <span className="text-white">Ken Burns Effect</span>
            <div
              onClick={() => onConfigChange({ kenBurns: !config.kenBurns })}
              className={`relative w-12 h-6 rounded-full transition-colors ${
                config.kenBurns ? 'bg-primary-600' : 'bg-gray-700'
              }`}
            >
              <div
                className={`absolute top-1 w-4 h-4 rounded-full bg-white transition-transform ${
                  config.kenBurns ? 'left-7' : 'left-1'
                }`}
              />
            </div>
          </label>
        </div>

        {/* Ken Burns Direction */}
        {config.kenBurns && (
          <div className="mb-6">
            <label className="block text-sm font-medium text-white/80 mb-2">Animation Direction</label>
            <select
              value={config.kenBurnsDirection}
              onChange={(e) => onConfigChange({ kenBurnsDirection: e.target.value as any })}
              className="w-full bg-gray-800 text-white rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="random">Random</option>
              <option value="zoom-in">Zoom In</option>
              <option value="zoom-out">Zoom Out</option>
              <option value="pan-left">Pan Left</option>
              <option value="pan-right">Pan Right</option>
            </select>
          </div>
        )}

        {/* Keyboard shortcuts hint */}
        <div className="mt-6 pt-6 border-t border-white/10">
          <p className="text-xs text-white/50 text-center">
            Space: Play/Pause | Arrows: Navigate | F: Fullscreen | S: Settings | L: Loop | K: Ken Burns
          </p>
        </div>
      </div>
    </div>
  )
}
