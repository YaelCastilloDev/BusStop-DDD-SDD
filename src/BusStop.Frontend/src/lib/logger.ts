type LogLevel = 'debug' | 'info' | 'warn' | 'error'

const LEVEL_PRIORITY: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
}

interface LoggerConfig {
  minLevel: LogLevel
}

let globalConfig: LoggerConfig = {
  minLevel: import.meta.env.DEV ? 'debug' : 'warn',
}

export function configureLogger(config: Partial<LoggerConfig>): void {
  globalConfig = { ...globalConfig, ...config }
}

function shouldLog(level: LogLevel): boolean {
  return LEVEL_PRIORITY[level] >= LEVEL_PRIORITY[globalConfig.minLevel]
}

function formatTime(): string {
  const now = new Date()
  const hh = String(now.getHours()).padStart(2, '0')
  const mm = String(now.getMinutes()).padStart(2, '0')
  const ss = String(now.getSeconds()).padStart(2, '0')
  const ms = String(now.getMilliseconds()).padStart(3, '0')
  return `${hh}:${mm}:${ss}.${ms}`
}

function write(
  level: LogLevel,
  module: string,
  message: string,
  data?: unknown
): void {
  const prefix = `${formatTime()} ${level.toUpperCase().padEnd(5)} [${module}]`

  if (data !== undefined) {
    switch (level) {
      case 'debug':
        // eslint-disable-next-line no-console
        console.debug(prefix, message, data)
        break
      case 'info':
        // eslint-disable-next-line no-console
        console.info(prefix, message, data)
        break
      case 'warn':
        // eslint-disable-next-line no-console
        console.warn(prefix, message, data)
        break
      case 'error':
        // eslint-disable-next-line no-console
        console.error(prefix, message, data)
        break
    }
  } else {
    switch (level) {
      case 'debug':
        // eslint-disable-next-line no-console
        console.debug(prefix, message)
        break
      case 'info':
        // eslint-disable-next-line no-console
        console.info(prefix, message)
        break
      case 'warn':
        // eslint-disable-next-line no-console
        console.warn(prefix, message)
        break
      case 'error':
        // eslint-disable-next-line no-console
        console.error(prefix, message)
        break
    }
  }
}

export interface Logger {
  debug: (message: string, data?: unknown) => void
  info: (message: string, data?: unknown) => void
  warn: (message: string, data?: unknown) => void
  error: (message: string, data?: unknown) => void
}

export function createLogger(module: string): Logger {
  return {
    debug: (message, data) => {
      if (shouldLog('debug')) write('debug', module, message, data)
    },
    info: (message, data) => {
      if (shouldLog('info')) write('info', module, message, data)
    },
    warn: (message, data) => {
      if (shouldLog('warn')) write('warn', module, message, data)
    },
    error: (message, data) => {
      if (shouldLog('error')) write('error', module, message, data)
    },
  }
}
