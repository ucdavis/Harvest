import React, { useMemo } from "react";
import { Cell, Column, TableState } from "react-table";
import { Button } from "reactstrap";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Expense } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { ShowFor } from "../Shared/ShowFor";

interface Props {
  expenses: Expense[];
  deleteExpense: (expense: Expense) => void;
  canDeleteExpense: boolean;
}

export const ExpenseTable = (props: Props) => {
  const expenseData = useMemo(() => props.expenses, [props.expenses]);

  const { deleteExpense, canDeleteExpense } = props;

  const columns: Column<Expense>[] = useMemo(
    () => [
      {
        Header: "Type",
        accessor: (row) => row.type,
      },
      {
        Header: "Description",
        accessor: (row) => row.description,
      },
      {
        Header: "Quantity",
        accessor: (row) => row.quantity,
      },
      {
        Header: "Rate",
        accessor: (row) => "$" + formatCurrency(row.price),
      },
      {
        Header: "Total",
        accessor: (row) => "$" + formatCurrency(row.total),
      },
      {
        Header: "Entered by",
        accessor: (row) => row.createdBy?.name,
      },
      {
        Header: "Entered on",
        accessor: (row) =>
          `${row.createdOn} ${
            row.createdOn ? new Date(row.createdOn).toLocaleDateString() : ""
          }`, //This can't be null in the db
        Cell: (data: Cell<Expense>) =>
          data.row.original.createdOn
            ? new Date(data.row.original.createdOn).toLocaleDateString()
            : "N/A",
      },
      {
        Header: "Approved",
        accessor: (row) => (row.approved ? "Yes" : "No"),
      },
      {
        Header: "Approved by",
        accessor: (row) => row.approvedBy?.name,
      },
      {
        Header: "Approved on",
        accessor: (row) =>
          row.approvedOn
            ? new Date(row.approvedOn).toLocaleDateString()
            : "N/A",
      },

      ...(canDeleteExpense
        ? [
            {
              Header: "Delete",
              Cell: (data: any) => (
                <ShowFor roles={["FieldManager", "Supervisor"]}>
                  <Button
                    color="link"
                    onClick={() => deleteExpense(data.row.original)}
                  >
                    Delete
                  </Button>
                </ShowFor>
              ),
            },
          ]
        : []),
    ],
    [deleteExpense, canDeleteExpense]
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "Activity" }, { id: "Type" }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={expenseData}
      initialState={initialState}
    />
  );
};
