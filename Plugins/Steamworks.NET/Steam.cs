// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#define VERSION_SAFE_STEAM_API_INTERFACES

using System.Runtime.InteropServices;

namespace Steamworks {
	public static class Version {
		public const string SteamworksNETVersion = "6.0.0";
		public const string SteamworksSDKVersion = "1.32";
		public const string SteamAPIDLLVersion = "02.59.51.43";
		public const int SteamAPIDLLSize = 187584;
		public const int SteamAPI64DLLSize = 208296;
	}

	public static class SteamAPI {
		private static bool _initialized = false;

		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	Steam API setup & shutdown
		//
		//	These functions manage loading, initializing and shutdown of the steamclient.dll
		//
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//

		// Detects if your executable was launched through the Steam client, and restarts your game through 
		// the client if necessary. The Steam client will be started if it is not running.
		//
		// Returns: true if your executable was NOT launched through the Steam client. This function will
		//          then start your application through the client. Your current process should exit.
		//
		//          false if your executable was started through the Steam client or a steam_appid.txt file
		//          is present in your game's directory (for development). Your current process should continue.
		//
		// NOTE: This function should be used only if you are using CEG or not using Steam's DRM. Once applied
		//       to your executable, Steam's DRM will handle restarting through Steam if necessary.
		public static bool RestartAppIfNecessary(AppId_t unOwnAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
		}

#if VERSION_SAFE_STEAM_API_INTERFACES
		public static bool InitSafe() {
			return Init();
		}

		// [Steamworks.NET] This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init() {
			if (_initialized) {
				throw new System.Exception("Tried to Initialize Steamworks twice in one session!");
			}

			InteropHelp.TestIfPlatformSupported();
			_initialized = NativeMethods.SteamAPI_InitSafe();
			return _initialized;
		}
#else
		public static bool Init() {
			if (_initialized) {
				throw new System.Exception("Tried to Initialize Steamworks twice in one session!");
			}

			InteropHelp.TestIfPlatformSupported();
			_initialized = NativeMethods.SteamAPI_Init();
			return _initialized;
		}
#endif

		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_Shutdown();
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	steam callback helper functions
		//
		//	The following classes/macros are used to be able to easily multiplex callbacks 
		//	from the Steam API into various objects in the app in a thread-safe manner
		//
		//	These functors are triggered via the SteamAPI_RunCallbacks() function, mapping the callback
		//  to as many functions/objects as are registered to it
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_RunCallbacks();
		}

		// checks if a local Steam client is running
		public static bool IsSteamRunning() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_IsSteamRunning();
		}

		// returns the HSteamUser of the last user to dispatch a callback
		public static HSteamUser GetHSteamUserCurrent() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.Steam_GetHSteamUserCurrent();
		}
		
		// returns the pipe we are communicating to Steam with
		public static HSteamPipe GetHSteamPipe() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamPipe)NativeMethods.SteamAPI_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.SteamAPI_GetHSteamUser();
		}
	}

	public static class GameServer {
		// Initialize ISteamGameServer interface object, and set server properties which may not be changed.
		//
		// After calling this function, you should set any additional server parameters, and then
		// call ISteamGameServer::LogOnAnonymous() or ISteamGameServer::LogOn()
		//
		// - usSteamPort is the local port used to communicate with the steam servers.
		// - usGamePort is the port that clients will connect to for gameplay.
		// - usQueryPort is the port that will manage server browser related duties and info
		//		pings from clients.  If you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE for usQueryPort, then it
		//		will use "GameSocketShare" mode, which means that the game is responsible for sending and receiving
		//		UDP packets for the master  server updater. See references to GameSocketShare in isteamgameserver.h.
		// - The version string is usually in the form x.x.x.x, and is used by the master server to detect when the
		//		server is out of date.  (Only servers with the latest version will be listed.)
#if VERSION_SAFE_STEAM_API_INTERFACES
		public static bool InitSafe(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using (var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
			}
		}

		// [Steamworks.NET] This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using (var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
			}
		}
