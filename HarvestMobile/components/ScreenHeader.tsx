import { Ionicons } from "@expo/vector-icons";
import { DrawerActions, useNavigation } from "@react-navigation/native";
import { StackHeaderTitleProps } from "@react-navigation/stack";
import React from "react";
import { View, Text } from "react-native";
import { TouchableOpacity } from "react-native-gesture-handler";
import { styles } from "../styles";

type ScreenHeaderProps = {
  name: string;
};

export const ScreenHeader = (props: ScreenHeaderProps)=> {
  const { name } = props;

  const navigation = useNavigation();

  return (
    <View style={styles.header}>
      <TouchableOpacity onPress={()=>navigation.dispatch(DrawerActions.openDrawer())}>
        <Ionicons name="ios-menu" size={32} />
      </TouchableOpacity>
      <Text>{name}</Text>
      <Text style={{width:50}}></Text>
    </View>
  );
}