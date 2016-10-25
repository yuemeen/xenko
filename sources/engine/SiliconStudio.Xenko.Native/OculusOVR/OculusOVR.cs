﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Native
{
    public static class OculusOvr
    {
        static OculusOvr()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
#endif
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrStartup", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Startup();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrShutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCreateSessionDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSessionDx(out long adapterLuidStr);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrDestroySession", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroySession(IntPtr outSessionPtr);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCreateTexturesDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateTexturesDx(IntPtr session, IntPtr dxDevice, out int outTextureCount, float pixelPerScreenPixel, int mirrorBufferWidth = 0, int mirrorBufferHeight = 0);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCreateQuadLayerTexturesDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateQuadLayerTexturesDx(IntPtr session, IntPtr dxDevice, out int outTextureCount, int width, int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrSetQuadLayerParams", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetQuadLayerParams(IntPtr layer, ref Vector3 position, ref Quaternion rotation, ref Vector2 size, bool headLocked);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetTextureAtIndexDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetTextureDx(IntPtr session, Guid textureGuid, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetQuadLayerTextureAtIndexDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetQuadLayerTextureDx(IntPtr session, IntPtr layer, Guid textureGuid, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetMirrorTextureDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMirrorTexture(IntPtr session, Guid textureGuid);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetCurrentTargetIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentTargetIndex(IntPtr session);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetCurrentQuadLayerTargetIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentQuadLayerTargetIndex(IntPtr session, IntPtr layer);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCommitFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CommitFrame(IntPtr session, IntPtr[] extraLayer, int numberOfExtraLayers);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct FrameProperties
        {
            public float Near;
            public float Far;
            public Matrix ProjLeft;
            public Matrix ProjRight;
            public Vector3 PosLeft;
            public Vector3 PosRight;
            public Quaternion RotLeft;
            public Quaternion RotRight;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrPrepareRender", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void PrepareRender(IntPtr session, FrameProperties* properties);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetError(IntPtr errorString);

        public static unsafe string GetError()
        {
            var buffer = stackalloc char[256];
            var errorCStr = new IntPtr(buffer);
            var error = GetError(errorCStr);
            var errorStr = Marshal.PtrToStringAnsi(errorCStr);
            return $"OculusOVR-Error({error}): {errorStr}";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct SessionStatusInternal
        {
            public int IsVisible;
            public int HmdPresent;
            public int HmdMounted;
            public int DisplayLost;
            public int ShouldQuit;
            public int ShouldRecenter;
        }

        public struct SessionStatus
        {
            /// <summary>
            /// True if the process has VR focus and thus is visible in the HMD.
            /// </summary>
            public bool IsVisible;
            /// <summary>
            /// True if an HMD is present.
            /// </summary>
            public bool HmdPresent;
            /// <summary>
            /// True if the HMD is on the user's head.
            /// </summary>
            public bool HmdMounted;
            /// <summary>
            /// True if the session is in a display-lost state. See ovr_SubmitFrame.
            /// </summary>
            public bool DisplayLost;
            /// <summary>
            /// True if the application should initiate shutdown.    
            /// </summary>
            public bool ShouldQuit;
            /// <summary>
            /// True if UX has requested re-centering. 
            /// </summary>
            public bool ShouldRecenter;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetStatus", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetStatus(IntPtr session, ref SessionStatusInternal status);

        public static SessionStatus GetStatus(IntPtr session)
        {
            var statusInternal = new SessionStatusInternal { DisplayLost = 0, IsVisible = 0, ShouldQuit = 0, HmdMounted = 0, HmdPresent = 0, ShouldRecenter = 0 };
            GetStatus(session, ref statusInternal);
            return new SessionStatus
            {
                DisplayLost = statusInternal.DisplayLost == 1,
                HmdMounted = statusInternal.HmdMounted == 1,
                HmdPresent = statusInternal.HmdPresent == 1,
                IsVisible = statusInternal.IsVisible == 1,
                ShouldQuit = statusInternal.ShouldQuit == 1,
                ShouldRecenter = statusInternal.ShouldRecenter == 1
            };
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrRecenter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Recenter(IntPtr session);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetAudioDeviceID", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void GetAudioDeviceID(StringBuilder deviceName);
    }
}
