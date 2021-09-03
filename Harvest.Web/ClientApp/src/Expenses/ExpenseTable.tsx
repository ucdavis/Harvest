import React, { useMemo } from "react";
import { Column, TableState } from "react-table";
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
        Header: "Activity",
        accessor: (row) => row.activity,
      },
      {
        Header: "Type",
        accessor: (row) => row.type,
      },
      {
        Header: "Description",
        accessor: (row) => row.description,
      },
      {
        Header: "Markup",
        accessor: (row) =>
          row.type === "Other" ? (row.markup ? "Yes" : "No") : "N/A",
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
          row.createdOn === undefined
            ? "N/A"
            : new Date(row.createdOn).toLocaleDateString(),
      },
      {
        Header: "Delete",
        Cell: (data: any) => (
          <ShowFor roles={["FieldManager", "Supervisor"]}>
            <Button
              color="link"
              onClick={() => deleteExpense(data.row.original)}
              disabled={!canDeleteExpense}
            >
              Delete
            </Button>
          </ShowFor>
        ),
      },
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
