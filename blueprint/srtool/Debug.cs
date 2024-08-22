namespace srtool
{
    public static class Debug
    {
        public static void InitConsoleSetup()
        {
            OnLog += (log) =>
              {
                  switch (log.type)
                  {
                      case LogObject.Type.Log:
                          {
                              Console.ForegroundColor = ConsoleColor.White;
                              Console.WriteLine(log.log);
                          }
                          break;
                      case LogObject.Type.System:
                          {
                              Console.ForegroundColor = ConsoleColor.Green;
                              Console.WriteLine(log.log);
                          }
                          break;
                      case LogObject.Type.Error:
                          {
                              if (log.exception != null)
                              {
                                  Console.ForegroundColor = ConsoleColor.Red;
                                  Console.WriteLine(log.exception.Message);
                                  Console.ForegroundColor = ConsoleColor.DarkRed;
                                  Console.WriteLine(log.exception.StackTrace);
                              }
                          }
                          break;
                  }
              };
        }
        public delegate void ONLOG(LogObject log);

        public static event ONLOG OnLog;
        public static void Log(object log)
        {
            if (log != null)
                Log(log.ToString());
            else
                Log("NULL OBJECT");
        }
        public static void Log(string log)
        {
            LogObject _log = new LogObject()
            {
                type = LogObject.Type.Log,
                log = log,
                datetime = DateTime.Now
            };
            OnLog?.Invoke(_log);
        }
        public static void Warning(string log)
        {
            LogObject _log = new LogObject()
            {
                type = LogObject.Type.Warning,
                log = log,
                datetime = DateTime.Now
            };
            OnLog?.Invoke(_log);
        }
        public static void System(string log)
        {
            LogObject _log = new LogObject()
            {
                type = LogObject.Type.System,
                log = log,
                datetime = DateTime.Now
            };
            OnLog?.Invoke(_log);
        }
        public static void Error(Exception e)
        {
            LogObject _log = new LogObject()
            {
                type = LogObject.Type.Error,
                exception = e,
                datetime = DateTime.Now
            };
            OnLog?.Invoke(_log);
        }
        public static void Error(string log, Exception e)
        {
            LogObject _log = new LogObject()
            {
                type = LogObject.Type.Error,
                log = log,
                exception = e,
                datetime = DateTime.Now
            };
            OnLog?.Invoke(_log);
        }

        public struct LogObject
        {
            public string log;
            public Type type;
            public Exception exception;
            public DateTime datetime;
            public enum Type
            {
                System,
                Error,
                Log,
                Warning,
            }
        }
    }
}