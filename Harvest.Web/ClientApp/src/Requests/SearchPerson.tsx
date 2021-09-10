import React, { useState } from "react";

import {
  AsyncTypeahead,
  Highlighter,
  TypeaheadProps,
} from "react-bootstrap-typeahead";
import { useIsMounted } from "../Shared/UseIsMounted";

import { User } from "../types";

interface Props extends Pick<TypeaheadProps<string>, "onBlur"> {
  user?: User;
  onChange: (user: User | undefined) => void;
}

export const SearchPerson = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [users, onChanges] = useState<User[]>([]);

  const getIsMounted = useIsMounted();
  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await fetch(`/people/search?query=${query}`);

    if (response.ok) {
      if (response.status === 204) {
        getIsMounted() && onChanges([]); // no content means no match
      } else {
        const user: User = await response.json();

        getIsMounted() && onChanges([user]);
      }
    }
    getIsMounted() && setIsSearchLoading(false);
  };

  const onSelect = (selected: User[]) => {
    if (selected && selected.length === 1) {
      // found our match
      props.onChange(selected[0]);
    } else {
      props.onChange(undefined);
    }
  };

  return (
    <AsyncTypeahead
      id="searchPeople" // for accessibility
      isLoading={isSearchLoading}
      minLength={3}
      clearButton
      defaultSelected={props.user ? [props.user] : []}
      placeholder="Search for person by email or kerberos"
      labelKey={(option: User) => `${option.name} (${option.email})`}
      filterBy={() => true} // don't filter on top of our search
      renderMenuItemChildren={(option, propsData, index) => (
        <div>
          <div>
            <Highlighter key="name" search={propsData.text || ""}>
              {option.name}
            </Highlighter>
          </div>
          <div>
            <Highlighter key="email" search={propsData.text || ""}>
              {option.email}
            </Highlighter>
          </div>
        </div>
      )}
      onSearch={onSearch}
      onChange={onSelect}
      options={users}
      onBlur={props.onBlur}
    />
  );
};
