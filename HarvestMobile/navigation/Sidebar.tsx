import * as React from 'react';
import { FlatList, TouchableOpacity, StyleSheet, Image, Text, Button } from 'react-native';
import { View } from '../components/Themed';
import { Ionicons, FontAwesome5 } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { globalAuthState } from '../hooks/useAuth';
import { useState } from '@hookstate/core';

type ItemProps = {
  item: RouteData
}

type RouteData = {
  name: string;
  icon: string;
};

function Item(props: ItemProps) {
  const { item } = props;
  const navigation = useNavigation();

  return (
    <TouchableOpacity style={styles.listItem} onPress={() => navigation.navigate(item.name)}>
      <Ionicons name={item.icon} size={32} />
      <Text style={styles.title}>{item.name}</Text>
    </TouchableOpacity>
  );
}

export default function Sidebar() {

  const authState = useState(globalAuthState);

  const routes: RouteData[] = authState.userToken.get() == null ? [
    { name: "SignIn", icon: "ios-log-in" },
  ] : [
      { name: "Home", icon: "ios-home" },
      { name: "TimeSheets", icon: "ios-settings" },
    ];

  return (
    <View style={styles.container}>
      {/* <Image source={require("../assets/images/favicon.png")} style={styles.profileImg} /> */}
      <FontAwesome5 name={"tractor"} size={64} style={styles.profileImg} />
      {authState.userToken.get() && (<>
        <Text style={{ fontWeight: "bold", fontSize: 16, marginTop: 10 }}>John Doe</Text>
        <Button title="Sign out" onPress={authState.signOut.get()} />
      </>)}
      <View style={styles.sidebarDivider}></View>
      <FlatList<RouteData>
        style={{ width: "100%", marginLeft: 30 }}
        data={routes}
        renderItem={({ item }) => <Item item={item} />}
        keyExtractor={(item) => item.name}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  listItem: {
    height: 60,
    alignItems: "center",
    flexDirection: "row",
  },
  profileImg: {
    width: 80,
    height: 80,
    borderRadius: 40,
    marginTop: 30
  },
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    marginLeft: 20
  },
  sidebarDivider: {
    height: 1,
    width: "100%",
    backgroundColor: "lightgray",
    marginVertical: 10
  }
});
