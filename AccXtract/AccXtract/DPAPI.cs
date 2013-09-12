using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

public class DPAPI
{
    [DllImport("crypt32.dll",
                SetLastError = true,
                CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern
        bool CryptProtectData(ref DATA_BLOB pPlainText,
                                    string szDescription,
                                ref DATA_BLOB pEntropy,
                                    IntPtr pReserved,
                                ref CRYPTPROTECT_PROMPTSTRUCT pPrompt,
                                    int dwFlags,
                                ref DATA_BLOB pCipherText);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct DATA_BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CRYPTPROTECT_PROMPTSTRUCT
    {
        public int cbSize;
        public int dwPromptFlags;
        public IntPtr hwndApp;
        public string szPrompt;
    }

    public static byte[] encryptBytes(byte[] input)
    {

        DATA_BLOB encryptedBlob = new DATA_BLOB();
        DATA_BLOB decryptedBlob = new DATA_BLOB();

        decryptedBlob.pbData = Marshal.AllocHGlobal(input.Length);
        decryptedBlob.cbData = input.Length;
        Marshal.Copy(input, 0, decryptedBlob.pbData, input.Length);

        DATA_BLOB b = new DATA_BLOB();
        CRYPTPROTECT_PROMPTSTRUCT d = new CRYPTPROTECT_PROMPTSTRUCT();
        CryptProtectData(ref decryptedBlob, @"", ref b, ((IntPtr)((int)(0))), ref d, 0, ref encryptedBlob);

        byte[] encryptedBytes = new byte[encryptedBlob.cbData];

        Marshal.Copy(encryptedBlob.pbData,
                         encryptedBytes,
                         0,
                         encryptedBlob.cbData);

        return encryptedBytes;
    }
}