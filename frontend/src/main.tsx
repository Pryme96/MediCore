import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ConfigProvider } from 'antd'
import '@fontsource/inter/400.css'
import '@fontsource/inter/500.css'
import '@fontsource/inter/600.css'
import '@fontsource/inter/700.css'
import './index.css'
import App from './App.tsx'
import { palette } from './theme/colors'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      theme={{
        token: { colorPrimary: palette.primary, fontFamily: "'Inter', sans-serif" },
        components: {
          Menu: {
            motionDurationSlow: '0.05s',
            motionDurationMid: '0.05s',
            darkItemBg: palette.primary,
            darkItemColor: '#ffffff',
            darkItemHoverBg: palette.primaryDark,
            darkItemSelectedBg: `${palette.backgroundTint}AA`,
            darkItemSelectedColor: palette.primaryDark,
          },
        },
      }}
    >
      <App />
    </ConfigProvider>
  </StrictMode>,
)
