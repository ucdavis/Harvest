// typeahead input box that allows entering valid account numbers
// each one entered is displayed on a new line where percentages can be set
import React, { createRef, useState } from "react";

import { AsyncTypeahead, Highlighter } from "react-bootstrap-typeahead";
import { Col, Input, Row } from "reactstrap";

import { ProjectAccount } from "../types";

interface Props {
  accounts: ProjectAccount[];
  setAccounts: (accounts: ProjectAccount[]) => void;
}

export const AccountsInput = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [searchResultAccounts, setSearchResultAccounts] = useState<
    ProjectAccount[]
  >([]);
  const [error, setError] = useState<string>();

  const typeaheadRef = createRef<AsyncTypeahead<ProjectAccount>>();

  const { accounts, setAccounts } = props;
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
      const chosenAccount = selected[0];

      if (accounts.some((a) => a.number === chosenAccount.number)) {
        setError("Account already selected -- choose a different account");
        return;
      } else {
        setError(undefined);
      }

      if (accounts.length === 0) {
        // if it's our first account, default to 100%
        chosenAccount.percentage = 100.0;
      }

      setAccounts([...accounts, chosenAccount]);

      // once we have made our selection reset the box so we can start over if desired
      (typeaheadRef.current as any)?.clear();
      setSearchResultAccounts([]);
    }
  };

  const updateAccountPercentage = (
    account: ProjectAccount,
    percentage: number
  ) => {
    const idx = accounts.findIndex((a) => a.number === account.number);

    accounts[idx].percentage = percentage;

    setAccounts([...accounts]);

    if (accounts.reduce((prev, curr) => prev + curr.percentage, 0) !== 100.0) {
      setError("Total percentage must equal 100%");
    } else {
      setError(undefined);
    }
  };

  return (
    <div>
      {error && <span className="text-danger">{error}</span>}
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
      <Row md={6}>
        <Col>Account To Charge</Col>
        <Col>Percent %</Col>
      </Row>
      {accounts.map((account) => (
        <Row key={account.number} md={6}>
          <Col>{account.number}</Col>
          <Col>
            {" "}
            <Input
              type="text"
              name="percent"
              defaultValue={account.percentage}
              onChange={(e) =>
                updateAccountPercentage(
                  account,
                  parseFloat(e.target.value) || 0
                )
              }
            />
          </Col>
        </Row>
      ))}
      <div>DEBUG: {JSON.stringify(accounts)}</div>
    </div>
  );
};
