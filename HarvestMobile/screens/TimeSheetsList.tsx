import { useNavigation } from '@react-navigation/native';
import * as React from 'react';
import { FlatList } from 'react-native';

import { Row, Separator } from '../components/Row';
import { timeSheets } from '../data';

export default function TimeSheetsList() {
  const navigation = useNavigation();

  return (
    <FlatList
      data={timeSheets}
      keyExtractor={item => {
        return `${item.id}`;
      }}
      renderItem={({ item }) => {
        const projectName = item.project.name;
        const projectDescription = item.project.description;

        return (
          <Row
            /*image={{ uri: item.picture.thumbnail }}*/
            title={projectName}
            subtitle={projectDescription}
            onPress={() => navigation.navigate('TimeSheetDetails', { timeSheet: item })}
          />
        );
      }}
      ItemSeparatorComponent={Separator}
      ListHeaderComponent={() => <Separator />}
      ListFooterComponent={() => <Separator />}
      contentContainerStyle={{ paddingVertical: 20 }}
    />
  );
};
