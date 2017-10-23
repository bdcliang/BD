namespace BD.Core
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    public enum AppRole
    {
        Server,
        Client,
        None
    }
    public sealed class Application
    {
        
        public static void EnvironmentExit()
        {
            Environment.Exit(0);
        }

        private static AppRole AppRole=AppRole.None;
        /// <summary>
        /// 配置应用程序
        /// </summary>
        /// <param name="role">程序角色</param>
        /// <param name="serverip1">服务器IP</param>
        /// <param name="failAction">查询结果的后续处理，false 失败，true成功</param>
        public static void AppConfig(AppRole role=AppRole.None,string serverip1 = "192.168.1.201",Action<bool> failAction=null)
        {
            AppRole = role;
            serverip = serverip1;
            failHandler = failAction;
            EmbedCheck();
        }
        private static BD.Net.UdpServer udp = new BD.Net.UdpServer();
        private static int FailCounter;
        private static int TimerCounter;
        private static string serverip = "192.168.1.201";
        private static System.Timers.Timer CheckTimer = new System.Timers.Timer(1000);//30000
        private static Action<bool> failHandler = null;
        private static void EmbedCheck()
        {   
            try { 
            if (AppRole == AppRole.None)
                return;
            if(AppRole==AppRole.Client)
            {
                udp.Listen(1866);
                udp.DataReceived += (s,e) => {
                    var tmp = e.RecString;
                    TimerCounter = 0;
                    if (tmp.StartsWith("#license#"))
                    {
                        var vali = tmp.Split('#')[2];
                        if(vali.ToLower()=="false")
                        {
                            FailCounter++;
                            if(FailCounter>5)//10
                            {
                                FailCounter = 0;
                                failHandler?.Invoke(false);
                            }
                        }
                        else if (vali.ToLower() == "true")
                        {
                            failHandler?.Invoke(true);
                            FailCounter = 0;
                        }
                    }
                };
                BD.Net.UdpClient.Send(serverip, 1867, "#query#license");
                CheckTimer.Enabled = true;
                CheckTimer.Elapsed +=(s,e)=>{
                    BD.Net.UdpClient.Send(serverip, 1867, "#query#license");
                    TimerCounter++;
                    if(TimerCounter>10)
                    {
                        failHandler?.Invoke(false);
                        TimerCounter = 0;
                    }                  
                };
            }
            else
            {
                udp.Listen(1867);
                udp.DataReceived += (s, e) => {                   
                    if (e.RecString == "#query#license")
                    {
                        try { var vali = BD.Security.HardLicenseHelper.Validate();
                            if (vali)
                                BD.Net.UdpClient.Send(e.IP, 1866, "#license#true");
                            else
                                BD.Net.UdpClient.Send(e.IP, 1866, "#license#false");
                        } catch {
                            return;
                        }                                                
                    }
                };
            }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        static Application()
        {
            System.Windows.Forms.Application.ApplicationExit += (s, e) => { ApplicationExit?.Invoke(s, e); };
            System.Windows.Forms.Application.EnterThreadModal += (s, e) => { EnterThreadModal?.Invoke(s, e); };
            System.Windows.Forms.Application.Idle += (s, e) => { Idle?.Invoke(s, e); };
            System.Windows.Forms.Application.ThreadException += (s, e) => { ThreadException?.Invoke(s, e); };
            System.Windows.Forms.Application.ThreadExit += (s, e) => { ThreadExit?.Invoke(s, e); };
            System.Windows.Forms.Application.LeaveThreadModal += (s, e) => { LeaveThreadModal?.Invoke(s, e); };

        }
        //
        // 摘要:
        //     Gets a value indicating whether the caller can quit this application.
        //
        // 返回结果:
        //     true if the caller can quit this application; otherwise, false.
        public static bool AllowQuit { get { return System.Windows.Forms.Application.AllowQuit; } }
        //
        // 摘要:
        //     Gets the path for the application data that is shared among all users.
        //
        // 返回结果:
        //     The path for the application data that is shared among all users.
        public static string CommonAppDataPath { get { return System.Windows.Forms.Application.CommonAppDataPath; } }
        //
        // 摘要:
        //     Gets the registry key for the application data that is shared among all users.
        //
        // 返回结果:
        //     A Microsoft.Win32.RegistryKey representing the registry key of the application
        //     data that is shared among all users.
        public static RegistryKey CommonAppDataRegistry { get { return System.Windows.Forms.Application.CommonAppDataRegistry; } }
        //
        // 摘要:
        //     Gets the company name associated with the application.
        //
        // 返回结果:
        //     The company name.
        public static string CompanyName { get { return System.Windows.Forms.Application.CompanyName; } }
        //
        // 摘要:
        //     Gets or sets the culture information for the current thread.
        //
        // 返回结果:
        //     A System.Globalization.CultureInfo representing the culture information for the
        //     current thread.
        public static CultureInfo CurrentCulture { get {
                return System.Windows.Forms.Application.CurrentCulture;
            } set
            {
                System.Windows.Forms.Application.CurrentCulture = value;
            } }
        //
        // 摘要:
        //     Gets or sets the current input language for the current thread.
        //
        // 返回结果:
        //     An System.Windows.Forms.InputLanguage representing the current input language
        //     for the current thread.
        public static InputLanguage CurrentInputLanguage { get {
                return System.Windows.Forms.Application.CurrentInputLanguage;
            } set {
                System.Windows.Forms.Application.CurrentInputLanguage = value;
            } }
        //
        // 摘要:
        //     Gets the path for the executable file that started the application, including
        //     the executable name.
        //
        // 返回结果:
        //     The path and executable name for the executable file that started the application.This
        //     path will be different depending on whether the Windows Forms application is
        //     deployed using ClickOnce. ClickOnce applications are stored in a per-user application
        //     cache in the C:\Documents and Settings\username directory. For more information,
        //     see Accessing Local and Remote Data in ClickOnce Applications.
        public static string ExecutablePath { get { return System.Windows.Forms.Application.ExecutablePath; } }
        //
        // 摘要:
        //     Gets the path for the application data of a local, non-roaming user.
        //
        // 返回结果:
        //     The path for the application data of a local, non-roaming user.
        public static string LocalUserAppDataPath { get { return System.Windows.Forms.Application.LocalUserAppDataPath; } }
        //
        // 摘要:
        //     Gets a value indicating whether a message loop exists on this thread.
        //
        // 返回结果:
        //     true if a message loop exists; otherwise, false.
        public static bool MessageLoop { get { return System.Windows.Forms.Application.MessageLoop; } }
        //
        // 摘要:
        //     Gets a collection of open forms owned by the application.
        //
        // 返回结果:
        //     A System.Windows.Forms.FormCollection containing all the currently open forms
        //     owned by this application.
        public static FormCollection OpenForms { get {return System.Windows.Forms.Application.OpenForms; } }
        //
        // 摘要:
        //     Gets the product name associated with this application.
        //
        // 返回结果:
        //     The product name.
        public static string ProductName { get { return System.Windows.Forms.Application.ProductName; } }
        //
        // 摘要:
        //     Gets the product version associated with this application.
        //
        // 返回结果:
        //     The product version.
        public static string ProductVersion { get { return System.Windows.Forms.Application.ProductVersion; } }
        //
        // 摘要:
        //     Gets a value specifying whether the current application is drawing controls with
        //     visual styles.
        //
        // 返回结果:
        //     true if visual styles are enabled for controls in the client area of application
        //     windows; otherwise, false.
        public static bool RenderWithVisualStyles { get { return System.Windows.Forms.Application.RenderWithVisualStyles; } }
        //
        // 摘要:
        //     Gets or sets the format string to apply to top-level window captions when they
        //     are displayed with a warning banner.
        //
        // 返回结果:
        //     The format string to apply to top-level window captions.
        public static string SafeTopLevelCaptionFormat { get {
                return System.Windows.Forms.Application.SafeTopLevelCaptionFormat;
            }
            set {
                System.Windows.Forms.Application.SafeTopLevelCaptionFormat = value;
            } }
        //
        // 摘要:
        //     Gets the path for the executable file that started the application, not including
        //     the executable name.
        //
        // 返回结果:
        //     The path for the executable file that started the application.This path will
        //     be different depending on whether the Windows Forms application is deployed using
        //     ClickOnce. ClickOnce applications are stored in a per-user application cache
        //     in the C:\Documents and Settings\username directory. For more information, see
        //     Accessing Local and Remote Data in ClickOnce Applications.
        public static string StartupPath { get { return System.Windows.Forms.Application.StartupPath; } }
        //
        // 摘要:
        //     Gets the path for the application data of a user.
        //
        // 返回结果:
        //     The path for the application data of a user.
        public static string UserAppDataPath { get { return System.Windows.Forms.Application.UserAppDataPath; } }
        //
        // 摘要:
        //     Gets the registry key for the application data of a user.
        //
        // 返回结果:
        //     A Microsoft.Win32.RegistryKey representing the registry key for the application
        //     data specific to the user.
        public static RegistryKey UserAppDataRegistry { get { return System.Windows.Forms.Application.UserAppDataRegistry; }}
        //
        // 摘要:
        //     Gets or sets whether the wait cursor is used for all open forms of the application.
        //
        // 返回结果:
        //     true is the wait cursor is used for all open forms; otherwise, false.
        public static bool UseWaitCursor { get { return System.Windows.Forms.Application.UseWaitCursor; }
            set { System.Windows.Forms.Application.UseWaitCursor = value; } }
        //
        // 摘要:
        //     Gets a value that specifies how visual styles are applied to application windows.
        //
        // 返回结果:
        //     A bitwise combination of the System.Windows.Forms.VisualStyles.VisualStyleState
        //     values.
        public static VisualStyleState VisualStyleState { get {
                return System.Windows.Forms.Application.VisualStyleState;
            } set {
                System.Windows.Forms.Application.VisualStyleState = value;
            } }

        //
        // 摘要:
        //     Occurs when the application is about to shut down.
        public static event EventHandler ApplicationExit;
        #region
        //
        // 摘要:
        //     Occurs when the application is about to enter a modal state.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static event EventHandler EnterThreadModal;
       
        //
        // 摘要:
        //     Occurs when the application finishes processing and is about to enter the idle
        //     state.
        public static event EventHandler Idle;
        //
        // 摘要:
        //     Occurs when the application is about to leave a modal state.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static event EventHandler LeaveThreadModal;
        //
        // 摘要:
        //     Occurs when an untrapped thread exception is thrown.
        public static event ThreadExceptionEventHandler ThreadException;
        //
        // 摘要:
        //     Occurs when a thread is about to shut down. When the main thread for an application
        //     is about to be shut down, this event is raised first, followed by an System.Windows.Forms.Application.ApplicationExit
        //     event.
        public static event EventHandler ThreadExit;
        #endregion
        
        //
        // 摘要:
        //     Adds a message filter to monitor Windows messages as they are routed to their
        //     destinations.
        //
        // 参数:
        //   value:
        //     The implementation of the System.Windows.Forms.IMessageFilter interface you want
        //     to install.
        public static void AddMessageFilter(IMessageFilter value)
        {
            System.Windows.Forms.Application.AddMessageFilter(value);
        }
        //
        // 摘要:
        //     Processes all Windows messages currently in the message queue.
        public static void DoEvents() { System.Windows.Forms.Application.DoEvents(); }
        //
        // 摘要:
        //     Enables visual styles for the application.
        public static void EnableVisualStyles() { System.Windows.Forms.Application.EnableVisualStyles(); }
        //
        // 摘要:
        //     Informs all message pumps that they must terminate, and then closes all application
        //     windows after the messages have been processed.
        public static void Exit() { System.Windows.Forms.Application.Exit(); }
        //
        // 摘要:
        //     Informs all message pumps that they must terminate, and then closes all application
        //     windows after the messages have been processed.
        //
        // 参数:
        //   e:
        //     Returns whether any System.Windows.Forms.Form within the application cancelled
        //     the exit.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Exit(CancelEventArgs e)
        { System.Windows.Forms.Application.Exit(e); }
        //
        // 摘要:
        //     Exits the message loop on the current thread and closes all windows on the thread.
        public static void ExitThread()
        {
            System.Windows.Forms.Application.ExitThread();
        }
        //
        // 摘要:
        //     Runs any filters against a window message, and returns a copy of the modified
        //     message.
        //
        // 参数:
        //   message:
        //     The Windows event message to filter.
        //
        // 返回结果:
        //     True if the filters were processed; otherwise, false.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static bool FilterMessage(ref Message message)
        { return System.Windows.Forms.Application.FilterMessage(ref message); }
        //
        // 摘要:
        //     Initializes OLE on the current thread.
        //
        // 返回结果:
        //     One of the System.Threading.ApartmentState values.
        public static ApartmentState OleRequired()
        {return System.Windows.Forms.Application.OleRequired(); }
        //
        // 摘要:
        //     Raises the System.Windows.Forms.Application.ThreadException event.
        //
        // 参数:
        //   t:
        //     An System.Exception that represents the exception that was thrown.
        public static void OnThreadException(Exception t)
        { System.Windows.Forms.Application.OnThreadException(t); }
        //
        // 摘要:
        //     Raises the System.Windows.Forms.Application.Idle event in hosted scenarios.
        //
        // 参数:
        //   e:
        //     The System.EventArgs objects to pass to the System.Windows.Forms.Application.Idle
        //     event.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RaiseIdle(EventArgs e)
        { System.Windows.Forms.Application.RaiseIdle(e); }
        //
        // 摘要:
        //     Registers a callback for checking whether the message loop is running in hosted
        //     environments.
        //
        // 参数:
        //   callback:
        //     The method to call when Windows Forms needs to check if the hosting environment
        //     is still sending messages.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RegisterMessageLoop(System.Windows.Forms.Application.MessageLoopCallback callback)
        {
            System.Windows.Forms.Application.RegisterMessageLoop(callback);
        }
        //
        // 摘要:
        //     Removes a message filter from the message pump of the application.
        //
        // 参数:
        //   value:
        //     The implementation of the System.Windows.Forms.IMessageFilter to remove from
        //     the application.
        public static void RemoveMessageFilter(IMessageFilter value)
        {
            System.Windows.Forms.Application.RemoveMessageFilter(value);
        }
        //
        // 摘要:
        //     Shuts down the application and starts a new instance immediately.
        //
        // 异常:
        //   T:System.NotSupportedException:
        //     Your code is not a Windows Forms application. You cannot call this method in
        //     this context.
        public static void Restart()
        { System.Windows.Forms.Application.Restart(); }
        //
        // 摘要:
        //     Begins running a standard application message loop on the current thread, without
        //     a form.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     A main message loop is already running on this thread.
        public static void Run()
        { System.Windows.Forms.Application.Run(); }

        //
        // 摘要:
        //     Begins running a standard application message loop on the current thread, with
        //     an System.Windows.Forms.ApplicationContext.
        //
        // 参数:
        //   context:
        //     An System.Windows.Forms.ApplicationContext in which the application is run.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     A main message loop is already running on this thread.
        public static void Run(ApplicationContext context)
        {
            System.Windows.Forms.Application.Run(context);
        }
        //
        // 摘要:
        //     Begins running a standard application message loop on the current thread, and
        //     makes the specified form visible.
        //
        // 参数:
        //   mainForm:
        //     A System.Windows.Forms.Form that represents the form to make visible.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     A main message loop is already running on the current thread.
        public static void Run(Form mainForm)
        {
            System.Windows.Forms.Application.Run(mainForm);
        }
        //
        // 摘要:
        //     Sets the application-wide default for the UseCompatibleTextRendering property
        //     defined on certain controls.
        //
        // 参数:
        //   defaultValue:
        //     The default value to use for new controls. If true, new controls that support
        //     UseCompatibleTextRendering use the GDI+ based System.Drawing.Graphics class for
        //     text rendering; if false, new controls use the GDI based System.Windows.Forms.TextRenderer
        //     class.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     You can only call this method before the first window is created by your Windows
        //     Forms application.
        public static void SetCompatibleTextRenderingDefault(bool defaultValue)
        {
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(defaultValue);
        }
        //
        // 摘要:
        //     Suspends or hibernates the system, or requests that the system be suspended or
        //     hibernated.
        //
        // 参数:
        //   state:
        //     A System.Windows.Forms.PowerState indicating the power activity mode to which
        //     to transition.
        //
        //   force:
        //     true to force the suspended mode immediately; false to cause Windows to send
        //     a suspend request to every application.
        //
        //   disableWakeEvent:
        //     true to disable restoring the system's power status to active on a wake event,
        //     false to enable restoring the system's power status to active on a wake event.
        //
        // 返回结果:
        //     true if the system is being suspended, otherwise, false.
        public static bool SetSuspendState(PowerState state, bool force, bool disableWakeEvent)
        {
           return System.Windows.Forms.Application.SetSuspendState(state, force, disableWakeEvent);
        }
        //
        // 摘要:
        //     Instructs the application how to respond to unhandled exceptions.
        //
        // 参数:
        //   mode:
        //     An System.Windows.Forms.UnhandledExceptionMode value describing how the application
        //     should behave if an exception is thrown without being caught.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     You cannot set the exception mode after the application has created its first
        //     window.
        public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode)
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(mode);
        }
        //
        // 摘要:
        //     Instructs the application how to respond to unhandled exceptions, optionally
        //     applying thread-specific behavior.
        //
        // 参数:
        //   mode:
        //     An System.Windows.Forms.UnhandledExceptionMode value describing how the application
        //     should behave if an exception is thrown without being caught.
        //
        //   threadScope:
        //     true to set the thread exception mode; otherwise, false.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     You cannot set the exception mode after the application has created its first
        //     window.
        public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode, bool threadScope)
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(mode,threadScope);
        }
        //
        // 摘要:
        //     Unregisters the message loop callback made with System.Windows.Forms.Application.RegisterMessageLoop(System.Windows.Forms.Application.MessageLoopCallback).
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void UnregisterMessageLoop()
        {
           System.Windows.Forms.Application.UnregisterMessageLoop();
        }
        /// <summary>
        /// Write log
        /// </summary>
        /// <param name="msg"></param>
        public static void log(string msg)
        {
            string path = Application.StartupPath + @"\log.txt";
            String strbuilder = "";
            strbuilder += DateTime.Now.ToString("yyyy-MM-dd  hh:mm:ss");
            strbuilder += "  ";
            strbuilder += msg;
            var stream = File.AppendText(path);
            stream.Write(strbuilder);
            stream.WriteLine();
            stream.Flush();
            stream.Close();
        }
    }
}
