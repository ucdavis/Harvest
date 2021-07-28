import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Expense } from "../types";
import { ExpenseTable } from "./ExpenseTable";
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";
import { ShowFor } from "../Shared/ShowFor";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

export const UnbilledExpensesContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [selectedExpense, setSelectedExpense] = useState<Expense | null>(null);

  useEffect(() => {
    // get unbilled expenses for the project
    if (projectId === undefined) return;

    const cb = async () => {
      const response = await fetch(`/Expense/GetUnbilled/${projectId}`);

      if (response.ok) {
        const expenses: Expense[] = await response.json();

        setExpenses(expenses);
      }
    };

    cb();
  }, [projectId]);

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
          <h1>Un-billed Expenses</h1>
        </div>
        <div className="col text-right">
          <ShowFor roles={["FieldManager", "Supervisor", "Worker"]}>
            <Link
              to={`/expense/entry/${projectId}`}
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
