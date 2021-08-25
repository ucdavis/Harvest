import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Expense, ExpenseQueryParams } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";
import { ShowFor } from "../Shared/ShowFor";
import { formatCurrency } from "../Util/NumberFormatting";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

interface Props {
  newExpenseCount?: number; // just used to force a refresh of data when new expenses are created outside of this component
}

export const UnbilledExpensesContainer = (props: Props) => {
  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [total, setTotal] = useState(0);
  const [selectedExpense, setSelectedExpense] = useState<Expense | null>(null);

  useEffect(() => {
    // get unbilled expenses for the project
    if (projectId === undefined) return;

    const cb = async () => {
      const response = await fetch(`/Expense/GetUnbilled/${projectId}`);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        setExpenses(expenses);
        setTotal(expenses.reduce((acc, cur) => acc + cur.total, 0));
      }
    };

    cb();
  }, [projectId, props.newExpenseCount]);

  // This function closes the modal and deletes the entry from the db
  const confirmModal = async (expenseId: number) => {
    const response = await fetch(`/Expense/Delete?expenseId=${expenseId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      let expensesCopy = [...expenses];
      const index = expensesCopy.findIndex(
        (element) => element.id === expenseId
      );
      expensesCopy.splice(index, 1);

      setExpenses(expensesCopy);
      setTotal(expensesCopy.reduce((acc, cur) => acc + cur.total, 0));
      setSelectedExpense(null);
    }
  };

  if (expenses.length === 0) {
    return <h3>No un-billed expenses found</h3>;
  }

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>Un-billed Expenses <small>(${formatCurrency(total)} total)</small></h1>
        </div>
        <div className="col text-right">
          <ShowFor roles={["FieldManager", "Supervisor", "Worker"]}>
            <Link
              to={`/expense/entry/${projectId}?${ExpenseQueryParams.ReturnOnSubmit}=true`}
              className="btn btn btn-primary "
            >
              Enter New <FontAwesomeIcon icon={faPlus} />
            </Link>
          </ShowFor>
        </div>

        <Modal isOpen={selectedExpense !== null}>
          <ModalHeader>Modal title</ModalHeader>
          <ModalBody>
            Are you sure you want to remove this unbilled expense?
          </ModalBody>
          <ModalFooter>
            <Button
              color="success"
              onClick={() =>
                selectedExpense !== null
                  ? confirmModal(selectedExpense.id)
                  : null
              }
            >
              Confirm
            </Button>{" "}
            <Button color="link" onClick={() => setSelectedExpense(null)}>
              Cancel
            </Button>
          </ModalFooter>
        </Modal>
      </div>

      <ExpenseTable
        expenses={expenses}
        setSelectedExpense={setSelectedExpense}
      ></ExpenseTable>
    </div>
  );
};
