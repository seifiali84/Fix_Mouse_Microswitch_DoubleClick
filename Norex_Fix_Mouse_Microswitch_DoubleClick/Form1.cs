using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Norex_Fix_Mouse_Microswitch_DoubleClick
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // define enums 
        public enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
        }

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        // Define the callback delegate for handling low-level mouse input events.
        private static LowLevelMouseProc _proc = HookCallback;

        // Declare a global variable to store the time of the last left click event.
        private static long _lastLeftClickTimeTicks = 0;

        // This method removes our low-level mouse hook from the system.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // This method calls the next hook procedure in line and passes it information about our intercepted message.
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        // This method retrieves a handle to the module that contains the specified address.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        // This method sets up a hook procedure that monitors low-level mouse input events.
        public static extern IntPtr SetWindowsHookEx(int idHook,
     LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        // This delegate describes a callback function for handling low-level mouse input events.
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        static private IntPtr HookCallback(
    int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct =
                    (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam,
                        typeof(MSLLHOOKSTRUCT));

                long currentTicks = DateTime.Now.Ticks;

                if ((currentTicks - _lastLeftClickTimeTicks) / TimeSpan.TicksPerMillisecond < 5)
                {
                    Console.WriteLine("Ignoring double-click event.");
                    return new System.IntPtr(1); // Return non-zero value to indicate we handled this message and it should not be passed on to other programs
                }

                _lastLeftClickTimeTicks = currentTicks;

                Console.WriteLine($"Received left click at ({hookStruct.pt.X}, {hookStruct.pt.Y})");
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Set up the hook procedure using SetWindowsHookEx().
            IntPtr hookId = SetHook(_proc);
            UnhookWindowsHookEx(hookId);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkBox1.Checked)
            {
                DialogResult result = MessageBox.Show("Are you sure to make this Program Startup?", "Confirmation", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes)
                {
                    rk.SetValue(Application.ProductName, Application.ExecutablePath);
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("do you need to Disable Startup this Program?" , "Confirmation" , MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes)
                {
                    rk.DeleteValue(Application.ProductName, false);
                }
                else
                {
                    checkBox1.Checked = true;
                }
            }
        }
    }
}