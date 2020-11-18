import * as React from 'react';
import { createState, useState } from '@hookstate/core';
import * as SecureStore from 'expo-secure-store';

export const globalAuthState = createState<AuthState>({
  isLoading: true,
  isSignout: false,
  userToken: null,
});

export type AuthState = {
  userToken: string | null;
  isLoading: boolean;
  isSignout: boolean;
};

export async function signIn(token: string) {
  await SecureStore.setItemAsync('userToken', token);
  globalAuthState.isSignout.set(false);
  globalAuthState.userToken.set(token);
}

export async function signOut() {
  try {
    await SecureStore.deleteItemAsync('userToken');
  }
  catch (e) {

  }
  globalAuthState.isSignout.set(true);
  globalAuthState.userToken.set(null);
}
