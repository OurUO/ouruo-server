namespace Scripts.Engines.Encryption
{
    public class Configuration
    {
        // Set this to true to enable this subsystem.
        public static bool Enabled = true;

        // Set this to false to disconnect unencrypted connections.
        public static bool AllowUnencryptedClients = false;

        public static LoginKey[] LoginKeys = new LoginKey[]
		{
            #region Login Keys Accepted

            new LoginKey("7.0.23.1", 0x2A9F868D, 0xA0437E7F),
            new LoginKey("7.0.10.3", 0x2DA36D5D, 0xA3C0A27F),

            #endregion 
		};
    }
}
