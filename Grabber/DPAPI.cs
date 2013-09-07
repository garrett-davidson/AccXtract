using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

public class DPAPI
{

    [DllImport( "crypt32.dll",
                SetLastError=true,
                CharSet=System.Runtime.InteropServices.CharSet.Auto)]
    private static extern
        bool CryptUnprotectData(ref DATA_BLOB       pCipherText,
                                ref string          pszDescription,
                                ref DATA_BLOB       pEntropy,
                                    IntPtr          pReserved,
                                ref CRYPTPROTECT_PROMPTSTRUCT pPrompt,
                                    int             dwFlags,
                                ref DATA_BLOB       pPlainText);

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct DATA_BLOB
    {
        public int     cbData;
        public IntPtr  pbData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct CRYPTPROTECT_PROMPTSTRUCT
    {
        public int      cbSize;
        public int      dwPromptFlags;
        public IntPtr   hwndApp;
        public string   szPrompt;
    }

    public static byte[] decryptBytes(byte[] input)
    {

        DATA_BLOB encryptedBlob = new DATA_BLOB();
        DATA_BLOB decryptedBlob = new DATA_BLOB();

        encryptedBlob.pbData = Marshal.AllocHGlobal(input.Length);
        encryptedBlob.cbData = input.Length;
        Marshal.Copy(input, 0, encryptedBlob.pbData, input.Length);

        string a = "";
        DATA_BLOB b = new DATA_BLOB();
        CRYPTPROTECT_PROMPTSTRUCT d = new CRYPTPROTECT_PROMPTSTRUCT();
        CryptUnprotectData(ref encryptedBlob, ref a, ref b, ((IntPtr)((int)(0))), ref d, 0, ref decryptedBlob);

        byte[] plainTextBytes = new byte[decryptedBlob.cbData];

        Marshal.Copy(decryptedBlob.pbData,
                         plainTextBytes,
                         0,
                         decryptedBlob.cbData);

        return plainTextBytes;
    }
}