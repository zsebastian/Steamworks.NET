// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2014 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Steamworks {
	public interface ICallbackBase {
		void Run(IntPtr param);
	}

	public interface ICallResultBase {
		int GetCallbackSizeBytes();
		int GetICallback();
		void Run(IntPtr param, bool bIOFailure);
	}

	public sealed class Callback<T> : ICallbackBase {
		public delegate void DispatchDelegate(T param);
		public event DispatchDelegate m_Func;

		public Callback(DispatchDelegate myFunc) {
			if (myFunc == null)
				throw new Exception("Function must not be null.");

			this.m_Func += myFunc;
			CallbackDispatcher.RegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		~Callback() {
			UnRegister();
		}

		public void UnRegister() {
			CallbackDispatcher.UnRegisterCallback(this, CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		// ICallbackBase
		public void Run(IntPtr pubParam) {
			this.m_Func((T)Marshal.PtrToStructure(pubParam, typeof(T)));
		}
	}

	public class CallResult<T> : ICallResultBase {
		private SteamAPICall_t m_hAPICall = SteamAPICall_t.Invalid;
		private int m_iCallback;
		private int m_Size;

		public delegate void APIDispatchDelegate(T param, bool bIOFailure);
		public event APIDispatchDelegate m_Func;

		public CallResult() {
			m_iCallback = CallbackIdentities.GetCallbackIdentity(typeof(T));
			m_Size = Marshal.SizeOf(typeof(T));
		}

		public CallResult(APIDispatchDelegate myFunc)
			: this() {
			if (myFunc != null)
				m_Func = myFunc;
		}

		public CallResult(APIDispatchDelegate myFunc, SteamAPICall_t hAPICall)
			: this(myFunc) {
			Set(hAPICall);
		}

		~CallResult() {
			Cancel();
		}

		public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate newFunc = null) {
			if (newFunc != null) {
				m_Func = newFunc;
			}

			if (m_Func == null)
				throw new Exception("CallResult function was null, you must either set it in the CallResult Constructor or in Set()");

			if (m_hAPICall != SteamAPICall_t.Invalid)
				Cancel();

			m_hAPICall = hAPICall;

			if (hAPICall != SteamAPICall_t.Invalid)
				CallbackDispatcher.RegisterCallResult(this, hAPICall);
		}

		public bool IsActive() {
			return (m_hAPICall != SteamAPICall_t.Invalid);
		}

		public void Cancel() {
			if (m_hAPICall != SteamAPICall_t.Invalid) {
				CallbackDispatcher.UnregisterCallResult(this, m_hAPICall);
				m_hAPICall = SteamAPICall_t.Invalid;
			}
		}

		// ICallResultBase
		public int GetCallbackSizeBytes() {
			return m_Size;
		}

		public int GetICallback() {
			return m_iCallback;
		}

		public void Run(IntPtr pubParam, bool bIOFailure) {
			// We made sure that m_Func is not null in Set()
			m_hAPICall = SteamAPICall_t.Invalid; // Caller unregisters for us.
			this.m_Func((T)Marshal.PtrToStructure(pubParam, typeof(T)), bIOFailure);
		}
	}

	public class CallbackDispatcher {
		private static Dictionary<int, List<ICallbackBase>> m_RegisteredCallbacks = new Dictionary<int, List<ICallbackBase>>();
		private static Dictionary<SteamAPICall_t, List<ICallResultBase>> m_RegisteredCallResults = new Dictionary<SteamAPICall_t, List<ICallResultBase>>();

		public static Callback<SteamAPICallCompleted_t> m_APICallbackCompleted = new Callback<SteamAPICallCompleted_t>(OnSteamCallResult);

		delegate void OnCallbackFromSteamDel(int k_iCallback, IntPtr pvParam);

		public static void RegisterCallback(ICallbackBase callback, int iCallback) {
			List<ICallbackBase> callbackList;
			if (m_RegisteredCallbacks.TryGetValue(iCallback, out callbackList)) {
				callbackList.Add(callback);
			}
			else {
				callbackList = new List<ICallbackBase>();
				callbackList.Add(callback);
				m_RegisteredCallbacks.Add(iCallback, callbackList);
			}
		}

		public static void UnRegisterCallback(ICallbackBase callback, int iCallback) {
			List<ICallbackBase> callbackList;
			if (m_RegisteredCallbacks.TryGetValue(iCallback, out callbackList)) {
				foreach (ICallbackBase c in callbackList) {
					if (c == callback) {
						callbackList.Remove(c);
						break;
					}
				}

				if (callbackList.Count == 0) {
					m_RegisteredCallbacks.Remove(iCallback);
				}
			}
		}

		public static void RegisterCallResult(ICallResultBase callback, SteamAPICall_t hAPICall) {
			List<ICallResultBase> callResultList;
			if (m_RegisteredCallResults.TryGetValue(hAPICall, out callResultList)) {
				callResultList.Add(callback);
			}
			else {
				callResultList = new List<ICallResultBase>();
				callResultList.Add(callback);
				m_RegisteredCallResults.Add(hAPICall, callResultList);
			}
		}

		public static void UnregisterCallResult(ICallResultBase callback, SteamAPICall_t hAPICall) {
			List<ICallResultBase> callResultList;
			if (m_RegisteredCallResults.TryGetValue(hAPICall, out callResultList)) {
				foreach (ICallResultBase c in callResultList) {
					if (c == callback) {
						callResultList.Remove(c);
						break;
					}
				}

				if (callResultList.Count == 0) {
					m_RegisteredCallResults.Remove(hAPICall);
				}
			}
		}

		public static void RunCallbacks() {
			NativeMethods.CSteamworks_RunCallbacks(pfnSteamCallback);
		}

		public static void RunGameServerCallbacks() {
			NativeMethods.CSteamworks_RunGameServerCallbacks(pfnSteamGameServerCallback);
		}

		static OnCallbackFromSteamDel pfnSteamCallback = new OnCallbackFromSteamDel(OnSteamCallback);
		private static void OnSteamCallback(int iCallback, IntPtr pubParam) {
			List<ICallbackBase> callbackList;
			if (m_RegisteredCallbacks.TryGetValue(iCallback, out callbackList)) {
				foreach (ICallbackBase callback in callbackList) {
					callback.Run(pubParam);
				}
			}
		}

		static OnCallbackFromSteamDel pfnSteamGameServerCallback = new OnCallbackFromSteamDel(OnSteamGameServerCallback);
		private static void OnSteamGameServerCallback(int iCallback, IntPtr pubParam) {
			List<ICallbackBase> callbackList;
			if (m_RegisteredCallbacks.TryGetValue(iCallback, out callbackList)) {
				foreach (ICallbackBase callback in callbackList) {
					callback.Run(pubParam);
				}
			}
		}

		private static void OnSteamCallResult(SteamAPICallCompleted_t apicall) {
			List<ICallResultBase> callResultList;

			if (!m_RegisteredCallResults.TryGetValue(apicall.m_hAsyncCall, out callResultList)) {
				return;
			}

			if (callResultList.Count == 0) {
				// This should never ever happen? Jankyness.
				m_RegisteredCallResults.Remove(apicall.m_hAsyncCall);
				return;
			}

			IntPtr pData = IntPtr.Zero;
			bool bFailed;

			try {
				ICallResultBase apiCallback = callResultList[0];
				pData = Marshal.AllocHGlobal(apiCallback.GetCallbackSizeBytes());
				if (!SteamUtils.GetAPICallResult(apicall.m_hAsyncCall, pData, apiCallback.GetCallbackSizeBytes(), apiCallback.GetICallback(), out bFailed)) {
					// If your API CallResult is failing you could try examining the following.
					// ESteamAPICallFailure failureReason = SteamUtils.GetAPICallFailureReason(apicall.m_hAsyncCall);
					return;
				}

				foreach (ICallResultBase callresult in callResultList) {
					callresult.Run(pData, bFailed);
				}
			}
			finally {
				// Unregister all 
				m_RegisteredCallResults.Remove(apicall.m_hAsyncCall);

				Marshal.FreeHGlobal(pData);
			}
		}
	}
}
