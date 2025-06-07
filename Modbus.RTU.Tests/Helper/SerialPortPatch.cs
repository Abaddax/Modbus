using HarmonyLib;
using System.IO.Ports;

namespace Abaddax.Modbus.RTU.Tests.Helper
{
    internal static class SerialPortPatch
    {
        private static Harmony _harmony = new Harmony("com.Abaddax.Modbus.RTU.Tests");

        #region Patches
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.IsOpen))]
        [HarmonyPatch(MethodType.Getter)]
        private class SerialPort_getIsOpen_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance, ref bool __result)
            {
#if DEBUG
                FileLog.Log("get_IsOpen called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                __result = mock.IsOpen;
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.BytesToRead))]
        [HarmonyPatch(MethodType.Getter)]
        private class SerialPort_getBytesToRead_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance, ref int __result)
            {
#if DEBUG
                FileLog.Log("get_BytesToRead called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                __result = mock.BytesToRead;
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.BytesToWrite))]
        [HarmonyPatch(MethodType.Getter)]
        private class SerialPort_getBytesToWrite_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance, ref int __result)
            {
#if DEBUG
                FileLog.Log("get_BytesToWrite called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                __result = mock.BytesToWrite;
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.Open))]
        [HarmonyPatch(MethodType.Normal)]
        private class SerialPort_Open_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance)
            {
#if DEBUG
                FileLog.Log("Open called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.Open();
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.Read))]
        [HarmonyPatch(MethodType.Normal, typeof(byte[]), typeof(int), typeof(int))]
        private class SerialPort_Read_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance, object[] __args, ref int __result)
            {
#if DEBUG
                FileLog.Log("Read called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                __result = mock.Read((byte[])__args[0], (int)__args[1], (int)__args[2]);
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.Write))]
        [HarmonyPatch(MethodType.Normal, typeof(byte[]), typeof(int), typeof(int))]
        private class SerialPort_Write_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance, object[] __args)
            {
#if DEBUG
                FileLog.Log("Write called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.Write((byte[])__args[0], (int)__args[1], (int)__args[2]);
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.DiscardInBuffer))]
        [HarmonyPatch(MethodType.Normal)]
        private class SerialPort_DiscardInBuffer_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance)
            {
#if DEBUG
                FileLog.Log("DiscardInBuffer called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.DiscardInBuffer();
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.DiscardOutBuffer))]
        [HarmonyPatch(MethodType.Normal)]
        private class SerialPort_DiscardOutBuffer_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance)
            {
#if DEBUG
                FileLog.Log("DiscardOutBuffer called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.DiscardOutBuffer();
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.Close))]
        [HarmonyPatch(MethodType.Normal)]
        private class SerialPort_Close_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance)
            {
#if DEBUG
                FileLog.Log("Close called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.Close();
                return false;
            }
        }
        [HarmonyPatch(typeof(SerialPort))]
        [HarmonyPatch(nameof(SerialPort.Dispose))]
        [HarmonyPatch(MethodType.Normal, typeof(bool))]
        private class SerialPort_Dispose_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(SerialPort __instance)
            {
#if DEBUG
                FileLog.Log("Close called!");
#endif
                var mock = SerialPortMock.GetMock(__instance);
                mock.Dispose();
                return true;
            }
        }
        #endregion

        public static void Apply()
        {
#if DEBUG
            Harmony.DEBUG = true;
#endif
            _harmony.PatchAll();
        }
        public static void Remove()
        {
#if DEBUG
            Harmony.DEBUG = false;
#endif
            _harmony.UnpatchAll();
        }
    }
}
