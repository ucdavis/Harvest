// typeahead input box that allows entering valid account numbers
// each one entered is displayed on a new line where percentages can be set
import React, { createRef, useEffect, useState } from "react";

import { AsyncTypeahead, Highlighter } from "react-bootstrap-typeahead";
import { Col, Input, Row } from "reactstrap";

import { ProjectAccount } from "../types";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMinusCircle } from "@fortawesome/free-solid-svg-icons";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  accounts: ProjectAccount[];
  setAccounts: (accounts: ProjectAccount[]) => void;
  setDisabled: (disabled: boolean) => void;
}

export const AccountsInput = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [searchResultAccounts, setSearchResultAccounts] = useState<
    ProjectAccount[]
  >([]);
  const [error, setError] = useState<string>();

  const typeaheadRef = createRef<AsyncTypeahead<ProjectAccount>>();

  const { accounts, setAccounts } = props;

  const getIsMounted = useIsMounted();
  useEffect(() => {
    let total = 0;
    for (let i = 0; i < accounts.length; i++) {
      total += accounts[i].percentage;
    }

    // some will return true if there is an instance that satifies the function given
    const hasZeroPercent = accounts.some((account) => account.percentage === 0);

    if (total === 100 && !hasZeroPercent) {
      props.setDisabled(false);
      setError(undefined);
    } else {
      if (total !== 100.0 && accounts.length > 0) {
        setError("Total percentage must equal 100%");
      } else if (hasZeroPercent) {
        setError("All accounts must be above 0%");
      } else {
        setError(undefined);
      }
      props.setDisabled(true);
    }
  }, [accounts, props]);

  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await authenticatedFetch(`/api/financialaccount/get?account=${query}`);

    if (response.ok) {
      if (response.status === 204) {
        getIsMounted() && setSearchResultAccounts([]); // no content
      } else {
        const accountInfo: ProjectAccount = await response.json();

        getIsMounted() && setSearchResultAccounts([accountInfo]);
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

  const removeAccount = (account: ProjectAccount) => {
    const otherAccounts = props.accounts.filter(
      (a) => a.number !== account.number
    );

    props.setAccounts(otherAccounts);
  };

  const updateAccountPercentage = (
    account: ProjectAccount,
    percentage: number
  ) => {
    const idx = accounts.findIndex((a) => a.number === account.number);

    if (percentage < 0) {
      accounts[idx].percentage = 0;
    } else {
      accounts[idx].percentage = percentage;
    }

    setAccounts([...accounts]);
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
      {accounts.length > 0 && (
        <Row className="approval-row approval-row-title">
          <Col md={6}>Account To Charge</Col>
          <Col md={4}>Percent %</Col>
        </Row>
      )}

      {accounts.map((account) => (
        <Row className="approval-row align-items-center" key={account.number}>
          <Col md={6}>
            <b>{account.number}</b>
            <br />
            <small>{account.name}</small>
            <br />
            Account Manager:{" "}
            <a href={`mailto:${account.accountManagerEmail}`}>
              {account.accountManagerName}
            </a>
          </Col>
          <Col md={3}>
            {" "}
            <Input
              type="text"
              name="percent"
              value={account.percentage}
              onChange={(e) =>
                updateAccountPercentage(
                  account,
                  parseFloat(e.target.value) || 0
                )
              }
            />
          </Col>
          <Col md={2}>
            <button
              className="btn btn-link btn-fa"
              onClick={() => removeAccount(account)}
            >
              <FontAwesomeIcon icon={faMinusCircle} />
            </button>
          </Col>
        </Row>
      ))}
      {accounts.length > 0 && (
        <Row>
          <Col className="col-md-4 offset-md-6">
            <b>
              Total Percent:{" "}
              {accounts.reduce((prev, curr) => prev + curr.percentage, 0)}%
            </b>
          </Col>
        </Row>
      )}
      {error && <span className="text-danger">{error}</span>}
      <p className="discreet mt-5">
        Please check with your account manager for each account above before
        approving
      </p>
    </div>
  );
};
