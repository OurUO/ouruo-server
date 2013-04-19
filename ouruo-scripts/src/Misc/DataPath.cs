using System;
using System.IO;
using Microsoft.Win32;
using Server;

namespace Server.Misc
{
	public class DataPath
	{
		/* If you have not installed Ultima Online,
		 * or wish the server to use a separate set of datafiles,
		 * change the 'CustomPath' value.
		 * Example:
		 *  private static string CustomPath = @"C:\Program Files\Ultima Online";
		 */
		private static string CustomPath = @"./mul";

		/* The following is a list of files which a required for proper execution:
		 * 
		 * Multi.idx
		 * Multi.mul
		 * VerData.mul
		 * TileData.mul
		 * Map*.mul or Map*LegacyMUL.uop
		 * StaIdx*.mul
		 * Statics*.mul
		 * MapDif*.mul
		 * MapDifL*.mul
		 * StaDif*.mul
		 * StaDifL*.mul
		 * StaDifI*.mul
		 */

		public static void Configure()
		{
			string pathUO = GetPath( @"Origin Worlds Online\Ultima Online\1.0", "ExePath" );
			string pathTD = GetPath( @"Origin Worlds Online\Ultima Online Third Dawn\1.0", "ExePath" ); //These refer to 2D & 3D, not the Third Dawn expansion
			string pathKR = GetPath( @"Origin Worlds Online\Ultima Online\KR Legacy Beta", "ExePath" ); //After KR, This is the new registry key for the 2D client
			string pathSA = GetPath( @"Electronic Arts\EA Games\Ultima Online Stygian Abyss Classic", "InstallDir" );
			string pathHS = GetPath( @"Electronic Arts\EA Games\Ultima Online Classic", "InstallDir" );

            if (CustomPath != null)
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Selecting custom path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(CustomPath);

                return;
            }

			if ( pathUO != null ) 
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Autodetected Ultima Online 1.0 path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(pathUO);

                return;
            }

			if ( pathTD != null )
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Autodetected Ultima Online Third Dawn path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(pathTD);

                return;
            }

			if ( pathKR != null )
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Autodetected Ultima Online Kingdom Reborn path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(pathKR);

                return;
            }

			if ( pathSA != null )
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Autodetected Ultima Online Stygian Abyss path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(pathSA);

                return;
            }

			if ( pathHS != null )
            {
                Utility.PushColor(ConsoleColor.Cyan);
                Console.WriteLine("Autodetected Ultima Online High Seas path for mulfiles.");
                Utility.PopColor();
                Core.DataDirectories.Add(pathHS);

                return;
            }

			if ( Core.DataDirectories.Count == 0 && !Core.Service )
			{
				Console.WriteLine( "Enter the Ultima Online directory:" );
				Console.Write( "> " );

				Core.DataDirectories.Add( Console.ReadLine() );

                return;
			}
		}

		private static string GetPath( string subName, string keyName )
		{
			try
			{
				string keyString;

				if( Core.Is64Bit )
					keyString = @"SOFTWARE\Wow6432Node\{0}";
				else
					keyString = @"SOFTWARE\{0}";

				using( RegistryKey key = Registry.LocalMachine.OpenSubKey( String.Format( keyString, subName ) ) )
				{
					if( key == null )
						return null;

					string v = key.GetValue( keyName ) as string;

					if( String.IsNullOrEmpty( v ) )
						return null;

					if ( keyName == "InstallDir" )
						v = v + @"\";

					v = Path.GetDirectoryName( v );

					if ( String.IsNullOrEmpty( v ) )
						return null;

					return v;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}