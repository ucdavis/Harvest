import { useState } from '@hookstate/core';
import * as React from 'react';
import { Button, StyleSheet, TextInput } from 'react-native';
import { useTranslation } from 'react-i18next';

import { globalAuthState } from '../hooks/useAuth';
import { Text, View } from '../components/Themed';

export default function SignInScreen() {
  const authState = useState(globalAuthState);

  const { t } = useTranslation();

  return (
    <View style={styles.container}>
      <Text style={styles.title}>{t("common:SignIn")}</Text>
      <View style={styles.separator} lightColor="#eee" darkColor="rgba(255,255,255,0.1)" />
      {/* <Button disabled={authState.isLoading.get() || Boolean(authState.userToken.get())} title="Sign in" onPress={() => authState.promptSignIn.get()() } /> */}
      <Button title="Sign in" onPress={() => authState.promptSignIn.get()()} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
  },
  separator: {
    marginVertical: 30,
    height: 1,
    width: '80%',
  },
});
