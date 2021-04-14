import React, { useEffect, useState } from "react";

import { Label } from "reactstrap";
import { AsyncTypeahead, Highlighter } from "react-bootstrap-typeahead";

import { User } from "../types";

interface Props {
  user?: User;
}

export const SearchPerson = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [users, setUsers] = useState<User[]>([]);

  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await fetch(`/people/search?query=${query}`);

    if (response.ok) {
      const user: User = await response.json();

      setUsers([user]);
    }
    setIsSearchLoading(false);
  };

  const onSelect = (selected: any) => {};

  return (
    <div>
      <Label for="searchPeople">Search</Label>
      <AsyncTypeahead
        id="searchPeople" // for accessibility
        isLoading={isSearchLoading}
        minLength={3}
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
    </div>
  );
};
