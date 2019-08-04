using System;
using System.Collections.Generic;
using System.Text;

using NetCore8583;

namespace WebApi.Client.ISO8583
{
    public static class Class1
    {
        private static string ISO8583_BASE64 = @"AhkwMjAw8jxEgQCAgCAAAAAAAAAAIjE2NTQ5OTk5MDEyMzQ1Njc4MTAwMzAwMDAwMDAwMDAwMDAyODA4MDExODQ2MTAwMDAwNDUxMjQ2MTAwODAxMjAwNTU4MTIwMTEwMDA2MDIzNDAwMTQ2NjMgICA4NDAwMTIwMDAwMDEwMDAwMDEwMTUyMTExMDE2NTQwMDExMDEwMDAzODREAAQAAAAAADMyYzBmM2VlOTA1ODU1NDMzNzg2ZGE0MTdmZTFiNDNjNTcxMTAwMzM1MjE0QWRkaXRpb25hbERhdGExMTAyMTFFeHRlcm5hbFRJRDE4MTAxMzI2NjUyMThHbG9iYWxQT1NFbnRyeU1vZGUyMTIyMTAxMDA3MDAwMDAyMjBHbG9iYWxQcm9jZXNzaW5nQ29kZTE2MDAzMDAwMjE4R2xvYmFsVGVybWluYWxUeXBlMTNEQzIxNkxhbmVJRDEwMjEwTWFya2V0RGF0YTIxM2FGdDAwMDAwMDAwMDEyMThQdXJjaGFzaW5nQ2FyZERhdGEyOTA8P3htbCB2ZXJzaW9uPSIxLjAiIGVuY29kaW5nPSJ1dGYtOCI/PjxQdXJjaGFzaW5nQ2FyZERhdGE+PENvbnRhY3QgLz48L1B1cmNoYXNpbmdDYXJkRGF0YT4yMTdUcmFuc2FjdGlvblN0YXR1czExMDIxN1Zpc2FFQ29tbUdvb2RzSW5kMTA=";

        public static void GetIsoMsg()
        {
            MessageFactory<IsoMessage> mf = new MessageFactory<IsoMessage>();
            mf.Encoding = Encoding.UTF8;

            var iso8583_DecodedString = System.Convert.FromBase64String(ISO8583_BASE64);
            sbyte[] signed = (sbyte[])(Array)iso8583_DecodedString;

            IsoMessage isoMessage = mf.ParseMessage(signed, 0, true);
        }
    }
}
