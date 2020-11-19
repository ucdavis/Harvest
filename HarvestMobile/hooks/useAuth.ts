import * as React from 'react';
import { createState, useState } from '@hookstate/core';
import * as SecureStore from 'expo-secure-store';
import { AuthRequestPromptOptions, AuthSessionResult, makeRedirectUri, ResponseType, useAuthRequest, useAutoDiscovery, } from "expo-auth-session";
import { useAsyncCallback } from 'react-async-hook';
import Constants from 'expo-constants';
import { AUTH_AUTO_DISCOVERY_URL, AUTH_CLIENT_ID } from '@env';

type AuthState = {
  userToken: string | null;
  isLoading: boolean;
  isSignout: boolean;
  tryRestoreToken: () => void;
  promptSignIn: () => void;
  signOut: () => void;
};

async function signOut() {
  try {
    await SecureStore.deleteItemAsync('userToken');
  }
  catch (e) {

  }
  globalAuthState.isSignout.set(true);
  globalAuthState.userToken.set(null);
}

export const globalAuthState = createState<AuthState>({
  isLoading: true,
  isSignout: false,
  userToken: null,
  tryRestoreToken: () => { },
  promptSignIn: () => { },
  signOut: signOut
});

export function useAuth() {
  const authState = useState(globalAuthState);

  const setToken = useAsyncCallback(async (token: string) => {
    await SecureStore.setItemAsync('userToken', token);
    authState.isSignout.set(false);
    authState.userToken.set(token);
  });

  const discovery = useAutoDiscovery(
    AUTH_AUTO_DISCOVERY_URL
  );

  // Request
  const [isRequestingSignIn, signInResponse, promptSignIn] = useAuthRequest(
    {
      clientId: AUTH_CLIENT_ID,
      scopes: ["openid", "profile"],
      redirectUri: makeRedirectUri({
        useProxy: false, // don't use their proxy to standardize the redirect uris.
        // For usage in bare and standalone
        native: Constants.manifest.scheme + "://redirect",
      })
    },
    discovery
  );

  const tryRestoreToken = useAsyncCallback(async () => {
    let userToken: string | null = null;

    try {
      userToken = await SecureStore.getItemAsync('userToken');
    } catch (e) {
      // Restoring token failed
    }

    // After restoring token, we may need to validate it in production apps

    // This will switch to the App screen or Auth screen and this loading
    // screen will be unmounted and thrown away.
    authState.userToken.set(userToken);
    authState.isLoading.set(false);
  });

  React.useEffect(() => {
    authState.promptSignIn.set(() => promptSignIn);
  }, [promptSignIn]);

  React.useEffect(() => {
    authState.tryRestoreToken.set(() => tryRestoreToken.execute);
  }, [tryRestoreToken]);

  React.useEffect(() => {
    if (signInResponse?.type === "success") {
      const { code } = signInResponse.params;
      // we could then take this code, send to our API and verify it w/ CAS
      // and then send back some JWT for storage & bearer auth

      setToken.execute(code);
    }
  }, [signInResponse]);

  React.useEffect(() => {
    authState.isLoading.set(Boolean(isRequestingSignIn) || setToken.loading)
  }, [isRequestingSignIn, setToken.loading])

  return authState;
}