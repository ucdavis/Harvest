// typeahead input box that allows entering valid account numbers
// each one entered is displayed on a new line where percentages can be set
import { createRef, useState } from "react";

import { AsyncTypeahead, Highlighter, Typeahead } from "react-bootstrap-typeahead";

import { ProjectAccount } from "../types";

export const AccountsInput = () => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [searchResultAccounts, setSearchResultAccounts] = useState<
    ProjectAccount[]
  >([]);
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]);
  const typeaheadRef = createRef<AsyncTypeahead<ProjectAccount>>();

  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await fetch(`/financialaccount/get?account=${query}`);

    if (response.ok) {
      if (response.status === 204) {
        setSearchResultAccounts([]); // no content
      } else {
        const accountInfo: ProjectAccount = await response.json();

        setSearchResultAccounts([accountInfo]);
      }
    }
    setIsSearchLoading(false);
  };

  const onSelect = (selected: ProjectAccount[]) => {
    if (selected && selected.length === 1) {
      // TODO: check that account is not already in list
      setAccounts([...accounts, selected[0]]);

      // once we have made our selection reset the box so we can start over if desired
      (typeaheadRef.current as any)?.clear();
    }
  };

  return (
    <div>
      <AsyncTypeahead
        id="searchAccounts" // for accessibility
        ref={typeaheadRef}
        isLoading={isSearchLoading}
        minLength={9}
        placeholder="Enter account number.  ex: 3-ABC1234"
        labelKey={(option: ProjectAccount) =>
          `${option.number} (${option.name})`
        }
        filterBy={() => true} // don't filter on top of our search
        renderMenuItemChildren={(option, propsData, index) => (
          <div>
            <div>
              <Highlighter key="name" search={propsData.text || ""}>
                {option.name}
              </Highlighter>
            </div>
            <div>
              <Highlighter key="number" search={propsData.text || ""}>
                {option.number}
              </Highlighter>
            </div>
          </div>
        )}
        onSearch={onSearch}
        onChange={onSelect}
        options={searchResultAccounts}
      />
      <ul>
        {accounts.map((account) => (
          <li>{account.number}</li>
        ))}
      </ul>
    </div>
  );
};
