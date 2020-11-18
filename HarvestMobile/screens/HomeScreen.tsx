import { DrawerNavigationHelpers } from '@react-navigation/drawer/lib/typescript/src/types';
import * as React from 'react';
import { StyleSheet, Platform } from 'react-native';
import Autocomplete from 'react-native-autocomplete-input';

import { Text, View } from '../components/Themed';
import { Project } from '../types'
import { projects } from '../data';
import { TouchableOpacity } from 'react-native';


export default function HomeScreen() {
    const [value, setValue] = React.useState("");
    const [hideList, setHideList] = React.useState(false);

    const data = projects.filter(p => value && 
      (p.name.toLowerCase().includes(value) || p.description.toLowerCase().includes(value)));

    function handleSelectItem(item: Project) {
      setValue(item.name);
      setHideList(true);
      console.log(item);
    }

    function handleChangeText(text: string) {
      setValue(text);
      setHideList(false);
    }
    
    return (
    <View style={styles.container}>
      <Autocomplete
          autoCapitalize="none"
          autoCorrect={false}
          containerStyle={styles.autocompleteContainer}
          hideResults={hideList}
          data={data}
          value={value}
          defaultValue={""}
          onChangeText={handleChangeText}
          placeholder="Choose Project"
          keyExtractor={(item: Project) => item.name}
          renderItem={({ item }) => (
            <TouchableOpacity onPress={() => handleSelectItem(item)}>
              <Text style={styles.itemText}>
                {item.name} ({item.description})
              </Text>
            </TouchableOpacity>
          )}
        />
    </View>
  );
}


const styles = StyleSheet.create({
  container: {
    backgroundColor: '#F5FCFF',
    flex: 1,
    paddingTop: 25
  },
  // 
  autocompleteContainer: {
    marginLeft: 10,
    marginRight: 10
  },
  itemText: {
    fontSize: 15,
    margin: 2
  },
  descriptionContainer: {
    // `backgroundColor` needs to be set otherwise the
    // autocomplete input will disappear on text input.
    backgroundColor: '#F5FCFF',
    marginTop: Platform.OS === 'android' ? 25 : 8
  },
  infoText: {
    textAlign: 'center'
  },
  titleText: {
    fontSize: 18,
    fontWeight: '500',
    marginBottom: 10,
    marginTop: 10,
    textAlign: 'center'
  },
  directorText: {
    color: 'grey',
    fontSize: 12,
    marginBottom: 10,
    textAlign: 'center'
  },
  openingText: {
    textAlign: 'center'
  }
});