#else
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using (var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_Init(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
		`	}
		}
#endif
		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_Shutdown();
		}

		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_RunCallbacks();
		}

		public static bool BSecure() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamGameServer_BSecure();
		}

		public static CSteamID GetSteamID() {
			InteropHelp.TestIfPlatformSupported();
			return (CSteamID)NativeMethods.SteamGameServer_GetSteamID();
		}

		public static HSteamPipe GetHSteamPipe() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamPipe)NativeMethods.SteamGameServer_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.SteamGameServer_GetHSteamUser();
		}
	}

	public static class SteamEncryptedAppTicket {
		public static bool BDecryptTicket(byte[] rgubTicketEncrypted, uint cubTicketEncrypted, byte[] rgubTicketDecrypted, ref uint pcubTicketDecrypted, byte[] rgubKey, int cubKey) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BDecryptTicket(rgubTicketEncrypted, cubTicketEncrypted, rgubTicketDecrypted, ref pcubTicketDecrypted, rgubKey, cubKey);
		}

		public static bool BIsTicketForApp(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BIsTicketForApp(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static uint GetTicketIssueTime(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.GetTicketIssueTime(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static void GetTicketSteamID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out CSteamID psteamID) {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.GetTicketSteamID(rgubTicketDecrypted, cubTicketDecrypted, out psteamID);
		}

		public static uint GetTicketAppID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.GetTicketAppID(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static bool BUserOwnsAppInTicket(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BUserOwnsAppInTicket(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static bool BUserIsVacBanned(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BUserIsVacBanned(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static byte[] GetUserVariableData(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out uint pcubUserData) {
			InteropHelp.TestIfPlatformSupported();
			System.IntPtr punSecretData = NativeMethods.GetUserVariableData(rgubTicketDecrypted, cubTicketDecrypted, out pcubUserData);
			byte[] ret = new byte[pcubUserData];
			System.Runtime.InteropServices.Marshal.Copy(punSecretData, ret, 0, (int)pcubUserData);
			return ret;
		}
	}

	//-----------------------------------------------------------------------------
	// Purpose: Functions for sending and receiving messages from the Game Coordinator
	//			for this application
	//-----------------------------------------------------------------------------
	public class SteamGameCoordinator {
		private enum Functions {
			SendMessage,
			IsMessageAvailable,
			RetrieveMessage,
		}

		private static System.IntPtr s_SteamGameCoordinator;

		private static void InitSteamGameCoordinator() {
			InteropHelp.TestIfPlatformSupported();
			HSteamPipe pipe = SteamAPI.GetHSteamPipe();
			HSteamUser user = SteamAPI.GetHSteamUser();
			s_SteamGameCoordinator = SteamClient.GetISteamGenericInterface(user, pipe, "SteamGameCoordinator001");

			System.IntPtr VTablePtr = Marshal.ReadIntPtr(s_SteamGameCoordinator);

			_SendMessage = (NativeSendMessage)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(VTablePtr, (int)Functions.SendMessage * System.IntPtr.Size), typeof(NativeSendMessage));
			_IsMessageAvailable = (NativeIsMessageAvailable)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(VTablePtr, (int)Functions.IsMessageAvailable * System.IntPtr.Size), typeof(NativeIsMessageAvailable));
			_RetrieveMessage = (NativeRetrieveMessage)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(VTablePtr, (int)Functions.RetrieveMessage * System.IntPtr.Size), typeof(NativeRetrieveMessage));
		}

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		private delegate EGCResults NativeSendMessage(System.IntPtr thisptr, uint unMsgType, System.IntPtr pubData, uint cubData);
		private static NativeSendMessage _SendMessage;
		/// <summary>
		/// <para>sends a message to the Game Coordinator</para>
		/// </summary>
		public static EGCResults SendMessage(uint unMsgType, System.IntPtr pubData, uint cubData) {
			if (s_SteamGameCoordinator == System.IntPtr.Zero) { InitSteamGameCoordinator(); }
			return _SendMessage(s_SteamGameCoordinator, unMsgType, pubData, cubData);
		}

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		[return: MarshalAs(UnmanagedType.I1)]
		private delegate bool NativeIsMessageAvailable(System.IntPtr thisptr, out uint unMsgType);
		private static NativeIsMessageAvailable _IsMessageAvailable;
		/// <summary>
		/// <para>returns true if there is a message waiting from the game coordinator</para>
		/// </summary>
		public static bool IsMessageAvailable(out uint pcubMsgSize) {
			if (s_SteamGameCoordinator == System.IntPtr.Zero) { InitSteamGameCoordinator(); }
			return _IsMessageAvailable(s_SteamGameCoordinator, out pcubMsgSize);
		}

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		private delegate EGCResults NativeRetrieveMessage(System.IntPtr thisptr, out uint unMsgType, byte[] pubDest, uint cubDest, out uint pcubMsgSize);
		private static NativeRetrieveMessage _RetrieveMessage;
		/// <summary>
		/// <para>fills the provided buffer with the first message in the queue and returns k_EGCResultOK or</para>
		/// <para>returns k_EGCResultNoMessage if there is no message waiting. pcubMsgSize is filled with the message size.</para>
		/// <para>If the provided buffer is not large enough to fit the entire message, k_EGCResultBufferTooSmall is returned</para>
		/// <para>and the message remains at the head of the queue.</para>
		/// </summary>
		public static EGCResults RetrieveMessage(out uint unMsgType, byte[] pubDest, uint cubDest, out uint pcubMsgSize) {
			if (s_SteamGameCoordinator == System.IntPtr.Zero) { InitSteamGameCoordinator(); }
			return _RetrieveMessage(s_SteamGameCoordinator, out unMsgType, pubDest, cubDest, out pcubMsgSize);
		}
	}

	//-----------------------------------------------------------------------------
	// Purpose: hand out a reasonable "future proof" view of an app ownership ticket
	// the raw (signed) buffer, and indices into that buffer where the appid and 
	// steamid are located.  the sizes of the appid and steamid are implicit in 
	// (each version of) the interface - currently uin32 appid and uint64 steamid
	//-----------------------------------------------------------------------------
	public class SteamAppTicket {
		private enum Functions {
			GetAppOwnershipTicketData,
		}

		private static System.IntPtr s_SteamAppTicket;

		private static void InitSteamAppTicket() {
			InteropHelp.TestIfPlatformSupported();
			HSteamPipe pipe = SteamAPI.GetHSteamPipe();
			HSteamUser user = SteamAPI.GetHSteamUser();
			s_SteamAppTicket = SteamClient.GetISteamGenericInterface(user, pipe, "STEAMAPPTICKET_INTERFACE_VERSION001");

			System.IntPtr VTablePtr = Marshal.ReadIntPtr(s_SteamAppTicket);

			_GetAppOwnershipTicketData = (NativeGetAppOwnershipTicketData)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(VTablePtr, (int)Functions.GetAppOwnershipTicketData * System.IntPtr.Size), typeof(NativeGetAppOwnershipTicketData));
		}

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		private delegate uint NativeGetAppOwnershipTicketData(System.IntPtr thisptr, uint nAppID, byte[] pvBuffer, uint cbBufferLength, out uint piAppId, out uint piSteamId, out uint piSignature, out uint pcbSignature);
		private static NativeGetAppOwnershipTicketData _GetAppOwnershipTicketData;
		public static uint GetAppOwnershipTicketData(uint nAppID, byte[] pvBuffer, uint cbBufferLength, out uint piAppId, out uint piSteamId, out uint piSignature, out uint pcbSignature) {
			if (s_SteamAppTicket == System.IntPtr.Zero) { InitSteamAppTicket(); }
			return _GetAppOwnershipTicketData(s_SteamAppTicket, nAppID, pvBuffer, cbBufferLength, out piAppId, out piSteamId, out piSignature, out pcbSignature);
		}
	}
}
