import React, { useState } from "react";

import { AsyncTypeahead, Highlighter } from "react-bootstrap-typeahead";

import { User } from "../types";

interface Props {
  user?: User;
  setUser: (user: User) => void;
}

export const SearchPerson = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [users, setUsers] = useState<User[]>([]);

  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await fetch(`/people/search?query=${query}`);

    if (response.ok) {
      if (response.status === 204) {
        setUsers([]); // no content means no match
      } else {
        const user: User = await response.json();

        setUsers([user]);
      }
    }
    setIsSearchLoading(false);
  };

  const onSelect = (selected: User[]) => {
    // TODO: need a way to clear out selected user -- perhaps we allow null/undefined to be passed up the line?
    if (selected && selected.length === 1) {
      // found our match
      props.setUser(selected[0]);
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
    />
  );
};
