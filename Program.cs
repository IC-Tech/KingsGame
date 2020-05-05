using System;

namespace KingsGame
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern int MessageBox(int hWnd, String text, String caption, uint type);
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main()
        {
			//try
			//{
				using (var game = new KingsGame())
					game.Run();
			//} catch (System.Exception e)
			//{
			//	MessageBox(0, e.Message, "Error", 0x00000010);
			//	return;
			//}
        }
    }
#endif
}
