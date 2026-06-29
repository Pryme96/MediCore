import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ConfigProvider } from 'antd'
import it_IT from 'antd/locale/it_IT'
import dayjs from 'dayjs'
import 'dayjs/locale/it'
import '@fontsource/inter/400.css'
import '@fontsource/inter/500.css'
import '@fontsource/inter/600.css'
import '@fontsource/inter/700.css'
import './index.css'
import App from './App.tsx'
import { palette } from './theme/colors'

dayjs.locale('it')

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      locale={it_IT}
      theme={{
        token: { colorPrimary: palette.primary, fontFamily: "'Inter', sans-serif" },
        components: {
          Table: {
            headerBg: palette.backgroundTint,
          },
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
